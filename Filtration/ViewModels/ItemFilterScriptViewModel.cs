using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Filtration.Common.Services;
using Filtration.Common.ViewModels;
using Filtration.Interface;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Commands;
using Filtration.ObjectModel.Commands.ItemFilterScript;
using Filtration.Parser.Interface.Services;
using Filtration.Properties;
using Filtration.Services;
using Filtration.ViewModels.Factories;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NLog;

namespace Filtration.ViewModels
{
    internal interface IItemFilterScriptViewModel : IEditableDocument
    {
        IItemFilterScript Script { get; }
        IItemFilterBlockViewModelBase SelectedBlockViewModel { get; set; }
        IItemFilterCommentBlockViewModel CommentBlockBrowserBrowserSelectedBlockViewModel { get; set; }
        IEnumerable<IItemFilterCommentBlockViewModel> ItemFilterCommentBlockViewModels { get; }
        ObservableCollection<string> CustomSoundsAvailable { get; }
        Predicate<IItemFilterBlockViewModel> BlockFilterPredicate { get; set; }
        
        bool ShowAdvanced { get; }
        string Description { get; set; }
        string DisplayName { get; }
        
        void Initialise(IItemFilterScript itemFilterScript, bool newScript);
        void RemoveDirtyFlag();
        void SetDirtyFlag();
        bool HasSelectedEnabledBlock();
        bool HasSelectedDisabledBlock();
        bool HasSelectedCommentBlock();

        RelayCommand AddBlockCommand { get; }
        RelayCommand AddSectionCommand { get; }
        RelayCommand DisableBlockCommand { get; }
        RelayCommand EnableBlockCommand { get; }
        RelayCommand DisableSectionCommand { get; }
        RelayCommand EnableSectionCommand { get; }
        RelayCommand ExpandSectionCommand { get; }
        RelayCommand CollapseSectionCommand { get; }
        RelayCommand DeleteBlockCommand { get; }
        RelayCommand MoveBlockUpCommand { get; }
        RelayCommand MoveBlockDownCommand { get; }
        RelayCommand MoveBlockToTopCommand { get; }
        RelayCommand MoveBlockToBottomCommand { get;}
        RelayCommand CopyBlockCommand { get; }
        RelayCommand PasteBlockCommand { get; }
        RelayCommand CopyBlockStyleCommand { get; }
        RelayCommand PasteBlockStyleCommand { get; }
        RelayCommand ExpandAllBlocksCommand { get; }
        RelayCommand CollapseAllBlocksCommand { get; }
        RelayCommand ExpandAllSectionsCommand { get; }
        RelayCommand CollapseAllSectionsCommand { get; }
        RelayCommand<bool> ToggleShowAdvancedCommand { get; }
        RelayCommand ClearFilterCommand { get; }

