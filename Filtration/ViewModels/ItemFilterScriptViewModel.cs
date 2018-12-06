using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using DynamicData.Binding;
using Filtration.Common.Services;
using Filtration.Common.ViewModels;
using Filtration.Interface;
using Filtration.ObjectModel;
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
        IItemFilterBlockViewModelBase LastSelectedBlockViewModel { get; }
        ObservableCollection<IItemFilterBlockViewModelBase> SelectedBlockViewModels { get; }
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
        bool CanModifySelectedBlocks();
        bool CanModifyBlock(IItemFilterBlockViewModelBase itemFilterBlock);

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
        RelayCommand MoveBlockToBottomCommand { get; }
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
        RelayCommand ClearStylesCommand { get; }
        RelayCommand EnableDropSoundsCommand { get; }
        RelayCommand DisableDropSoundsCommand { get; }

        void AddCommentBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase);
        void AddBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase);
        void CopyBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase);
        void CopyBlocks(IEnumerable<IItemFilterBlockViewModelBase> targetBlockViewModels);
        void CopyBlockStyle(IItemFilterBlockViewModel targetBlockViewModel);
        void PasteBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase);
        void PasteBlockStyle(IItemFilterBlockViewModel targetBlockViewModel);
        void DeleteBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase);
        void DeleteBlocks(IEnumerable<IItemFilterBlockViewModelBase> targetBlockViewModels);
        void MoveBlockUp(IItemFilterBlockViewModelBase targetBlockViewModelBase);
        void MoveBlockDown(IItemFilterBlockViewModelBase targetBlockViewModelBase);
        void MoveBlockToTop(IItemFilterBlockViewModelBase targetBlockViewModelBase);
        void MoveBlocksToTop(IEnumerable<IItemFilterBlockViewModelBase> targetBlockViewModels);
        void MoveBlockToBottom(IItemFilterBlockViewModelBase targetBlockViewModelBase);
        void MoveBlocksToBottom(IEnumerable<IItemFilterBlockViewModelBase> targetBlockViewModels);
        void ToggleSection(IItemFilterCommentBlockViewModel targetCommentBlockViewModelBase, bool deferViewUpdate = false);
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

        private bool _isDirty;
        private IItemFilterCommentBlockViewModel _sectionBrowserSelectedBlockViewModel;
        private Predicate<IItemFilterBlockViewModel> _blockFilterPredicate;
        private ICommandManager _scriptCommandManager;

        private List<IDisposable> _subscriptions;
        private ObservableCollection<string> _customSoundsAvailable;
        private readonly List<IItemFilterBlockViewModelBase> _lastAddedBlocks;

        public ItemFilterScriptViewModel(IItemFilterBlockBaseViewModelFactory itemFilterBlockBaseViewModelFactory,
                                         IItemFilterBlockTranslator blockTranslator,
                                         IItemFilterScriptTranslator scriptTranslator,
                                         IAvalonDockWorkspaceViewModel avalonDockWorkspaceViewModel,
                                         IItemFilterPersistenceService persistenceService,
                                         IMessageBoxService messageBoxService,
                                         IClipboardService clipboardService)
        {
            _itemFilterBlockBaseViewModelFactory = itemFilterBlockBaseViewModelFactory;
            _blockTranslator = blockTranslator;
            _scriptTranslator = scriptTranslator;
            _avalonDockWorkspaceViewModel = avalonDockWorkspaceViewModel;
            _avalonDockWorkspaceViewModel.ActiveDocumentChanged += OnActiveDocumentChanged;
            _persistenceService = persistenceService;
            _messageBoxService = messageBoxService;
            _clipboardService = clipboardService;
            _subscriptions = new List<IDisposable>();
            ItemFilterBlockViewModels = new ObservableCollection<IItemFilterBlockViewModelBase>();
            SelectedBlockViewModels = new ObservableCollection<IItemFilterBlockViewModelBase>();

            _subscriptions.Add(
                SelectedBlockViewModels.ToObservableChangeSet()
                .Throttle(TimeSpan.FromMilliseconds(30))
                .Subscribe(x => {
                    RaisePropertyChanged(nameof(SelectedBlockViewModels));
                    RaisePropertyChanged(nameof(LastSelectedBlockViewModel));
                    Messenger.Default.Send(new NotificationMessage("LastSelectedBlockChanged"));
                })
            );

            _lastAddedBlocks = new List<IItemFilterBlockViewModelBase>();
            _showAdvanced = Settings.Default.ShowAdvanced;

            _avalonDockWorkspaceViewModel.ActiveDocumentChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(IsActiveDocument));
            };

            ToggleShowAdvancedCommand = new RelayCommand<bool>(OnToggleShowAdvancedCommand);
            ClearFilterCommand = new RelayCommand(OnClearFilterCommand, () => BlockFilterPredicate != null);
            ClearStylesCommand = new RelayCommand(OnClearStylesCommand, () => SelectedBlockViewModels.OfType<IItemFilterBlockViewModel>().Any());
            CloseCommand = new RelayCommand(async () => await OnCloseCommand());
            DeleteBlockCommand = new RelayCommand(OnDeleteBlockCommand, CanModifySelectedBlocks);
            MoveBlockToTopCommand = new RelayCommand(OnMoveBlockToTopCommand, () => SelectedBlockViewModels.Count > 0 && CanModifySelectedBlocks());
            MoveBlockToBottomCommand = new RelayCommand(OnMoveBlockToBottomCommand, () => SelectedBlockViewModels.Count > 0 && CanModifySelectedBlocks());
            MoveBlockUpCommand = new RelayCommand(OnMoveBlockUpCommand, () => SelectedBlockViewModels.Count == 1 && ViewItemFilterBlockViewModels.IndexOf(LastSelectedBlockViewModel) > 0);
            MoveBlockDownCommand = new RelayCommand(OnMoveBlockDownCommand, () => SelectedBlockViewModels.Count == 1 && ViewItemFilterBlockViewModels.IndexOf(LastSelectedBlockViewModel) < (ViewItemFilterBlockViewModels.Count - 1));
            AddBlockCommand = new RelayCommand(OnAddBlockCommand);
            AddSectionCommand = new RelayCommand(OnAddCommentBlockCommand);
            DisableBlockCommand = new RelayCommand(OnDisableBlockCommand, () => HasSelectedEnabledBlock() && CanModifySelectedBlocks());
            EnableBlockCommand = new RelayCommand(OnEnableBlockCommand, () => HasSelectedDisabledBlock() && CanModifySelectedBlocks());
            DisableSectionCommand = new RelayCommand(OnDisableSectionCommand, () => HasSelectedCommentBlock() && CanModifySelectedBlocks());
            EnableSectionCommand = new RelayCommand(OnEnableSectionCommand, () => HasSelectedCommentBlock() && CanModifySelectedBlocks());
            ExpandSectionCommand = new RelayCommand(OnExpandSectionCommand, HasSelectedCommentBlock);
            CollapseSectionCommand = new RelayCommand(OnCollapseSectionCommand, HasSelectedCommentBlock);
            CopyBlockCommand = new RelayCommand(OnCopyBlockCommand, () => SelectedBlockViewModels.Count > 0);
            CopyBlockStyleCommand = new RelayCommand(OnCopyBlockStyleCommand, () => LastSelectedBlockViewModel != null);
            PasteBlockCommand = new RelayCommand(OnPasteBlockCommand, () => LastSelectedBlockViewModel != null);
            PasteBlockStyleCommand = new RelayCommand(OnPasteBlockStyleCommand, () => LastSelectedBlockViewModel != null);
            ExpandAllBlocksCommand = new RelayCommand(OnExpandAllBlocksCommand);
            CollapseAllBlocksCommand = new RelayCommand(OnCollapseAllBlocksCommand);
            ExpandAllSectionsCommand = new RelayCommand(ExpandAllSections);
            CollapseAllSectionsCommand = new RelayCommand(CollapseAllSections);
            EnableDropSoundsCommand = new RelayCommand(OnEnableDropSoundsCommand, CanModifySelectedBlocks);
            DisableDropSoundsCommand = new RelayCommand(OnDisableDropSoundsCommand, CanModifySelectedBlocks);

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
            InitialiseCustomSounds();

            AddItemFilterBlockViewModels(Script.ItemFilterBlocks, -1);

            UpdateChildCount();
            UpdateFilteredBlockList();

            foreach (var block in Script.ItemFilterBlocks.OfType<IItemFilterBlock>())
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

            _filenameIsFake = newScript;

            if (newScript)
            {
                Script.FilePath = "Untitled.filter";
            }

            Title = Filename;
            ContentId = "ScriptContentId";

            if (!Settings.Default.BlocksExpandedOnOpen)
            {
                CollapseAllSections();
            }
        }

        private void InitialiseCustomSounds()
        {
            _customSoundsAvailable = new ObservableCollection<string>();
            _customSoundsAvailable.CollectionChanged += CustomSoundsAvailableOnCollectionChanged;

            var poeFolderFiles = Directory.GetFiles(_persistenceService.ItemFilterScriptDirectory + "\\").Where(
                s => s.EndsWith(".mp3")
                     || s.EndsWith(".wav")
                     || s.EndsWith(".wma")
                     || s.EndsWith(".3gp")
                     || s.EndsWith(".aag")
                     || s.EndsWith(".m4a")
                     || s.EndsWith(".ogg")
            ).OrderBy(f => f);

            foreach (var file in poeFolderFiles)
            {
                _customSoundsAvailable.Add(file.Replace(_persistenceService.ItemFilterScriptDirectory + "\\", ""));
            }
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
                        RemoveItemFilterBlockViewModels(notifyCollectionChangedEventArgs.OldItems.Cast<IItemFilterBlockBase>());
                        break;
                    }
                default:
                    {
                        Debugger.Break(); // Unhandled NotifyCollectionChangedAction
                        break;
                    }
            }

            UpdateChildCount();
            UpdateFilteredBlockList();
        }

        private void CustomSoundsAvailableOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RaisePropertyChanged(nameof(CustomSoundsAvailable));
        }

        private void AddItemFilterBlockViewModels(IEnumerable<IItemFilterBlockBase> itemFilterBlocks, int addAtIndex)
        {
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

                _lastAddedBlocks.Add(vm);

                if (itemFilterBlock is IItemFilterBlock itemBlock)
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

        private void RemoveItemFilterBlockViewModels(IEnumerable<IItemFilterBlockBase> itemFilterBlocks)
        {
            foreach (var itemFilterBlock in itemFilterBlocks)
            {
                var itemFilterBlockViewModel = ItemFilterBlockViewModels.FirstOrDefault(f => f.BaseBlock == itemFilterBlock);
                if (itemFilterBlockViewModel == null)
                {
                    throw new InvalidOperationException("Item Filter Block removed from model but does not exist in view model!");
                }

                ItemFilterBlockViewModels.Remove(itemFilterBlockViewModel);
            }
        }

        private void UpdateFilteredBlockList()
        {
            ICollectionView filteredBlocks = CollectionViewSource.GetDefaultView(ItemFilterBlockViewModels);
            filteredBlocks.Filter = BlockFilter;

            ItemFilterCommentBlockViewModel previousSection = new ItemFilterCommentBlockViewModel();
            foreach (IItemFilterBlockViewModelBase block in filteredBlocks)
            {
                if (block is IItemFilterBlockViewModel)
                {
                    previousSection.VisibleChildCount++;
                }
                else if (block is ItemFilterCommentBlockViewModel model)
                {
                    previousSection = model;
                    previousSection.VisibleChildCount = 0;
                }
            }

            ViewItemFilterBlockViewModels = (ListCollectionView)CollectionViewSource.GetDefaultView(
                filteredBlocks.Cast<IItemFilterBlockViewModelBase>().ToList());
            ViewItemFilterBlockViewModels.Filter = BlockVisibilityFilter;

            Messenger.Default.Send(new NotificationMessage("SectionsChanged"));
            SelectedBlockViewModels.Clear();
            RaisePropertyChanged(nameof(ViewItemFilterBlockViewModels));
        }

        private void UpdateChildCount()
        {
            ItemFilterCommentBlockViewModel previousSection = new ItemFilterCommentBlockViewModel();
            foreach (IItemFilterBlockViewModelBase block in ItemFilterBlockViewModels)
            {
                if (block is IItemFilterBlockViewModel)
                {
                    previousSection.ChildCount++;
                    block.IsVisible = previousSection.IsExpanded;
                }
                else if (block is ItemFilterCommentBlockViewModel model)
                {
                    previousSection = model;
                    previousSection.ChildCount = 0;
                }
            }
        }

        private List<IItemFilterBlockViewModelBase> GetChildren(IItemFilterCommentBlockViewModel targetBlockViewModel)
        {
            return ItemFilterBlockViewModels.ToList().GetRange(ItemFilterBlockViewModels.IndexOf(targetBlockViewModel) + 1,
                targetBlockViewModel.ChildCount);
        }

        private List<int> GetChildrenIndexes(IItemFilterCommentBlockViewModel targetBlockViewModel)
        {
            return Enumerable.Range(ItemFilterBlockViewModels.IndexOf(targetBlockViewModel) + 1,
                targetBlockViewModel.ChildCount).ToList();
        }

        private void ValidateSelectedBlocks()
        {
            for (var i = 0; i < SelectedBlockViewModels.Count; i++)
            {
                if (SelectedBlockViewModels[i] == null || !ViewItemFilterBlockViewModels.Contains(SelectedBlockViewModels[i]))
                {
                    SelectedBlockViewModels.RemoveAt(i--);
                }
            }
        }

        public RelayCommand<bool> ToggleShowAdvancedCommand { get; }
        public RelayCommand ClearFilterCommand { get; }
        public RelayCommand ClearStylesCommand { get; }
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
        public RelayCommand EnableDropSoundsCommand { get; }
        public RelayCommand DisableDropSoundsCommand { get; }

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
            private set
            {
                _customSoundsAvailable = value;
                RaisePropertyChanged();
            }
        }

        public ListCollectionView ViewItemFilterBlockViewModels { get; private set; }

        public ObservableCollection<IItemFilterBlockViewModelBase> ItemFilterBlockViewModels { get; }

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
            if (!(item is IItemFilterBlockViewModelBase))
                return false;

            if (Script.ItemFilterScriptSettings.BlockGroupsEnabled && BlockFilterPredicate != null && item is IItemFilterCommentBlockViewModel && !(item as IItemFilterCommentBlockViewModel).HasVisibleChild)
                return false;

            return (item as IItemFilterBlockViewModelBase).IsVisible;
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
                UpdateFilteredBlockList();
            }
        }

        public IEnumerable<IItemFilterCommentBlockViewModel> ItemFilterCommentBlockViewModels => ViewItemFilterBlockViewModels.OfType<IItemFilterCommentBlockViewModel>();

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
                UpdateFilteredBlockList();
            }
        }

        public bool HasSelectedEnabledBlock()
        {
            foreach (var block in SelectedBlockViewModels.OfType<IItemFilterBlockViewModel>())
            {
                if (block.BlockEnabled)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasSelectedDisabledBlock()
        {
            foreach (var block in SelectedBlockViewModels.OfType<IItemFilterBlockViewModel>())
            {
                if (!block.BlockEnabled)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasSelectedCommentBlock()
        {
            return SelectedBlockViewModels.OfType<IItemFilterCommentBlockViewModel>().Any();
        }

        public bool CanModifySelectedBlocks()
        {
            ValidateSelectedBlocks();

            if (SelectedBlockViewModels.Count < 1)
                return false;

            foreach (var block in SelectedBlockViewModels)
            {
                if (!CanModifyBlock(block))
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanModifyBlock(IItemFilterBlockViewModelBase itemFilterBlock)
        {
            if (itemFilterBlock == null)
                return false;

            if (itemFilterBlock is IItemFilterBlockViewModel)
                return true;

            var itemFilterCommentBlock = itemFilterBlock as ItemFilterCommentBlockViewModel;
            if (itemFilterCommentBlock.IsExpanded)
                return true;

            return itemFilterCommentBlock.ChildCount == itemFilterCommentBlock.VisibleChildCount;
        }

        public ObservableCollection<IItemFilterBlockViewModelBase> SelectedBlockViewModels { get; }

        public IItemFilterBlockViewModelBase LastSelectedBlockViewModel => SelectedBlockViewModels.Count > 0 ? SelectedBlockViewModels.Last() : null;

        public IItemFilterCommentBlockViewModel CommentBlockBrowserBrowserSelectedBlockViewModel
        {
            get => _sectionBrowserSelectedBlockViewModel;
            set
            {
                _sectionBrowserSelectedBlockViewModel = value;
                SelectedBlockViewModels.Clear();
                SelectedBlockViewModels.Add(value);
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

        public async Task<bool> Close()
        {
            if (!IsDirty)
            {
                CloseScript();
                return true;
            }

            var result = _messageBoxService.Show("Filtration",
                                                 "Save script \"" + Filename + "\"?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                {
                    await SaveAsync();
                    CloseScript();
                    return true;
                }
                case MessageBoxResult.No:
                {
                    CloseScript();
                    return true;
                }
                case MessageBoxResult.Cancel:
                {
                    return false;
                }
                default:
                {
                    return false;
                }
            }
        }

        private void CloseScript()
        {
            foreach (var disposable in Enumerable.Reverse(_subscriptions))
            {
                disposable.Dispose();
            }

            _subscriptions.Clear();

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

        private void OnClearStylesCommand()
        {
            ValidateSelectedBlocks();
            foreach (var block in SelectedBlockViewModels.OfType<IItemFilterBlockViewModel>())
            {
                var blockItems = block.Block.BlockItems;
                for (var i = 0; i < blockItems.Count; i++)
                {
                    if (blockItems[i] is IAudioVisualBlockItem)
                    {
                        blockItems.RemoveAt(i--);
                    }
                }
                block.RefreshBlockPreview();
            }
        }

        private void OnCopyBlockCommand()
        {
            var blocksToCopy = new List<IItemFilterBlockViewModelBase>();
            foreach (var block in SelectedBlockViewModels.OfType<IItemFilterBlockViewModelBase>())
            {
                blocksToCopy.Add(block);
                if (block is IItemFilterCommentBlockViewModel model && !model.IsExpanded)
                {
                    blocksToCopy.AddRange(GetChildren(model));
                }
            }

            CopyBlocks(blocksToCopy);
        }

        public void CopyBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            if (!(targetBlockViewModelBase is IItemFilterCommentBlockViewModel) ||
                (targetBlockViewModelBase as IItemFilterCommentBlockViewModel).IsExpanded)
            {
                CopyBlocks(new List<IItemFilterBlockViewModelBase> { targetBlockViewModelBase });
            }
            else
            {
                var blocksToCopy = new List<IItemFilterBlockViewModelBase> { targetBlockViewModelBase };
                blocksToCopy.AddRange(GetChildren((IItemFilterCommentBlockViewModel)targetBlockViewModelBase));
                CopyBlocks(blocksToCopy);
            }
        }

        public void CopyBlocks(IEnumerable<IItemFilterBlockViewModelBase> targetBlockViewModels)
        {
            if (!targetBlockViewModels.Any())
            {
                return;
            }

            var copyText = "";
            foreach (var block in targetBlockViewModels)
            {
                copyText += Environment.NewLine + _blockTranslator.TranslateItemFilterBlockBaseToString(block.BaseBlock) + Environment.NewLine;
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
            if (LastSelectedBlockViewModel is IItemFilterBlockViewModel selectedBlockViewModel)
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
            if (LastSelectedBlockViewModel is IItemFilterBlockViewModel selectedBlockViewModel)
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
            PasteBlock(LastSelectedBlockViewModel);
        }

        public void PasteBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            var commentBlock = targetBlockViewModelBase as IItemFilterCommentBlockViewModel;
            if (commentBlock != null && !commentBlock.IsExpanded)
            {
                targetBlockViewModelBase = ItemFilterBlockViewModels[ItemFilterBlockViewModels.IndexOf(targetBlockViewModelBase) +
                    commentBlock.ChildCount];
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

                ExecuteCommandAndSelectAdded(new PasteBlocksCommand(Script, pastedScript.ItemFilterBlocks.ToList(), targetBlockViewModelBase.BaseBlock));

                Messenger.Default.Send(new NotificationMessage<bool>(ShowAdvanced, "BlockGroupsChanged"));

                if (_lastAddedBlocks.Count > 0 && !(_lastAddedBlocks[0] is IItemFilterCommentBlockViewModel))
                {
                    if (commentBlock != null && !commentBlock.IsExpanded)
                    {
                        ToggleSection(commentBlock);
                    }
                }

                SetDirtyFlag();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                var innerException = e.InnerException?.Message ?? string.Empty;

                _messageBoxService.Show("Paste Error", e.Message + Environment.NewLine + innerException, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnMoveBlockUpCommand()
        {
            MoveBlockUp(LastSelectedBlockViewModel);
        }

        public void MoveBlockUp(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            var blockIndex = ViewItemFilterBlockViewModels.IndexOf(targetBlockViewModelBase);
            if (blockIndex < 1)
                return;

            var indexToMove = ItemFilterBlockViewModels.IndexOf(
                ViewItemFilterBlockViewModels.GetItemAt(blockIndex - 1) as IItemFilterBlockViewModelBase);

            if (targetBlockViewModelBase is IItemFilterCommentBlockViewModel model &&
                !model.IsExpanded)
            {
                ExecuteCommandAndSelectAdded(new MoveBlocksToIndexCommand(Script,
                    Enumerable.Range(ItemFilterBlockViewModels.IndexOf(model),
                                     model.ChildCount + 1).ToList(), indexToMove));
                if (_lastAddedBlocks.Count > 0)
                {
                    ToggleSection(_lastAddedBlocks[0] as IItemFilterCommentBlockViewModel);
                }
            }
            else
            {
                var parent = blockIndex > 1 ? ViewItemFilterBlockViewModels.GetItemAt(blockIndex - 2) as IItemFilterCommentBlockViewModel : null;
                if (parent != null && !parent.IsExpanded)
                {
                    ToggleSection(parent);
                }
                ExecuteCommandAndSelectAdded(new MoveBlocksToIndexCommand(Script, targetBlockViewModelBase.BaseBlock, indexToMove));
            }

            SetDirtyFlag();
        }

        private void OnMoveBlockDownCommand()
        {
            MoveBlockDown(LastSelectedBlockViewModel);
        }

        public void MoveBlockDown(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            var blockIndex = ViewItemFilterBlockViewModels.IndexOf(targetBlockViewModelBase);
            if (blockIndex >= ViewItemFilterBlockViewModels.Count - 1)
                return;

            var belowBlock = ViewItemFilterBlockViewModels.GetItemAt(blockIndex + 1) as IItemFilterBlockViewModelBase;
            var indexToMove = ItemFilterBlockViewModels.IndexOf(belowBlock);
            if (belowBlock is ItemFilterCommentBlockViewModel && !(belowBlock as ItemFilterCommentBlockViewModel).IsExpanded)
            {
                indexToMove += (belowBlock as ItemFilterCommentBlockViewModel).ChildCount;
            }

            if (targetBlockViewModelBase is IItemFilterCommentBlockViewModel model &&
                !model.IsExpanded)
            {
                indexToMove -= model.ChildCount;
                ExecuteCommandAndSelectAdded(new MoveBlocksToIndexCommand(Script,
                    Enumerable.Range(ItemFilterBlockViewModels.IndexOf(model),
                                     model.ChildCount + 1).ToList(), indexToMove));
                if (_lastAddedBlocks.Count > 0)
                {
                    ToggleSection(_lastAddedBlocks[0] as IItemFilterCommentBlockViewModel);
                }
            }
            else
            {
                if (belowBlock is ItemFilterCommentBlockViewModel && !(belowBlock as ItemFilterCommentBlockViewModel).IsExpanded)
                {
                    ToggleSection(belowBlock as IItemFilterCommentBlockViewModel);
                }
                ExecuteCommandAndSelectAdded(new MoveBlocksToIndexCommand(Script, targetBlockViewModelBase.BaseBlock, indexToMove));
            }

            SetDirtyFlag();
        }

        private void OnAddBlockCommand()
        {
            AddBlock(LastSelectedBlockViewModel);
        }

        public void AddBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            if (targetBlockViewModelBase is IItemFilterCommentBlockViewModel model && !model.IsExpanded)
            {
                ToggleSection(model);
                targetBlockViewModelBase = ItemFilterBlockViewModels[ItemFilterBlockViewModels.IndexOf(targetBlockViewModelBase) + model.ChildCount];
            }

            ExecuteCommandAndSelectAdded(new AddBlockCommand(Script, targetBlockViewModelBase?.BaseBlock));
            SetDirtyFlag();
        }

        private void OnAddCommentBlockCommand()
        {
            AddCommentBlock(LastSelectedBlockViewModel);
        }

        public void AddCommentBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            if (targetBlockViewModelBase is IItemFilterCommentBlockViewModel && !(targetBlockViewModelBase as IItemFilterCommentBlockViewModel).IsExpanded)
            {
                targetBlockViewModelBase = ItemFilterBlockViewModels[ItemFilterBlockViewModels.IndexOf(targetBlockViewModelBase) +
                    (targetBlockViewModelBase as ItemFilterCommentBlockViewModel).ChildCount];
            }

            ExecuteCommandAndSelectAdded(new AddCommentBlockCommand(Script, targetBlockViewModelBase?.BaseBlock));
            SetDirtyFlag();
        }

        private void OnDeleteBlockCommand()
        {
            var blocksToDelete = new List<IItemFilterBlockViewModelBase>();
            foreach (var block in SelectedBlockViewModels.OfType<IItemFilterBlockViewModelBase>())
            {
                blocksToDelete.Add(block);
                if (block is IItemFilterCommentBlockViewModel && !(block as IItemFilterCommentBlockViewModel).IsExpanded)
                {
                    blocksToDelete.AddRange(GetChildren(block as IItemFilterCommentBlockViewModel));
                }
            }

            DeleteBlocks(blocksToDelete);
        }

        public void DeleteBlock(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            if (targetBlockViewModelBase is IItemFilterCommentBlockViewModel && !(targetBlockViewModelBase as IItemFilterCommentBlockViewModel).IsExpanded)
            {
                _scriptCommandManager.ExecuteCommand(new RemoveBlocksCommand(Script,
                    Enumerable.Range(ItemFilterBlockViewModels.IndexOf(targetBlockViewModelBase),
                    (targetBlockViewModelBase as ItemFilterCommentBlockViewModel).ChildCount + 1).ToList()));
            }
            else
            {
                _scriptCommandManager.ExecuteCommand(new RemoveBlocksCommand(Script, targetBlockViewModelBase.BaseBlock));
            }

            SetDirtyFlag();
        }

        public void DeleteBlocks(IEnumerable<IItemFilterBlockViewModelBase> targetBlockViewModels)
        {
            var blockIndexes = new List<int>();
            foreach (var block in targetBlockViewModels)
            {
                blockIndexes.Add(ItemFilterBlockViewModels.IndexOf(block));
            }

            _scriptCommandManager.ExecuteCommand(new RemoveBlocksCommand(Script, blockIndexes));
            SetDirtyFlag();
        }

        private void OnMoveBlockToBottomCommand()
        {
            MoveBlocksToBottom(SelectedBlockViewModels.OfType<IItemFilterBlockViewModelBase>());
        }

        public void MoveBlockToBottom(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            if (targetBlockViewModelBase is IItemFilterCommentBlockViewModel && !(targetBlockViewModelBase as IItemFilterCommentBlockViewModel).IsExpanded)
            {
                ExecuteCommandAndSelectAdded(new MoveBlocksToBottomCommand(Script,
                    Enumerable.Range(ItemFilterBlockViewModels.IndexOf(targetBlockViewModelBase),
                    (targetBlockViewModelBase as ItemFilterCommentBlockViewModel).ChildCount + 1).ToList()));
                if (_lastAddedBlocks.Count > 0)
                {
                    ToggleSection(_lastAddedBlocks[0] as IItemFilterCommentBlockViewModel);
                }
            }
            else
            {
                _scriptCommandManager.ExecuteCommand(new MoveBlocksToBottomCommand(Script, targetBlockViewModelBase.BaseBlock));
            }

            SetDirtyFlag();
        }

        public void MoveBlocksToBottom(IEnumerable<IItemFilterBlockViewModelBase> targetCommentBlockViewModels)
        {
            var sourceIndexes = new List<int>();
            foreach (var block in targetCommentBlockViewModels)
            {
                sourceIndexes.Add(ItemFilterBlockViewModels.IndexOf(block));
                if (block is IItemFilterCommentBlockViewModel && !(block as IItemFilterCommentBlockViewModel).IsExpanded)
                {
                    sourceIndexes.AddRange(GetChildrenIndexes(block as IItemFilterCommentBlockViewModel));
                }
            }

            ExecuteCommandAndSelectAdded(new MoveBlocksToBottomCommand(Script, sourceIndexes));
            for (var i = ItemFilterBlockViewModels.Count - sourceIndexes.Count; i < ItemFilterBlockViewModels.Count; i++)
            {
                if (ItemFilterBlockViewModels[i] as IItemFilterCommentBlockViewModel != null)
                {
                    ToggleSection(ItemFilterBlockViewModels[i] as IItemFilterCommentBlockViewModel);
                }
            }

            SetDirtyFlag();
        }

        private void OnMoveBlockToTopCommand()
        {
            MoveBlocksToTop(SelectedBlockViewModels.OfType<IItemFilterBlockViewModelBase>());
        }

        public void MoveBlockToTop(IItemFilterBlockViewModelBase targetBlockViewModelBase)
        {
            if (targetBlockViewModelBase is IItemFilterCommentBlockViewModel model && !model.IsExpanded)
            {
                ExecuteCommandAndSelectAdded(new MoveBlocksToTopCommand(Script,
                    Enumerable.Range(ItemFilterBlockViewModels.IndexOf(model),
                    model.ChildCount + 1).ToList()));
                if (_lastAddedBlocks.Count > 0)
                {
                    ToggleSection(_lastAddedBlocks[0] as IItemFilterCommentBlockViewModel);
                }
            }
            else
            {
                ExecuteCommandAndSelectAdded(new MoveBlocksToTopCommand(Script, targetBlockViewModelBase.BaseBlock));
            }

            SetDirtyFlag();
        }

        public void MoveBlocksToTop(IEnumerable<IItemFilterBlockViewModelBase> targetCommentBlockViewModels)
        {
            var sourceIndexes = new List<int>();
            foreach (var block in targetCommentBlockViewModels)
            {
                sourceIndexes.Add(ItemFilterBlockViewModels.IndexOf(block));
                if (block is IItemFilterCommentBlockViewModel model && !model.IsExpanded)
                {
                    sourceIndexes.AddRange(GetChildrenIndexes(model));
                }
            }

            ExecuteCommandAndSelectAdded(new MoveBlocksToTopCommand(Script, sourceIndexes));
            for (var i = 0; i < sourceIndexes.Count; i++)
            {
                if (ItemFilterBlockViewModels[i] is IItemFilterCommentBlockViewModel model)
                {
                    ToggleSection(model);
                }
            }

            SetDirtyFlag();
        }

        public void OnEnableDropSoundsCommand()
        {
            ValidateSelectedBlocks();

            var input = new List<Tuple<ObservableCollection<IItemFilterBlockItem>, IItemFilterBlockItem>>();

            foreach (var block in SelectedBlockViewModels.OfType<IItemFilterBlockViewModel>())
            {
                var blockItems = block.Block.BlockItems;
                for (var i = 0; i < blockItems.Count; i++)
                {
                    var blockItem = blockItems[i];
                    if (blockItem is DisableDropSoundBlockItem)
                    {
                        input.Add(new Tuple<ObservableCollection<IItemFilterBlockItem>, IItemFilterBlockItem>(blockItems, blockItem));
                    }
                }
            }

            if (input.Count > 0)
            {
                _scriptCommandManager.ExecuteCommand(new RemoveBlockItemFromBlocksCommand(input));
                SetDirtyFlag();
            }
        }

        public void OnDisableDropSoundsCommand()
        {
            ValidateSelectedBlocks();

            var input = new List<Tuple<ObservableCollection<IItemFilterBlockItem>, IItemFilterBlockItem>>();

            foreach (var block in SelectedBlockViewModels.OfType<IItemFilterBlockViewModel>())
            {
                var blockItems = block.Block.BlockItems;
                var found = false;
                foreach (var item in blockItems)
                {
                    if (item is DisableDropSoundBlockItem)
                    {
                        found = true;
                    }
                }

                if (!found) {
                    var item = new DisableDropSoundBlockItem();
                    input.Add(new Tuple<ObservableCollection<IItemFilterBlockItem>, IItemFilterBlockItem>(blockItems, item));
                }
            }

            if (input.Count > 0)
            {
                _scriptCommandManager.ExecuteCommand(new AddBlockItemToBlocksCommand(input));
                SetDirtyFlag();
            }
        }

        private void OnBlockBecameDirty(object sender, EventArgs e)
        {
            SetDirtyFlag();
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

        private void OnDisableBlockCommand()
        {
            foreach (var block in SelectedBlockViewModels)
            {
                if (block is IItemFilterCommentBlockViewModel model)
                {
                    if (!model.IsExpanded)
                    {
                        foreach (var child in GetChildren(model))
                        {
                            ((IItemFilterBlockViewModel)child).BlockEnabled = false;
                        }
                    }
                }
                else
                {
                    (block as IItemFilterBlockViewModel).BlockEnabled = false;
                }
            }
        }

        private void OnEnableBlockCommand()
        {
            foreach (var block in SelectedBlockViewModels)
            {
                if (block is IItemFilterCommentBlockViewModel model)
                {
                    if (!model.IsExpanded)
                    {
                        foreach (var child in GetChildren(model))
                        {
                            ((IItemFilterBlockViewModel)child).BlockEnabled = true;
                        }
                    }
                }
                else
                {
                    (block as IItemFilterBlockViewModel).BlockEnabled = true;
                }
            }
        }

        private void OnDisableSectionCommand()
        {
            foreach (var block in SelectedBlockViewModels.OfType<IItemFilterCommentBlockViewModel>())
            {
                foreach (var child in GetChildren(block))
                {
                    ((IItemFilterBlockViewModel)child).BlockEnabled = false;
                }
            }
        }

        private void OnEnableSectionCommand()
        {
            foreach (var block in SelectedBlockViewModels.OfType<IItemFilterCommentBlockViewModel>())
            {
                foreach (var child in GetChildren(block))
                {
                    ((IItemFilterBlockViewModel)child).BlockEnabled = true;
                }
            }
        }

        private void OnExpandSectionCommand()
        {
            foreach (var block in SelectedBlockViewModels.OfType<IItemFilterCommentBlockViewModel>())
            {
                if (!block.IsExpanded)
                {
                    ToggleSection(block);
                }
            }
        }

        private void OnCollapseSectionCommand()
        {
            foreach (var block in SelectedBlockViewModels.OfType<IItemFilterCommentBlockViewModel>())
            {
                if (block.IsExpanded)
                {
                    ToggleSection(block);
                }
            }
        }

        public void ToggleSection(IItemFilterCommentBlockViewModel targetCommentBlockViewModelBase, bool deferViewUpdate = false)
        {
            var newState = !targetCommentBlockViewModelBase.IsExpanded;
            targetCommentBlockViewModelBase.IsExpanded = newState;
            foreach (var child in GetChildren(targetCommentBlockViewModelBase))
            {
                child.IsVisible = newState;
            }

            if (!deferViewUpdate)
            {
                ViewItemFilterBlockViewModels.Refresh();
                ValidateSelectedBlocks();
                RaisePropertyChanged(nameof(ViewItemFilterBlockViewModels));
            }
        }

        private void CollapseAllSections()
        {
            foreach (var model in ItemFilterBlockViewModels.OfType<IItemFilterCommentBlockViewModel>())
            {
                if (model.IsExpanded)
                {
                    ToggleSection(model, true);
                }
            }

            ViewItemFilterBlockViewModels.Refresh();
            ValidateSelectedBlocks();
            RaisePropertyChanged(nameof(ViewItemFilterBlockViewModels));
        }

        private void ExpandAllSections()
        {
            foreach (var model in ItemFilterBlockViewModels.OfType<IItemFilterCommentBlockViewModel>())
            {
                if (!model.IsExpanded)
                {
                    ToggleSection(model, true);
                }
            }

            ViewItemFilterBlockViewModels.Refresh();
            ValidateSelectedBlocks();
            RaisePropertyChanged(nameof(ViewItemFilterBlockViewModels));
        }

        private void ExecuteCommandAndSelectAdded(ICommand command)
        {
            _lastAddedBlocks.Clear();
            _scriptCommandManager.ExecuteCommand(command);
            SelectedBlockViewModels.Clear();
            foreach (var block in _lastAddedBlocks)
            {
                if (ViewItemFilterBlockViewModels.Contains(block))
                {
                    SelectedBlockViewModels.Add(block);
                }
            }

            ValidateSelectedBlocks();
        }
    }
}
