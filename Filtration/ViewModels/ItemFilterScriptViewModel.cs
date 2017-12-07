using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using Filtration.ObjectModel.Commands;
using Filtration.ObjectModel.Commands.ItemFilterScript;
using Filtration.Parser.Interface.Services;
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
        Predicate<IItemFilterBlockViewModel> BlockFilterPredicate { get; set; }
        
        bool ShowAdvanced { get; }
        string Description { get; set; }
        string DisplayName { get; }
        
        void Initialise(IItemFilterScript itemFilterScript, bool newScript);
        void RemoveDirtyFlag();
        void SetDirtyFlag();
        bool HasSelectedEnabledBlock();
        bool HasSelectedDisabledBlock();

        RelayCommand AddBlockCommand { get; }
        RelayCommand AddSectionCommand { get; }
        RelayCommand DisableBlockCommand { get; }
        RelayCommand EnableBlockCommand { get; }
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
    }

    internal class ItemFilterScriptViewModel : PaneViewModel, IItemFilterScriptViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IItemFilterBlockBaseViewModelFactory _itemFilterBlockBaseViewModelFactory;
        private readonly IItemFilterBlockTranslator _blockTranslator;
        private readonly IAvalonDockWorkspaceViewModel _avalonDockWorkspaceViewModel;
        private readonly IItemFilterPersistenceService _persistenceService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IClipboardService _clipboardService;
        private readonly IBlockGroupHierarchyBuilder _blockGroupHierarchyBuilder;

        private bool _isDirty;
        private IItemFilterBlockViewModelBase _selectedBlockViewModel;
        private IItemFilterCommentBlockViewModel _sectionBrowserSelectedBlockViewModel;
        private readonly ObservableCollection<IItemFilterBlockViewModelBase> _itemFilterBlockViewModels;
        private ICollectionView _itemFilterBlockViewModelsCollectionView;
        private Predicate<IItemFilterBlockViewModel> _blockFilterPredicate;
        private ICommandManager _scriptCommandManager;

        public ItemFilterScriptViewModel(IItemFilterBlockBaseViewModelFactory itemFilterBlockBaseViewModelFactory,
                                         IItemFilterBlockTranslator blockTranslator,
                                         IAvalonDockWorkspaceViewModel avalonDockWorkspaceViewModel,
                                         IItemFilterPersistenceService persistenceService,
                                         IMessageBoxService messageBoxService,
                                         IClipboardService clipboardService,
                                         IBlockGroupHierarchyBuilder blockGroupHierarchyBuilder)
        {
            _itemFilterBlockBaseViewModelFactory = itemFilterBlockBaseViewModelFactory;
            _blockTranslator = blockTranslator;
            _avalonDockWorkspaceViewModel = avalonDockWorkspaceViewModel;
            _avalonDockWorkspaceViewModel.ActiveDocumentChanged += OnActiveDocumentChanged;
            _persistenceService = persistenceService;
            _messageBoxService = messageBoxService;
            _clipboardService = clipboardService;
            _blockGroupHierarchyBuilder = blockGroupHierarchyBuilder;
            _itemFilterBlockViewModels = new ObservableCollection<IItemFilterBlockViewModelBase>();

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
            CopyBlockCommand = new RelayCommand(OnCopyBlockCommand, () => SelectedBlockViewModel != null);
            CopyBlockStyleCommand = new RelayCommand(OnCopyBlockStyleCommand, () => SelectedBlockViewModel != null);
            PasteBlockCommand = new RelayCommand(OnPasteBlockCommand, () => SelectedBlockViewModel != null);
            PasteBlockStyleCommand = new RelayCommand(OnPasteBlockStyleCommand, () => SelectedBlockViewModel != null);
            ExpandAllBlocksCommand = new RelayCommand(OnExpandAllBlocksCommand);
            CollapseAllBlocksCommand = new RelayCommand(OnCollapseAllBlocksCommand);

            var icon = new BitmapImage();
            icon.BeginInit();
            icon.UriSource = new Uri("pack://application:,,,/Filtration;component/Resources/Icons/script_icon.png");
            icon.EndInit();
            IconSource = icon;
        }

        public void Initialise(IItemFilterScript itemFilterScript, bool newScript)
        {
            ItemFilterBlockViewModels.Clear();

            Script = itemFilterScript;
            _scriptCommandManager = Script.CommandManager;
            AddItemFilterBlockViewModels(Script.ItemFilterBlocks, -1);
            
            Script.ItemFilterBlocks.CollectionChanged += ItemFilterBlocksOnCollectionChanged;

            _filenameIsFake = newScript;

            if (newScript)
            {
                Script.FilePath = "Untitled.filter";
            }
            
            Title = Filename;
            ContentId = "ScriptContentId";
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
        public RelayCommand CopyBlockCommand { get; }
        public RelayCommand CopyBlockStyleCommand { get; }
        public RelayCommand PasteBlockCommand { get; }
        public RelayCommand PasteBlockStyleCommand { get; }
        public RelayCommand ExpandAllBlocksCommand { get; }
        public RelayCommand CollapseAllBlocksCommand { get; }

        public bool IsActiveDocument
        {
            get
            {
                var isActiveDocument = _avalonDockWorkspaceViewModel.ActiveScriptViewModel == this;
                Debug.WriteLine($"IsActiveDocument: {isActiveDocument}");

                return isActiveDocument;

            }
        }

        public ObservableCollection<IItemFilterBlockViewModelBase> ItemFilterBlockViewModels
        {
            get
            {
                _itemFilterBlockViewModelsCollectionView =
                    CollectionViewSource.GetDefaultView(_itemFilterBlockViewModels);
                _itemFilterBlockViewModelsCollectionView.Filter = BlockFilter;
                
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
                RaisePropertyChanged(nameof(ItemFilterBlockViewModels));
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
                RaisePropertyChanged(nameof(ItemFilterBlockViewModels));
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
                            b => b.BlockItems.OfType<ColorBlockItem>().Count(i => i.ThemeComponent == t) > 0) == 0).ToList();

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
            CopyBlock(SelectedBlockViewModel);
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
            try
            {
                var clipboardText = _clipboardService.GetClipboardText();
                if (string.IsNullOrEmpty(clipboardText)) return;
                
                var translatedBlock = _blockTranslator.TranslateStringToItemFilterBlock(clipboardText, Script, true); // TODO: Doesn't handle pasting comment blocks?
                if (translatedBlock == null) return;

                _scriptCommandManager.ExecuteCommand(new PasteBlockCommand(Script, translatedBlock, targetBlockViewModelBase.BaseBlock));
            }
            catch (Exception e)
            {
                Logger.Error(e);
                var innerException = e.InnerException?.Message ?? string.Empty;

                _messageBoxService.Show("Paste Error", e.Message + Environment.NewLine + innerException, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnMoveBlockToTopCommand()
        {
            MoveBlockToTop(SelectedBlockViewModel);
        }

        private void OnMoveBlockUpCommand()
        {
            MoveBlockUp(SelectedBlockViewModel);
        }

        public void MoveBlockUp(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            _scriptCommandManager.ExecuteCommand(new MoveBlockUpCommand(Script, targetBlockViewModelBase?.BaseBlock));
        }

        private void OnMoveBlockDownCommand()
        {
            MoveBlockDown(SelectedBlockViewModel);
        }

        public void MoveBlockDown(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            _scriptCommandManager.ExecuteCommand(new MoveBlockDownCommand(Script, targetBlockViewModelBase?.BaseBlock));
        }

        private void OnMoveBlockToBottomCommand()
        {
            MoveBlockToBottom(SelectedBlockViewModel);
        }

        private void OnAddBlockCommand()
        {
            AddBlock(SelectedBlockViewModel);
        }

        public void AddBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            _scriptCommandManager.ExecuteCommand(new AddBlockCommand(Script, targetBlockViewModelBase?.BaseBlock));
            // TODO: Expand new viewmodel
        }

        public void AddCommentBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            _scriptCommandManager.ExecuteCommand(new AddCommentBlockCommand(Script, targetBlockViewModelBase.BaseBlock));
        }

        public void DeleteBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            _scriptCommandManager.ExecuteCommand(new RemoveBlockCommand(Script, targetBlockViewModelBase.BaseBlock));
        }

        public void MoveBlockToBottom(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            _scriptCommandManager.ExecuteCommand(new MoveBlockToBottomCommand(Script, targetBlockViewModelBase.BaseBlock));
        }

        public void MoveBlockToTop(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            _scriptCommandManager.ExecuteCommand(new MoveBlockToTopCommand(Script, targetBlockViewModelBase.BaseBlock));
        }

        private void OnBlockBecameDirty(object sender, EventArgs e)
        {
            SetDirtyFlag();
        }

        private void OnAddCommentBlockCommand()
        {
            AddCommentBlock(SelectedBlockViewModel);
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
    }
}