        void AddCommentBlock(IItemFilterBlockViewModelBase targetBlockViewModel);
        void AddBlock(IItemFilterBlockViewModelBase targetBlockViewModel);
        void CopyBlock(IItemFilterBlockViewModelBase targetBlockViewModel);
        void CopyBlockStyle(IItemFilterBlockViewModel targetBlockViewModel);
        void PasteBlock(IItemFilterBlockViewModelBase targetBlockViewModel);
        void PasteBlockStyle(IItemFilterBlockViewModel targetBlockViewModel);
        void DeleteBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase);
        void MoveBlockToTop(IItemFilterBlockViewModelBase targetBlockViewModelBase);
        void MoveBlockUp(IItemFilterBlockViewModelBase targetBlockViewModelBase);
        void MoveBlockDown(IItemFilterBlockViewModelBase targetBlockViewModelBase);
        void MoveBlockToBottom(IItemFilterBlockViewModelBase targetBlockViewModelBase);
        void ToggleSection(IItemFilterCommentBlockViewModel targetCommentBlockViewModelBase, bool deferPropertyChanged = false);
    }

    internal class ItemFilterScriptViewModel : PaneViewModel, IItemFilterScriptViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IItemFilterBlockBaseViewModelFactory _itemFilterBlockBaseViewModelFactory;
        private readonly IItemFilterBlockTranslator _blockTranslator;
        private readonly IItemFilterScriptTranslator _scriptTranslator;
        private readonly IAvalonDockWorkspaceViewModel _avalonDockWorkspaceViewModel;
        private readonly IItemFilterPersistenceService _persistenceService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IClipboardService _clipboardService;
        private readonly IBlockGroupHierarchyBuilder _blockGroupHierarchyBuilder;

        private bool _isDirty;
        private IItemFilterBlockViewModelBase _selectedBlockViewModel;
        private IItemFilterCommentBlockViewModel _sectionBrowserSelectedBlockViewModel;
        private readonly ObservableCollection<IItemFilterBlockViewModelBase> _itemFilterBlockViewModels;
        private Predicate<IItemFilterBlockViewModel> _blockFilterPredicate;
        private ICommandManager _scriptCommandManager;

        private ObservableCollection<string> _customSoundsAvailable;

        public ItemFilterScriptViewModel(IItemFilterBlockBaseViewModelFactory itemFilterBlockBaseViewModelFactory,
                                         IItemFilterBlockTranslator blockTranslator,
                                         IItemFilterScriptTranslator scriptTranslator,
                                         IAvalonDockWorkspaceViewModel avalonDockWorkspaceViewModel,
                                         IItemFilterPersistenceService persistenceService,
                                         IMessageBoxService messageBoxService,
                                         IClipboardService clipboardService,
                                         IBlockGroupHierarchyBuilder blockGroupHierarchyBuilder)
        {
            _itemFilterBlockBaseViewModelFactory = itemFilterBlockBaseViewModelFactory;
            _blockTranslator = blockTranslator;
            _scriptTranslator = scriptTranslator;
            _avalonDockWorkspaceViewModel = avalonDockWorkspaceViewModel;
            _avalonDockWorkspaceViewModel.ActiveDocumentChanged += OnActiveDocumentChanged;
            _persistenceService = persistenceService;
            _messageBoxService = messageBoxService;
            _clipboardService = clipboardService;
            _blockGroupHierarchyBuilder = blockGroupHierarchyBuilder;
            _itemFilterBlockViewModels = new ObservableCollection<IItemFilterBlockViewModelBase>();
            _showAdvanced = Settings.Default.ShowAdvanced;

            _avalonDockWorkspaceViewModel.ActiveDocumentChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(IsActiveDocument));
            };

            ToggleShowAdvancedCommand = new RelayCommand<bool>(OnToggleShowAdvancedCommand);
            ClearFilterCommand = new RelayCommand(OnClearFilterCommand, () => BlockFilterPredicate != null);
            CloseCommand = new RelayCommand(async () => await OnCloseCommand());
            DeleteBlockCommand = new RelayCommand(OnDeleteBlockCommand, () => SelectedBlockViewModel != null);
            MoveBlockToTopCommand = new RelayCommand(OnMoveBlockToTopCommand, () => SelectedBlockViewModel != null && ItemFilterBlockViewModels.IndexOf(SelectedBlockViewModel) > 0);
            MoveBlockUpCommand = new RelayCommand(OnMoveBlockUpCommand, () => SelectedBlockViewModel != null && ItemFilterBlockViewModels.IndexOf(SelectedBlockViewModel) > 0);
            MoveBlockDownCommand = new RelayCommand(OnMoveBlockDownCommand, () => SelectedBlockViewModel != null && ItemFilterBlockViewModels.IndexOf(SelectedBlockViewModel) < ItemFilterBlockViewModels.Count);
            MoveBlockToBottomCommand = new RelayCommand(OnMoveBlockToBottomCommand, () => SelectedBlockViewModel != null && ItemFilterBlockViewModels.IndexOf(SelectedBlockViewModel) < ItemFilterBlockViewModels.Count);
            AddBlockCommand = new RelayCommand(OnAddBlockCommand);
            AddSectionCommand = new RelayCommand(OnAddCommentBlockCommand, () => SelectedBlockViewModel != null);
            DisableBlockCommand = new RelayCommand(OnDisableBlockCommand, HasSelectedEnabledBlock);
            EnableBlockCommand = new RelayCommand(OnEnableBlockCommand, HasSelectedDisabledBlock);
            DisableSectionCommand = new RelayCommand(OnDisableSectionCommand, HasSelectedCommentBlock);
            EnableSectionCommand = new RelayCommand(OnEnableSectionCommand, HasSelectedCommentBlock);
            ExpandSectionCommand = new RelayCommand(OnExpandSectionCommand, HasSelectedCommentBlock);
            CollapseSectionCommand = new RelayCommand(OnCollapseSectionCommand, HasSelectedCommentBlock);
            CopyBlockCommand = new RelayCommand(OnCopyBlockCommand, () => SelectedBlockViewModel != null);
            CopyBlockStyleCommand = new RelayCommand(OnCopyBlockStyleCommand, () => SelectedBlockViewModel != null);
            PasteBlockCommand = new RelayCommand(OnPasteBlockCommand, () => SelectedBlockViewModel != null);
            PasteBlockStyleCommand = new RelayCommand(OnPasteBlockStyleCommand, () => SelectedBlockViewModel != null);
            ExpandAllBlocksCommand = new RelayCommand(OnExpandAllBlocksCommand);
            CollapseAllBlocksCommand = new RelayCommand(OnCollapseAllBlocksCommand);
            ExpandAllSectionsCommand = new RelayCommand(ExpandAllSections);
            CollapseAllSectionsCommand = new RelayCommand(CollapseAllSections);

            var icon = new BitmapImage();
            icon.BeginInit();
            icon.UriSource = new Uri("pack://application:,,,/Filtration;component/Resources/Icons/script_icon.png");
            icon.EndInit();
            IconSource = icon;

            _customSoundsAvailable = new ObservableCollection<string>();
            
            var poeFolderFiles = Directory.GetFiles(persistenceService.DefaultPathOfExileDirectory() + "\\").Where(
                s => s.EndsWith(".mp3")
                || s.EndsWith(".wav")
                || s.EndsWith(".wma")
                || s.EndsWith(".3gp")
                || s.EndsWith(".aag")
                || s.EndsWith(".m4a")
                || s.EndsWith(".ogg")
            ).OrderBy(f => f);

            foreach(var file in poeFolderFiles)
            {
                _customSoundsAvailable.Add(file.Replace(persistenceService.DefaultPathOfExileDirectory() + "\\", ""));
            }
        }

        public void Initialise(IItemFilterScript itemFilterScript, bool newScript)
        {
            ItemFilterBlockViewModels.Clear();

            Script = itemFilterScript;
            _scriptCommandManager = Script.CommandManager;
            AddItemFilterBlockViewModels(Script.ItemFilterBlocks, -1);

            foreach(var block in Script.ItemFilterBlocks.OfType<IItemFilterBlock>())
            {
                foreach (var customSoundBlockItem in block.BlockItems.OfType<CustomSoundBlockItem>())
                {
                    if (!string.IsNullOrWhiteSpace(customSoundBlockItem.Value) && CustomSoundsAvailable.IndexOf(customSoundBlockItem.Value) < 0)
                    {
                        CustomSoundsAvailable.Add(customSoundBlockItem.Value);
                    }
                }
            }
            
            Script.ItemFilterBlocks.CollectionChanged += ItemFilterBlocksOnCollectionChanged;
            _customSoundsAvailable.CollectionChanged += CustomSoundsAvailableOnCollectionChanged;

            _filenameIsFake = newScript;

            if (newScript)
            {
                Script.FilePath = "Untitled.filter";
            }
            
            Title = Filename;
            ContentId = "ScriptContentId";

            CollapseAllSections();
        }

        private void ItemFilterBlocksOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    AddItemFilterBlockViewModels(notifyCollectionChangedEventArgs.NewItems.Cast<IItemFilterBlockBase>(), notifyCollectionChangedEventArgs.NewStartingIndex);
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    RemoveItemFilterBlockviewModels(notifyCollectionChangedEventArgs.OldItems.Cast<IItemFilterBlockBase>());
                    break;
                }
                default:
                {
                    Debugger.Break(); // Unhandled NotifyCollectionChangedAction
                    break;
                }
            }

            RaisePropertyChanged(nameof(ViewItemFilterBlockViewModels));
        }

        private void CustomSoundsAvailableOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RaisePropertyChanged(nameof(CustomSoundsAvailable));
        }

        private void AddItemFilterBlockViewModels(IEnumerable<IItemFilterBlockBase> itemFilterBlocks, int addAtIndex)
        {
            var firstNewViewModel = true;

            foreach (var itemFilterBlock in itemFilterBlocks)
            {
                var vm = _itemFilterBlockBaseViewModelFactory.Create(itemFilterBlock);
                vm.Initialise(itemFilterBlock, this);
                vm.BlockBecameDirty += OnBlockBecameDirty;

                if (addAtIndex == -1)
                {
                    ItemFilterBlockViewModels.Add(vm);
                }
                else
                {
                    ItemFilterBlockViewModels.Insert(addAtIndex, vm);
                }

                if (firstNewViewModel)
                {
                    SelectedBlockViewModel = vm;
                    firstNewViewModel = false;
                }

                var itemBlock = itemFilterBlock as IItemFilterBlock;
                if (itemBlock != null)
                {
                    foreach (var customSoundBlockItem in itemBlock.BlockItems.OfType<CustomSoundBlockItem>())
                    {
                        if (!string.IsNullOrWhiteSpace(customSoundBlockItem.Value) && CustomSoundsAvailable.IndexOf(customSoundBlockItem.Value) < 0)
                        {
                            CustomSoundsAvailable.Add(customSoundBlockItem.Value);
                        }
                    }
                }
            }
        }

        private void RemoveItemFilterBlockviewModels(IEnumerable<IItemFilterBlockBase> itemFilterBlocks)
        {
            foreach (var itemFilterBlock in itemFilterBlocks)
            {
                var itemFilterBlockViewModel = ItemFilterBlockViewModels.FirstOrDefault(f => f.BaseBlock == itemFilterBlock);
                if (itemFilterBlockViewModel == null)
                {
                    throw new InvalidOperationException("Item Filter Block removed from model but does not exist in view model!");
                }

                ItemFilterBlockViewModels.Remove(itemFilterBlockViewModel);
                if (SelectedBlockViewModel == itemFilterBlockViewModel)
                {
                    SelectedBlockViewModel = null;
                }
            }
        }

        public RelayCommand<bool> ToggleShowAdvancedCommand { get; }
        public RelayCommand ClearFilterCommand { get; }
        public RelayCommand CloseCommand { get; }
        public RelayCommand DeleteBlockCommand { get; }
        public RelayCommand MoveBlockToTopCommand { get; }
        public RelayCommand MoveBlockUpCommand { get; }
        public RelayCommand MoveBlockDownCommand { get; }
        public RelayCommand MoveBlockToBottomCommand { get; }
        public RelayCommand AddBlockCommand { get; }
        public RelayCommand AddSectionCommand { get; }
        public RelayCommand EnableBlockCommand { get; }
        public RelayCommand DisableBlockCommand { get; }
        public RelayCommand DisableSectionCommand { get; }
        public RelayCommand EnableSectionCommand { get; }
        public RelayCommand ExpandSectionCommand { get; }
        public RelayCommand CollapseSectionCommand { get; }
        public RelayCommand CopyBlockCommand { get; }
        public RelayCommand CopyBlockStyleCommand { get; }
        public RelayCommand PasteBlockCommand { get; }
        public RelayCommand PasteBlockStyleCommand { get; }
        public RelayCommand ExpandAllBlocksCommand { get; }
        public RelayCommand CollapseAllBlocksCommand { get; }
        public RelayCommand ExpandAllSectionsCommand { get; }
        public RelayCommand CollapseAllSectionsCommand { get; }

        public bool IsActiveDocument
        {
            get
            {
                var isActiveDocument = _avalonDockWorkspaceViewModel.ActiveScriptViewModel == this;
                Debug.WriteLine($"IsActiveDocument: {isActiveDocument}");

                return isActiveDocument;

            }
        }

        public ObservableCollection<string> CustomSoundsAvailable
        {
            get => _customSoundsAvailable;
            set
            {
                _customSoundsAvailable = value;
                RaisePropertyChanged();
            }
        }

        public ICollectionView ViewItemFilterBlockViewModels
        {
            get
            {
                ICollectionView filteredView = CollectionViewSource.GetDefaultView(ItemFilterBlockViewModels);
                filteredView.Filter = BlockFilter;

                IItemFilterBlockViewModelBase previousBlock = null;
                foreach (IItemFilterBlockViewModelBase block in filteredView)
                {
                    if (block is IItemFilterBlockViewModel)
                    {
                        if (previousBlock is IItemFilterCommentBlockViewModel)
                        {
                            (previousBlock as IItemFilterCommentBlockViewModel).HasChild = true;
                        }
                    }
                    else if (block is IItemFilterCommentBlockViewModel)
                    {
                        (block as IItemFilterCommentBlockViewModel).HasChild = false;
                    }
                    previousBlock = block;
                }

                ICollectionView visibilityView = CollectionViewSource.GetDefaultView(filteredView.Cast<IItemFilterBlockViewModelBase>());
                visibilityView.Filter = BlockVisibilityFilter;

                return visibilityView;
            }
        }

        public ObservableCollection<IItemFilterBlockViewModelBase> ItemFilterBlockViewModels
        {
            get
            {
                return _itemFilterBlockViewModels;
            }
        }

        private bool BlockFilter(object item)
        {
            if (!(item is IItemFilterBlockViewModelBase)) return false;
            if (item is IItemFilterCommentBlockViewModel) return true;
            var blockViewModel = item as IItemFilterBlockViewModel;

            if (BlockFilterPredicate != null)
            {
                return BlockFilterPredicate(blockViewModel) && ShowBlockBasedOnAdvanced(blockViewModel);
            }

            return ShowBlockBasedOnAdvanced(blockViewModel);
        }

        private bool BlockVisibilityFilter(object item)
        {
            return (bool)(item as IItemFilterBlockViewModelBase)?.IsVisible;
        }

        private bool ShowBlockBasedOnAdvanced(IItemFilterBlockViewModel blockViewModel)
        {
            if (ShowAdvanced)
            {
                return true;
            }

            if (blockViewModel.Block.BlockGroup == null)
            {
                return true;
            }

            return !blockViewModel.Block.BlockGroup.Advanced;

        }

        public Predicate<IItemFilterBlockViewModel> BlockFilterPredicate
        {
            get => _blockFilterPredicate;
            set
            {
                _blockFilterPredicate = value;
                RaisePropertyChanged(nameof(ViewItemFilterBlockViewModels));
            }
        }

        public IEnumerable<IItemFilterCommentBlockViewModel> ItemFilterCommentBlockViewModels => ItemFilterBlockViewModels.OfType<IItemFilterCommentBlockViewModel>();

        public bool IsScript => true;
        public bool IsTheme => false;

        public string Description
        {
            get => Script.Description;
            set => _scriptCommandManager.ExecuteCommand(new SetScriptDescriptionCommand(Script, value));
        }

        public bool ShowAdvanced
        {
            get => _showAdvanced;
            private set
            {
                _showAdvanced = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ViewItemFilterBlockViewModels));
                Settings.Default.ShowAdvanced = value;
            }
        }

        public bool HasSelectedEnabledBlock()
        {
            var selectedBlockViewModel = SelectedBlockViewModel as IItemFilterBlockViewModel;
            if (selectedBlockViewModel == null) return false;

            return HasSelectedBlock() && selectedBlockViewModel.BlockEnabled;
        }

        public bool HasSelectedDisabledBlock()
        {
            var selectedBlockViewModel = SelectedBlockViewModel as IItemFilterBlockViewModel;
            if (selectedBlockViewModel == null) return false;

            return HasSelectedBlock() && !selectedBlockViewModel.BlockEnabled;
        }

        private bool HasSelectedBlock()
        {
            return SelectedBlockViewModel != null;
        }

        public bool HasSelectedCommentBlock()
        {
            var selectedBlockViewModel = SelectedBlockViewModel as IItemFilterCommentBlockViewModel;

            return selectedBlockViewModel != null;
        }

        public IItemFilterBlockViewModelBase SelectedBlockViewModel
        {
            get => _selectedBlockViewModel;
            set
            {
                if (value != _selectedBlockViewModel)
                {
                    _selectedBlockViewModel = value;
                    Messenger.Default.Send(new NotificationMessage("SelectedBlockChanged"));
                    RaisePropertyChanged();
                }
            }
        }

        public IItemFilterCommentBlockViewModel CommentBlockBrowserBrowserSelectedBlockViewModel
        {
            get => _sectionBrowserSelectedBlockViewModel;
            set
            {
                _sectionBrowserSelectedBlockViewModel = value;
                SelectedBlockViewModel = value;
                RaisePropertyChanged();
            }
        }

        public IItemFilterScript Script { get; private set; }

        public bool IsDirty
        {
            get => _isDirty || HasDirtyChildren;
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    if (_isDirty)
                    {
                        Title = Filename + "*";
                    }
                    else
                    {
                        Title = Filename;
                    }
                }

            }
        }

        public void RemoveDirtyFlag()
        {
            CleanChildren();
            IsDirty = false;
            RaisePropertyChanged(nameof(Filename));
            RaisePropertyChanged(nameof(DisplayName));
        }

        public void SetDirtyFlag()
        {
            IsDirty = true;
            RaisePropertyChanged(nameof(Filename));
            RaisePropertyChanged(nameof(DisplayName));
        }

        public string DisplayName => !string.IsNullOrEmpty(Filename) ? Filename : Description;

        public string Filename => Path.GetFileName(Script.FilePath);

        public string Filepath => Script.FilePath;

        private bool _filenameIsFake;
        private bool _showAdvanced;
        
        public async Task SaveAsync()
        {
            if (!ValidateScript()) return;
            if (!CheckForUnusedThemeComponents()) return;

            if (_filenameIsFake)
            {
                await SaveAsAsync();
                return;
            }

            Messenger.Default.Send(new NotificationMessage("ShowLoadingBanner"));
            try
            {
                await _persistenceService.SaveItemFilterScriptAsync(Script);
                RemoveDirtyFlag();
            }
            catch (Exception e)
            {
                if (Logger.IsErrorEnabled)
                {
                    Logger.Error(e);
                }

                _messageBoxService.Show("Save Error", "Error saving filter file - " + e.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                Messenger.Default.Send(new NotificationMessage("HideLoadingBanner"));
            }
        }

        public async Task SaveAsAsync()
        {
            if (!ValidateScript()) return;
            if (!CheckForUnusedThemeComponents()) return;

            var saveDialog = new SaveFileDialog
            {
                DefaultExt = ".filter",
                Filter = @"Filter Files (*.filter)|*.filter|All Files (*.*)|*.*",
                InitialDirectory = _persistenceService.ItemFilterScriptDirectory
            };

            var result = saveDialog.ShowDialog();

            if (result != DialogResult.OK) return;
            
            Messenger.Default.Send(new NotificationMessage("ShowLoadingBanner"));

            var previousFilePath = Script.FilePath;
            try
            {
                Script.FilePath = saveDialog.FileName;
                await _persistenceService.SaveItemFilterScriptAsync(Script);
                _filenameIsFake = false;
                Title = Filename;
                RemoveDirtyFlag();
            }
            catch (Exception e)
            {
                if (Logger.IsErrorEnabled)
                {
                    Logger.Error(e);
                }

                _messageBoxService.Show("Save Error", "Error saving filter file - " + e.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Script.FilePath = previousFilePath;
            }
            finally
            {
                Messenger.Default.Send(new NotificationMessage("HideLoadingBanner"));
            }
        }

        private bool CheckForUnusedThemeComponents()
        {
            var unusedThemeComponents =
                Script.ThemeComponents.Where(
                    t =>
                        Script.ItemFilterBlocks.OfType<ItemFilterBlock>().Count(
                            b => b.BlockItems.OfType<IBlockItemWithTheme>().Count(i => i.ThemeComponent == t) > 0) == 0).ToList();

            if (unusedThemeComponents.Count <= 0) return true;

            var themeComponents = unusedThemeComponents.Aggregate(string.Empty,
                (current, themeComponent) => current + themeComponent.ComponentName + Environment.NewLine);

            var ignoreUnusedThemeComponents = _messageBoxService.Show("Unused Theme Components",
                "The following theme components are unused, they will be lost when this script is reopened. Save anyway?" +
                Environment.NewLine + Environment.NewLine + themeComponents, MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);

            return ignoreUnusedThemeComponents != MessageBoxResult.No;
        }

        private void OnActiveDocumentChanged(object sender, EventArgs e)
        {
            if (_avalonDockWorkspaceViewModel.ActiveScriptViewModel != this)
            {
                BlockFilterPredicate = null;
            }
        }

        private bool HasDirtyChildren
        {
            get { return ItemFilterBlockViewModels.Any(vm => vm.IsDirty); }
        }

        private void CleanChildren()
        {
            foreach (var vm in ItemFilterBlockViewModels)
            {
                vm.IsDirty = false;
            }
        }

        private bool ValidateScript()
        {
            var result = Script.Validate();

            if (result.Count == 0) return true;

            var failures = string.Empty;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string failure in result)
            {
                failures += failure + Environment.NewLine;
            }

            var messageText = "The following script validation errors occurred:" + Environment.NewLine + failures;

            _messageBoxService.Show("Script Validation Failure", messageText, MessageBoxButton.OK,
                MessageBoxImage.Exclamation);
            return false;
        }

        private async Task OnCloseCommand()
        {
            await Close();
        }
        
        public async Task Close()
        {
            if (!IsDirty)
            {
                CloseScript();
            }
            else
            {
                var result = _messageBoxService.Show("Filtration",
                    "Save script \"" + Filename + "\"?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                    {
                        await SaveAsync();
                        CloseScript();
                        break;
                    }
                    case MessageBoxResult.No:
                    {
                        CloseScript();
                        break;
                    }
                    case MessageBoxResult.Cancel:
                    {
                        return;
                    }
                }
            }
        }
        
        private void CloseScript()
        {
            var openMasterThemForScript =
                _avalonDockWorkspaceViewModel.OpenMasterThemeForScript(this);
            if (openMasterThemForScript != null)
            {
                _avalonDockWorkspaceViewModel.CloseDocument(openMasterThemForScript);
            }

            _avalonDockWorkspaceViewModel.ActiveDocumentChanged -= OnActiveDocumentChanged;
            _avalonDockWorkspaceViewModel.CloseDocument(this);
        }

        private void OnToggleShowAdvancedCommand(bool showAdvanced)
        {
            ShowAdvanced = !ShowAdvanced;
            Messenger.Default.Send(new NotificationMessage<bool>(ShowAdvanced, "ShowAdvancedToggled"));
        }

        private void OnClearFilterCommand()
        {
            BlockFilterPredicate = null;
        }

        private void OnCopyBlockCommand()
        {
            var commentBlockViewModel = SelectedBlockViewModel as IItemFilterCommentBlockViewModel;
            if (commentBlockViewModel == null || commentBlockViewModel.IsExpanded)
            {
                CopyBlock(SelectedBlockViewModel);
            }
            else
            {
                CopySection(commentBlockViewModel);
            }
        }

        public void CopyBlock(IItemFilterBlockViewModelBase targetBlockViewModel)
        {
            try
            {
                _clipboardService.SetClipboardText(_blockTranslator.TranslateItemFilterBlockBaseToString(SelectedBlockViewModel.BaseBlock));
            }
            catch
            {
                _messageBoxService.Show("Clipboard Error", "Failed to access the clipboard, copy command not completed.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CopySection(IItemFilterCommentBlockViewModel targetCommentBlockViewModel)
        {
            var sectionStart = ItemFilterBlockViewModels.IndexOf(targetCommentBlockViewModel) + 1;
            var copyText = _blockTranslator.TranslateItemFilterBlockBaseToString(targetCommentBlockViewModel.BaseBlock);
            while (sectionStart < ItemFilterBlockViewModels.Count && ItemFilterBlockViewModels[sectionStart] as IItemFilterCommentBlockViewModel == null)
            {
                copyText += Environment.NewLine + Environment.NewLine + _blockTranslator.TranslateItemFilterBlockBaseToString(ItemFilterBlockViewModels[sectionStart].BaseBlock);
                sectionStart++;
            }

            try
            {
                _clipboardService.SetClipboardText(copyText);
            }
            catch
            {
                _messageBoxService.Show("Clipboard Error", "Failed to access the clipboard, copy command not completed.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCopyBlockStyleCommand()
        {
            var selectedBlockViewModel = SelectedBlockViewModel as IItemFilterBlockViewModel;
            if (selectedBlockViewModel != null)
            {
                CopyBlockStyle(selectedBlockViewModel);
            }
        }

        public void CopyBlockStyle(IItemFilterBlockViewModel targetBlockViewModel)
        {
            string outputText = string.Empty;

            foreach (var blockItem in targetBlockViewModel.Block.BlockItems.Where(b => b is IAudioVisualBlockItem))
            {
                if (outputText != string.Empty)
                {
                    outputText += Environment.NewLine;
                }
                outputText += blockItem.OutputText;
            }
            try
            {
                _clipboardService.SetClipboardText(outputText);
            }
            catch
            {
                _messageBoxService.Show("Clipboard Error", "Failed to access the clipboard, copy command not completed.",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnPasteBlockStyleCommand()
        {
            var selectedBlockViewModel = SelectedBlockViewModel as IItemFilterBlockViewModel;
            if (selectedBlockViewModel != null)
            {
                PasteBlockStyle(selectedBlockViewModel);
            }
        }

        public void PasteBlockStyle(IItemFilterBlockViewModel targetBlockViewModel)
        {
            var clipboardText = _clipboardService.GetClipboardText();
            if (string.IsNullOrEmpty(clipboardText))
            {
                return;
            }

            _blockTranslator.ReplaceAudioVisualBlockItemsFromString(targetBlockViewModel.Block.BlockItems, clipboardText);
            targetBlockViewModel.RefreshBlockPreview();
        }

        private void OnPasteBlockCommand()
        {
            PasteBlock(SelectedBlockViewModel);
        }

        public void PasteBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            var commentBlock = targetBlockViewModelBase as IItemFilterCommentBlockViewModel;
            if(commentBlock != null && !commentBlock.IsExpanded)
            {
                var blockIndex = ItemFilterBlockViewModels.IndexOf(targetBlockViewModelBase) + 1;
                while (blockIndex < ItemFilterBlockViewModels.Count && (ItemFilterBlockViewModels[blockIndex] as IItemFilterCommentBlockViewModel) == null)
                {
                    blockIndex++;
                }
                targetBlockViewModelBase = ItemFilterBlockViewModels[blockIndex - 1];
            }
            try
            {
                var clipboardText = _clipboardService.GetClipboardText();
                if (string.IsNullOrEmpty(clipboardText)) return;

                IItemFilterScript pastedScript = _scriptTranslator.TranslatePastedStringToItemFilterScript(clipboardText,
                    Script.ItemFilterScriptSettings.BlockGroupsEnabled);

                foreach (var themeComponent in pastedScript.ThemeComponents.Where(item => !Script.ThemeComponents.ComponentExists(item)))
                {
                    Script.ThemeComponents.Add(themeComponent);
                }

                foreach (var blockGroup in pastedScript.ItemFilterBlockGroups.First().ChildGroups)
                {
                    Script.ItemFilterBlockGroups.First().ChildGroups.Add(blockGroup);
                }

                _scriptCommandManager.ExecuteCommand(new PasteMultipleBlocksCommand(Script, pastedScript.ItemFilterBlocks.ToList(),
                    targetBlockViewModelBase.BaseBlock));

                Messenger.Default.Send(new NotificationMessage<bool>(ShowAdvanced, "BlockGroupsChanged"));
                var lastBlockIndex = ItemFilterBlockViewModels.IndexOf(targetBlockViewModelBase) + pastedScript.ItemFilterBlocks.Count;
                var lastBlock = ItemFilterBlockViewModels[lastBlockIndex];
                if(ViewItemFilterBlockViewModels.Contains(lastBlock))
                {
                    SelectedBlockViewModel = lastBlock;
                }
                RaisePropertyChanged(nameof(SelectedBlockViewModel));
            }
            catch (Exception e)
            {
                Logger.Error(e);
                var innerException = e.InnerException?.Message ?? string.Empty;

                _messageBoxService.Show("Paste Error", e.Message + Environment.NewLine + innerException, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string PrepareBlockForParsing(string inputString)
        {
            inputString = inputString.Replace("\t", "");
            var lines = Regex.Split(inputString, "\r\n|\r|\n").ToList();
            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length == 0)
                {
                    lines.RemoveAt(i--);
                }
                else
                    break;
            }
            for (var i = lines.Count - 1; i >= 0; i--)
            {
                if (lines[i].Length == 0)
                {
                    lines.RemoveAt(i++);
                }
                else
                    break;
            }
            var allCommented = true;
            for (var i = 0; i < lines.Count; i++)
            {
                lines[i] = Regex.Replace(lines[i], @"\s+", " ");
                if(lines[i][0] == '#')
                {
                    if (lines[i].Length > 1 && lines[i][1] != ' ')
                    {
                        lines[i] = "# " + lines[i].Substring(1);
                    }
                }
                else
                {
                    allCommented = false;
                }
            }

            var disabledBlock = -1;
            if (allCommented)
            {
                for (var i = 0; i < lines.Count; i++)
                {
                    if (lines[i].StartsWith("#"))
                    {
                        string curLine = Regex.Replace(lines[i].Substring(1), @"\s+", "");
                        if ((curLine.StartsWith("Show") || curLine.StartsWith("Hide")) && (curLine.Length == 4 || curLine[4] == '#'))
                        {
                            lines[i] = lines[i].Substring(0, 6) + "Disabled" + lines[i].Substring(6);
                            disabledBlock = i;
                            break;
                        }
                    }
                }
            }

            if(disabledBlock >= 0)
            {
                for (var i = disabledBlock; i < lines.Count; i++)
                {
                    lines[i] = lines[i].Substring(2);
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        private void OnMoveBlockToTopCommand()
        {
            var commentBlockViewModel = SelectedBlockViewModel as IItemFilterCommentBlockViewModel;
            if (commentBlockViewModel == null || commentBlockViewModel.IsExpanded)
            {
                MoveBlockToTop(SelectedBlockViewModel);
            }
            else
            {
                MoveSectionToTop(commentBlockViewModel);
            }
        }

        private void OnMoveBlockUpCommand()
        {
            var commentBlockViewModel = SelectedBlockViewModel as IItemFilterCommentBlockViewModel;
            if(commentBlockViewModel == null || commentBlockViewModel.IsExpanded)
            {
                MoveBlockUp(SelectedBlockViewModel);
            }
            else
            {
                MoveSectionUp(commentBlockViewModel);
            }
        }

        public void MoveBlockUp(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            var blockIndex = ItemFilterBlockViewModels.IndexOf(targetBlockViewModelBase);
            if (ItemFilterBlockViewModels[blockIndex - 1].IsVisible)
            {
                _scriptCommandManager.ExecuteCommand(new MoveBlockUpCommand(Script, targetBlockViewModelBase?.BaseBlock));
                SelectedBlockViewModel = ItemFilterBlockViewModels[blockIndex - 1];
            }
            else
            {
                var aboveSectionStart = blockIndex - 1;
                while(ItemFilterBlockViewModels[aboveSectionStart] as IItemFilterCommentBlockViewModel == null)
                {
                    aboveSectionStart--;
                }
                _scriptCommandManager.ExecuteCommand(new MoveSectionToIndexCommand(Script, blockIndex, 1, aboveSectionStart));
                SelectedBlockViewModel = ItemFilterBlockViewModels[aboveSectionStart];
            }

			RaisePropertyChanged("SelectedBlockViewModel");
			SetDirtyFlag();
		}

        public void MoveSectionUp(IItemFilterCommentBlockViewModel targetCommentBlockViewModel)
        {
            var sectionStart = ItemFilterBlockViewModels.IndexOf(targetCommentBlockViewModel);
            var sectionEnd = sectionStart + 1;
            while(sectionEnd < ItemFilterBlockViewModels.Count && ItemFilterBlockViewModels[sectionEnd] as IItemFilterCommentBlockViewModel == null)
            {
                sectionEnd++;
            }

            var newLocation = sectionStart - 1;
            if (ItemFilterBlockViewModels[newLocation].IsVisible)
            {
                _scriptCommandManager.ExecuteCommand(new MoveSectionToIndexCommand(Script, sectionStart, sectionEnd - sectionStart, newLocation));
            }
            else
            {
                while (ItemFilterBlockViewModels[newLocation] as IItemFilterCommentBlockViewModel == null)
                {
                    newLocation--;
                }
                _scriptCommandManager.ExecuteCommand(new MoveSectionToIndexCommand(Script, sectionStart, sectionEnd - sectionStart, newLocation));
            }

            ToggleSection(ItemFilterBlockViewModels[newLocation] as IItemFilterCommentBlockViewModel);
            SelectedBlockViewModel = ItemFilterBlockViewModels[newLocation];
            RaisePropertyChanged("SelectedBlockViewModel");
			SetDirtyFlag();
		}

        private void OnMoveBlockDownCommand()
        {
            var commentBlockViewModel = SelectedBlockViewModel as IItemFilterCommentBlockViewModel;
            if (commentBlockViewModel == null || commentBlockViewModel.IsExpanded)
            {
                MoveBlockDown(SelectedBlockViewModel);
            }
            else
            {
                MoveSectionDown(commentBlockViewModel);
            }
        }

        public void MoveBlockDown(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            var blockIndex = ItemFilterBlockViewModels.IndexOf(targetBlockViewModelBase);
            var beloveBlockAsComment = ItemFilterBlockViewModels[blockIndex + 1] as IItemFilterCommentBlockViewModel;
            if (beloveBlockAsComment == null || beloveBlockAsComment.IsExpanded)
            {
                _scriptCommandManager.ExecuteCommand(new MoveBlockDownCommand(Script, targetBlockViewModelBase?.BaseBlock));
                SelectedBlockViewModel = ItemFilterBlockViewModels[blockIndex + 1];
            }
            else
            {
                var beloveSectionEnd = blockIndex + 2;
                while (beloveSectionEnd < ItemFilterBlockViewModels.Count && ItemFilterBlockViewModels[beloveSectionEnd] as IItemFilterCommentBlockViewModel == null)
                {
                    beloveSectionEnd++;
                }
                _scriptCommandManager.ExecuteCommand(new MoveSectionToIndexCommand(Script, blockIndex, 1, beloveSectionEnd - 1));
                SelectedBlockViewModel = ItemFilterBlockViewModels[beloveSectionEnd - 1];
            }

			RaisePropertyChanged("SelectedBlockViewModel");
			SetDirtyFlag();
		}

        public void MoveSectionDown(IItemFilterCommentBlockViewModel targetCommentBlockViewModel)
        {
            var sectionStart = ItemFilterBlockViewModels.IndexOf(targetCommentBlockViewModel);
            var sectionEnd = sectionStart + 1;
            while (sectionEnd < ItemFilterBlockViewModels.Count && ItemFilterBlockViewModels[sectionEnd] as IItemFilterCommentBlockViewModel == null)
            {
                sectionEnd++;
            }

            if (sectionEnd >= ItemFilterBlockViewModels.Count)
                return;

            var sectionSize = sectionEnd - sectionStart;

            var newLocation = sectionStart + 1;
            var beloveBlockAsComment = ItemFilterBlockViewModels[sectionEnd] as IItemFilterCommentBlockViewModel;
            if (beloveBlockAsComment == null || beloveBlockAsComment.IsExpanded)
            {
                _scriptCommandManager.ExecuteCommand(new MoveSectionToIndexCommand(Script, sectionStart, sectionSize, newLocation));
            }
            else
            {
                while ((newLocation + sectionSize) < ItemFilterBlockViewModels.Count && ItemFilterBlockViewModels[newLocation + sectionSize] as IItemFilterCommentBlockViewModel == null)
                {
                    newLocation++;
                }
                _scriptCommandManager.ExecuteCommand(new MoveSectionToIndexCommand(Script, sectionStart, sectionEnd - sectionStart, newLocation));
            }

            ToggleSection(ItemFilterBlockViewModels[newLocation] as IItemFilterCommentBlockViewModel);
            SelectedBlockViewModel = ItemFilterBlockViewModels[newLocation];
            RaisePropertyChanged("SelectedBlockViewModel");
			SetDirtyFlag();
		}

        private void OnMoveBlockToBottomCommand()
        {
            var commentBlockViewModel = SelectedBlockViewModel as IItemFilterCommentBlockViewModel;
            if (commentBlockViewModel == null || commentBlockViewModel.IsExpanded)
            {
                MoveBlockToBottom(SelectedBlockViewModel);
            }
            else
            {
                MoveSectionToBottom(commentBlockViewModel);
            }
        }

        private void OnAddBlockCommand()
        {
            var selectedBlockAsCommentBlock = SelectedBlockViewModel as IItemFilterCommentBlockViewModel;
            if(selectedBlockAsCommentBlock == null || selectedBlockAsCommentBlock.IsExpanded)
            {
                AddBlock(SelectedBlockViewModel);
            }
            else
            {
                var sectionStart = ItemFilterBlockViewModels.IndexOf(selectedBlockAsCommentBlock);
                var sectionEnd = sectionStart + 1;
                while (sectionEnd < ItemFilterBlockViewModels.Count && ItemFilterBlockViewModels[sectionEnd] as IItemFilterCommentBlockViewModel == null)
                {
                    sectionEnd++;
                }
                AddBlock(ItemFilterBlockViewModels[sectionEnd - 1]);
            }
        }

        public void AddBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            _scriptCommandManager.ExecuteCommand(new AddBlockCommand(Script, targetBlockViewModelBase?.BaseBlock));
			RaisePropertyChanged("SelectedBlockViewModel");
			SetDirtyFlag();
			// TODO: Expand new viewmodel
		}

        public void AddCommentBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            _scriptCommandManager.ExecuteCommand(new AddCommentBlockCommand(Script, targetBlockViewModelBase.BaseBlock));
			RaisePropertyChanged("SelectedBlockViewModel");
			SetDirtyFlag();
		}

        public void DeleteBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            var commentBlockViewModel = SelectedBlockViewModel as IItemFilterCommentBlockViewModel;
            if (commentBlockViewModel == null || commentBlockViewModel.IsExpanded)
            {
                _scriptCommandManager.ExecuteCommand(new RemoveBlockCommand(Script, targetBlockViewModelBase.BaseBlock));
            }
            else
            {
                var sectionStart = ItemFilterBlockViewModels.IndexOf(targetBlockViewModelBase);
                var sectionEnd = sectionStart + 1;
                while (sectionEnd < ItemFilterBlockViewModels.Count && ItemFilterBlockViewModels[sectionEnd] as IItemFilterCommentBlockViewModel == null)
                {
                    sectionEnd++;
                }

                _scriptCommandManager.ExecuteCommand(new RemoveSectionCommand(Script, sectionStart, sectionEnd - sectionStart));
            }

			RaisePropertyChanged("SelectedBlockViewModel");
			SetDirtyFlag();
		}

        public void MoveBlockToBottom(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            _scriptCommandManager.ExecuteCommand(new MoveBlockToBottomCommand(Script, targetBlockViewModelBase.BaseBlock));
			RaisePropertyChanged("SelectedBlockViewModel");
			SetDirtyFlag();
		}

        public void MoveSectionToBottom(IItemFilterCommentBlockViewModel targetCommentBlockViewModel)
        {
            var sectionStart = ItemFilterBlockViewModels.IndexOf(targetCommentBlockViewModel);
            var sectionEnd = sectionStart + 1;
            while (sectionEnd < ItemFilterBlockViewModels.Count && ItemFilterBlockViewModels[sectionEnd] as IItemFilterCommentBlockViewModel == null)
            {
                sectionEnd++;
            }

            var newLocation = ItemFilterBlockViewModels.Count - (sectionEnd - sectionStart);
            _scriptCommandManager.ExecuteCommand(new MoveSectionToIndexCommand(Script, sectionStart, sectionEnd - sectionStart, newLocation));

            ToggleSection(ItemFilterBlockViewModels[newLocation] as IItemFilterCommentBlockViewModel);
            SelectedBlockViewModel = ItemFilterBlockViewModels[newLocation];
			RaisePropertyChanged("SelectedBlockViewModel");
			SetDirtyFlag();
		}

        public void MoveBlockToTop(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            _scriptCommandManager.ExecuteCommand(new MoveBlockToTopCommand(Script, targetBlockViewModelBase.BaseBlock));
			RaisePropertyChanged("SelectedBlockViewModel");
			SetDirtyFlag();
		}

        public void MoveSectionToTop(IItemFilterCommentBlockViewModel targetCommentBlockViewModel)
        {
            var sectionStart = ItemFilterBlockViewModels.IndexOf(targetCommentBlockViewModel);
            var sectionEnd = sectionStart + 1;
            while (sectionEnd < ItemFilterBlockViewModels.Count && ItemFilterBlockViewModels[sectionEnd] as IItemFilterCommentBlockViewModel == null)
            {
                sectionEnd++;
            }

            var newLocation = 0;
            _scriptCommandManager.ExecuteCommand(new MoveSectionToIndexCommand(Script, sectionStart, sectionEnd - sectionStart, newLocation));

            ToggleSection(ItemFilterBlockViewModels[newLocation] as IItemFilterCommentBlockViewModel);
            SelectedBlockViewModel = ItemFilterBlockViewModels[newLocation];
			RaisePropertyChanged("SelectedBlockViewModel");
			SetDirtyFlag();
		}

        private void OnBlockBecameDirty(object sender, EventArgs e)
        {
            SetDirtyFlag();
        }

        private void OnAddCommentBlockCommand()
        {
            var selectedBlockAsCommentBlock = SelectedBlockViewModel as IItemFilterCommentBlockViewModel;
            if (selectedBlockAsCommentBlock == null || selectedBlockAsCommentBlock.IsExpanded)
            {
                AddCommentBlock(SelectedBlockViewModel);
            }
            else
            {
                var sectionStart = ItemFilterBlockViewModels.IndexOf(selectedBlockAsCommentBlock);
                var sectionEnd = sectionStart + 1;
                while (sectionEnd < ItemFilterBlockViewModels.Count && ItemFilterBlockViewModels[sectionEnd] as IItemFilterCommentBlockViewModel == null)
                {
                    sectionEnd++;
                }
                AddCommentBlock(ItemFilterBlockViewModels[sectionEnd - 1]);
            }
        }

        private void OnExpandAllBlocksCommand()
        {
            foreach (var blockViewModel in ItemFilterBlockViewModels.OfType<IItemFilterBlockViewModel>())
            {
                blockViewModel.IsExpanded = true;
            }
        }

        private void OnCollapseAllBlocksCommand()
        {
            foreach (var blockViewModel in ItemFilterBlockViewModels.OfType<IItemFilterBlockViewModel>())
            {
                blockViewModel.IsExpanded = false;
            }
        }

        private void OnDeleteBlockCommand()
        {
            DeleteBlock(SelectedBlockViewModel);
        }

        private void OnDisableBlockCommand()
        {
            var selectedBlockViewModel = SelectedBlockViewModel as IItemFilterBlockViewModel;
            if (selectedBlockViewModel != null)
            {
                selectedBlockViewModel.BlockEnabled = false;
            }
        }

        private void OnEnableBlockCommand()
        {
            var selectedBlockViewModel = SelectedBlockViewModel as IItemFilterBlockViewModel;
            if (selectedBlockViewModel != null)
            {
                selectedBlockViewModel.BlockEnabled = true;
            }
        }

        private void OnDisableSectionCommand()
        {
            var selectedBlockViewModel = SelectedBlockViewModel as IItemFilterCommentBlockViewModel;
            if (selectedBlockViewModel != null)
            {
                var sectionIndex = ItemFilterBlockViewModels.IndexOf(selectedBlockViewModel);
                for (int i = sectionIndex + 1; i < ItemFilterBlockViewModels.Count; i++)
                {
                    var block = ItemFilterBlockViewModels[i] as IItemFilterBlockViewModel;
                    if (block != null)
                    {
                        block.BlockEnabled = false;
                    }
                    else
                        break;
                }
            }
        }

        private void OnEnableSectionCommand()
        {
            var selectedBlockViewModel = SelectedBlockViewModel as IItemFilterCommentBlockViewModel;
            if (selectedBlockViewModel != null)
            {
                var sectionIndex = ItemFilterBlockViewModels.IndexOf(selectedBlockViewModel);
                for (int i = sectionIndex + 1; i < ItemFilterBlockViewModels.Count; i++)
                {
                    var block = ItemFilterBlockViewModels[i] as IItemFilterBlockViewModel;
                    if (block != null)
                    {
                        block.BlockEnabled = true;
                    }
                    else
                        break;
                }
            }
        }

        private void OnExpandSectionCommand()
        {
            var selectedBlockViewModel = SelectedBlockViewModel as IItemFilterCommentBlockViewModel;
            if (selectedBlockViewModel != null && !selectedBlockViewModel.IsExpanded)
            {
                ToggleSection(selectedBlockViewModel);
            }
        }

        private void OnCollapseSectionCommand()
        {
            var selectedBlockViewModel = SelectedBlockViewModel as IItemFilterCommentBlockViewModel;
            if (selectedBlockViewModel != null && selectedBlockViewModel.IsExpanded)
            {
                ToggleSection(selectedBlockViewModel);
            }
        }

        public void ToggleSection(IItemFilterCommentBlockViewModel targetCommentBlockViewModelBase, bool deferPropertyChanged = false)
        {
            var newState = !targetCommentBlockViewModelBase.IsExpanded;
            targetCommentBlockViewModelBase.IsExpanded = newState;
            var sectionIndex = ItemFilterBlockViewModels.IndexOf(targetCommentBlockViewModelBase);
            for (int i = sectionIndex + 1; i < ItemFilterBlockViewModels.Count; i++)
            {
                var block = ItemFilterBlockViewModels[i] as IItemFilterBlockViewModel;
                if (block != null)
                {
                    block.IsVisible = newState;
                }
                else
                    break;
            }

            if (!deferPropertyChanged)
                RaisePropertyChanged(nameof(ViewItemFilterBlockViewModels));
        }

        private void CollapseAllSections()
        {
            for (int i = 0; i < ItemFilterBlockViewModels.Count; i++)
            {
                var block = ItemFilterBlockViewModels[i] as IItemFilterCommentBlockViewModel;
                if (block != null)
                {
                    if(block.IsExpanded)
                    {
                        ToggleSection(block, true);
                    }
                }
            }
            
            RaisePropertyChanged(nameof(ViewItemFilterBlockViewModels));
        }

        private void ExpandAllSections()
        {
            for (int i = 0; i < ItemFilterBlockViewModels.Count; i++)
            {
                var block = ItemFilterBlockViewModels[i] as IItemFilterCommentBlockViewModel;
                if (block != null && !block.IsExpanded)
                {
                    ToggleSection(block, true);
                }
            }
            
            RaisePropertyChanged(nameof(ViewItemFilterBlockViewModels));
        }
    }
}
