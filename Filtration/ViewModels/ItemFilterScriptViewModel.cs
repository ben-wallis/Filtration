using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Castle.Core.Internal;
using Filtration.Common.Services;
using Filtration.Common.ViewModels;
using Filtration.Interface;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.Services;
using Filtration.Translators;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using Clipboard = System.Windows.Clipboard;

namespace Filtration.ViewModels
{
    internal interface IItemFilterScriptViewModel : IEditableDocument
    {
        ItemFilterScript Script { get; }
        IItemFilterBlockViewModel SelectedBlockViewModel { get; set; }
        IItemFilterBlockViewModel SectionBrowserSelectedBlockViewModel { get; set; }
        IEnumerable<IItemFilterBlockViewModel> ItemFilterSectionViewModels { get; }
        Predicate<IItemFilterBlockViewModel> BlockFilterPredicate { get; set; }
        
        bool ShowAdvanced { get; }
        string Description { get; set; }
        string DisplayName { get; }
        
        void Initialise(ItemFilterScript itemFilterScript, bool newScript);
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

        void AddSection(IItemFilterBlockViewModel targetBlockViewModel);
        void AddBlock(IItemFilterBlockViewModel targetBlockViewModel);
        void CopyBlock(IItemFilterBlockViewModel targetBlockViewModel);
        void CopyBlockStyle(IItemFilterBlockViewModel targetBlockViewModel);
        void PasteBlock(IItemFilterBlockViewModel targetBlockViewModel);
        void PasteBlockStyle(IItemFilterBlockViewModel targetBlockViewModel);
    }

    internal class ItemFilterScriptViewModel : PaneViewModel, IItemFilterScriptViewModel
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IItemFilterBlockViewModelFactory _itemFilterBlockViewModelFactory;
        private readonly IItemFilterBlockTranslator _blockTranslator;
        private readonly IAvalonDockWorkspaceViewModel _avalonDockWorkspaceViewModel;
        private readonly IItemFilterPersistenceService _persistenceService;
        private readonly IMessageBoxService _messageBoxService;

        private bool _isDirty;
        private IItemFilterBlockViewModel _selectedBlockViewModel;
        private IItemFilterBlockViewModel _sectionBrowserSelectedBlockViewModel;
        private readonly ObservableCollection<IItemFilterBlockViewModel> _itemFilterBlockViewModels;
        private ICollectionView _itemFilterBlockViewModelsCollectionView;
        private Predicate<IItemFilterBlockViewModel> _blockFilterPredicate;

        public ItemFilterScriptViewModel(IItemFilterBlockViewModelFactory itemFilterBlockViewModelFactory,
                                         IItemFilterBlockTranslator blockTranslator,
                                         IAvalonDockWorkspaceViewModel avalonDockWorkspaceViewModel,
                                         IItemFilterPersistenceService persistenceService,
                                         IMessageBoxService messageBoxService)
        {
            _itemFilterBlockViewModelFactory = itemFilterBlockViewModelFactory;
            _blockTranslator = blockTranslator;
            _avalonDockWorkspaceViewModel = avalonDockWorkspaceViewModel;
            _avalonDockWorkspaceViewModel.ActiveDocumentChanged += OnActiveDocumentChanged;
            _persistenceService = persistenceService;
            _messageBoxService = messageBoxService;
            _itemFilterBlockViewModels = new ObservableCollection<IItemFilterBlockViewModel>();
            
            ToggleShowAdvancedCommand = new RelayCommand<bool>(OnToggleShowAdvancedCommand);
            ClearFilterCommand = new RelayCommand(OnClearFilterCommand, () => BlockFilterPredicate != null);
            CloseCommand = new RelayCommand(OnCloseCommand);
            DeleteBlockCommand = new RelayCommand(OnDeleteBlockCommand, () => SelectedBlockViewModel != null);
            MoveBlockToTopCommand = new RelayCommand(OnMoveBlockToTopCommand, () => SelectedBlockViewModel != null);
            MoveBlockUpCommand = new RelayCommand(OnMoveBlockUpCommand, () => SelectedBlockViewModel != null);
            MoveBlockDownCommand = new RelayCommand(OnMoveBlockDownCommand, () => SelectedBlockViewModel != null);
            MoveBlockToBottomCommand = new RelayCommand(OnMoveBlockToBottomCommand, () => SelectedBlockViewModel != null);
            AddBlockCommand = new RelayCommand(OnAddBlockCommand);
            AddSectionCommand = new RelayCommand(OnAddSectionCommand, () => SelectedBlockViewModel != null);
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

        public RelayCommand<bool> ToggleShowAdvancedCommand { get; private set; }
        public RelayCommand ClearFilterCommand { get; private set; }
        public RelayCommand CloseCommand { get; private set; }
        public RelayCommand DeleteBlockCommand { get; private set; }
        public RelayCommand MoveBlockToTopCommand { get; private set; }
        public RelayCommand MoveBlockUpCommand { get; private set; }
        public RelayCommand MoveBlockDownCommand { get; private set; }
        public RelayCommand MoveBlockToBottomCommand { get; private set; }
        public RelayCommand AddBlockCommand { get; private set; }
        public RelayCommand AddSectionCommand { get; private set; }
        public RelayCommand EnableBlockCommand { get; private set; }
        public RelayCommand DisableBlockCommand { get; private set; }
        public RelayCommand CopyBlockCommand { get; private set; }
        public RelayCommand CopyBlockStyleCommand { get; private set; }
        public RelayCommand PasteBlockCommand { get; private set; }
        public RelayCommand PasteBlockStyleCommand { get; private set; }
        public RelayCommand ExpandAllBlocksCommand { get; private set; }
        public RelayCommand CollapseAllBlocksCommand { get; private set; }

        public ObservableCollection<IItemFilterBlockViewModel> ItemFilterBlockViewModels
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
            get { return _blockFilterPredicate; }
            set
            {
                _blockFilterPredicate = value;
                RaisePropertyChanged("ItemFilterBlockViewModels");
            }
        }

        public IEnumerable<IItemFilterBlockViewModel> ItemFilterSectionViewModels
        {
            get { return ItemFilterBlockViewModels.Where(b => b.Block.GetType() == typeof (ItemFilterSection)); }
        }

        public bool IsScript { get { return true; } }
        public bool IsTheme { get { return false; } }

        public string Description
        {
            get { return Script.Description; }
            set
            {
                Script.Description = value;
                IsDirty = true;
                RaisePropertyChanged();
            }
        }

        public bool ShowAdvanced
        {
            get { return _showAdvanced; }
            private set
            {
                _showAdvanced = value;
                RaisePropertyChanged();
                RaisePropertyChanged("ItemFilterBlockViewModels");
            }
        }

        public bool HasSelectedBlock()
        {
            return SelectedBlockViewModel != null;
        }

        public bool HasSelectedEnabledBlock()
        {
            return HasSelectedBlock() && !(SelectedBlockViewModel.Block is ItemFilterSection) && SelectedBlockViewModel.BlockEnabled;
        }

        public bool HasSelectedDisabledBlock()
        {
            return HasSelectedBlock() && !(SelectedBlockViewModel.Block is ItemFilterSection) && !SelectedBlockViewModel.BlockEnabled;
        }

        public IItemFilterBlockViewModel SelectedBlockViewModel
        {
            get { return _selectedBlockViewModel; }
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

        public IItemFilterBlockViewModel SectionBrowserSelectedBlockViewModel
        {
            get { return _sectionBrowserSelectedBlockViewModel; }
            set
            {
                _sectionBrowserSelectedBlockViewModel = value;
                SelectedBlockViewModel = value;
                RaisePropertyChanged();
            }
        }

        public ItemFilterScript Script { get; private set; }

        public bool IsDirty
        {
            get { return _isDirty || HasDirtyChildren; }
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
            RaisePropertyChanged("Filename");
            RaisePropertyChanged("DisplayName");
        }

        public void SetDirtyFlag()
        {
            IsDirty = true;
            RaisePropertyChanged("Filename");
            RaisePropertyChanged("DisplayName");
        }

        public string DisplayName
        {
            get { return !string.IsNullOrEmpty(Filename) ? Filename : Description; }
        }

        public string Filename
        {
            get { return Path.GetFileName(Script.FilePath); }
        }

        public string Filepath
        {
            get { return Script.FilePath; }
        }

        private bool _filenameIsFake;
        private bool _showAdvanced;

        public void Initialise(ItemFilterScript itemFilterScript, bool newScript)
        {
            ItemFilterBlockViewModels.Clear();

            Script = itemFilterScript;
            foreach (var block in Script.ItemFilterBlocks)
            {
                var vm = _itemFilterBlockViewModelFactory.Create();
                vm.Initialise(block, this);
                ItemFilterBlockViewModels.Add(vm);
            }
           
            _filenameIsFake = newScript;

            if (newScript)
            {
                Script.FilePath = "Untitled.filter";
            }

            if (ItemFilterBlockViewModels.Count > 0)
            {
                SelectedBlockViewModel = ItemFilterBlockViewModels.First();
            }

            Title = Filename;
            ContentId = "ScriptContentId";
        }

        public void Save()
        {
            if (!ValidateScript()) return;
            if (!CheckForUnusedThemeComponents()) return;

            if (_filenameIsFake)
            {
                SaveAs();
                return;
            }

            try
            {
                _persistenceService.SaveItemFilterScript(Script);
                RemoveDirtyFlag();
            }
            catch (Exception e)
            {
                if (_logger.IsErrorEnabled)
                {
                    _logger.Error(e);
                }

                _messageBoxService.Show("Save Error", "Error saving filter file - " + e.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void SaveAs()
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

            var previousFilePath = Script.FilePath;
            try
            {
                Script.FilePath = saveDialog.FileName;
                _persistenceService.SaveItemFilterScript(Script);
                _filenameIsFake = false;
                Title = Filename;
                RemoveDirtyFlag();
            }
            catch (Exception e)
            {
                if (_logger.IsErrorEnabled)
                {
                    _logger.Error(e);
                }

                _messageBoxService.Show("Save Error", "Error saving filter file - " + e.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Script.FilePath = previousFilePath;
            }
        }

        private bool CheckForUnusedThemeComponents()
        {
            var unusedThemeComponents =
                Script.ThemeComponents.Where(
                    t =>
                        Script.ItemFilterBlocks.Count(
                            b => b.BlockItems.OfType<ColorBlockItem>().Count(i => i.ThemeComponent == t) > 0) == 0).ToList();

            if (unusedThemeComponents.Count <= 0) return true;

            var themeComponents = unusedThemeComponents.Aggregate(string.Empty,
                (current, themeComponent) => current + (themeComponent.ComponentName + Environment.NewLine));

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

        private void OnCloseCommand()
        {
            Close();
        }
        
        public void Close()
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
                            Save();
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

        public void CopyBlock(IItemFilterBlockViewModel targetBlockViewModel)
        {
            Clipboard.SetText(_blockTranslator.TranslateItemFilterBlockToString(SelectedBlockViewModel.Block));
        }

        private void OnCopyBlockStyleCommand()
        {
            CopyBlockStyle(SelectedBlockViewModel);
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

            Clipboard.SetText(outputText);
        }

        private void OnPasteBlockStyleCommand()
        {
            PasteBlockStyle(SelectedBlockViewModel);
        }

        public void PasteBlockStyle(IItemFilterBlockViewModel targetBlockViewModel)
        {
            var clipboardText = Clipboard.GetText();
            if (string.IsNullOrEmpty(clipboardText))
            {
                return;
            }

            _blockTranslator.ReplaceColorBlockItemsFromString(targetBlockViewModel.Block.BlockItems, clipboardText);
            targetBlockViewModel.RefreshBlockPreview();
        }

        private void OnPasteBlockCommand()
        {
            PasteBlock(SelectedBlockViewModel);
        }

        public void PasteBlock(IItemFilterBlockViewModel targetBlockViewModel)
        {
            try
            {
                var clipboardText = Clipboard.GetText();
                if (clipboardText.IsNullOrEmpty()) return;

                var translatedBlock = _blockTranslator.TranslateStringToItemFilterBlock(clipboardText, Script.ThemeComponents);
                if (translatedBlock == null) return;

                var vm = _itemFilterBlockViewModelFactory.Create();
                vm.Initialise(translatedBlock, this);

                if (ItemFilterBlockViewModels.Count > 0)
                {
                    Script.ItemFilterBlocks.Insert(Script.ItemFilterBlocks.IndexOf(targetBlockViewModel.Block) + 1,
                        translatedBlock);
                    ItemFilterBlockViewModels.Insert(ItemFilterBlockViewModels.IndexOf(targetBlockViewModel) + 1, vm);
                }
                else
                {
                    Script.ItemFilterBlocks.Add(translatedBlock);
                    ItemFilterBlockViewModels.Add(vm);
                }

                SelectedBlockViewModel = vm;
                IsDirty = true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                _messageBoxService.Show("Paste Error",
                    e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine +
                    e.InnerException.Message + Environment.NewLine + e.InnerException.StackTrace, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OnMoveBlockToTopCommand()
        {
            MoveBlockToTop(SelectedBlockViewModel);
        }

        public void MoveBlockToTop(IItemFilterBlockViewModel targetBlockViewModel)
        {
            var currentIndex = ItemFilterBlockViewModels.IndexOf(targetBlockViewModel);

            if (currentIndex > 0)
            {
                var block = targetBlockViewModel.Block;
                Script.ItemFilterBlocks.Remove(block);
                Script.ItemFilterBlocks.Insert(0, block);
                ItemFilterBlockViewModels.Move(currentIndex, 0);
                IsDirty = true;
                RaisePropertyChanged("ItemFilterSectionViewModels");
            }
        }

        private void OnMoveBlockUpCommand()
        {
            MoveBlockUp(SelectedBlockViewModel);
        }

        public void MoveBlockUp(IItemFilterBlockViewModel targetBlockViewModel)
        {
            var currentIndex = ItemFilterBlockViewModels.IndexOf(targetBlockViewModel);

            if (currentIndex > 0)
            {
                var block = targetBlockViewModel.Block;
                var blockPos = Script.ItemFilterBlocks.IndexOf(block);
                Script.ItemFilterBlocks.RemoveAt(blockPos);
                Script.ItemFilterBlocks.Insert(blockPos - 1, block);
                ItemFilterBlockViewModels.Move(currentIndex, currentIndex - 1);
                IsDirty = true;
                RaisePropertyChanged("ItemFilterSectionViewModels");
            }
        }

        private void OnMoveBlockDownCommand()
        {
            MoveBlockDown(SelectedBlockViewModel);
        }

        public void MoveBlockDown(IItemFilterBlockViewModel targetBlockViewModel)
        {
            var currentIndex = ItemFilterBlockViewModels.IndexOf(targetBlockViewModel);

            if (currentIndex < ItemFilterBlockViewModels.Count - 1)
            {
                var block = targetBlockViewModel.Block;
                var blockPos = Script.ItemFilterBlocks.IndexOf(block);
                Script.ItemFilterBlocks.RemoveAt(blockPos);
                Script.ItemFilterBlocks.Insert(blockPos + 1, block);
                ItemFilterBlockViewModels.Move(currentIndex, currentIndex + 1);
                IsDirty = true;
                RaisePropertyChanged("ItemFilterSectionViewModels");
            }
        }

        private void OnMoveBlockToBottomCommand()
        {
            MoveBlockToBottom(SelectedBlockViewModel);
        }

        public void MoveBlockToBottom(IItemFilterBlockViewModel targetBlockViewModel)
        {
            var currentIndex = ItemFilterBlockViewModels.IndexOf(targetBlockViewModel);

            if (currentIndex < ItemFilterBlockViewModels.Count - 1)
            {
                var block = targetBlockViewModel.Block;
                Script.ItemFilterBlocks.Remove(block);
                Script.ItemFilterBlocks.Add(block);
                ItemFilterBlockViewModels.Move(currentIndex, ItemFilterBlockViewModels.Count - 1);
                IsDirty = true;
                RaisePropertyChanged("ItemFilterSectionViewModels");
            }
        }

        private void OnAddBlockCommand()
        {
            AddBlock(SelectedBlockViewModel);
        }

        public void AddBlock(IItemFilterBlockViewModel targetBlockViewModel)
        {
            var vm = _itemFilterBlockViewModelFactory.Create();
            var newBlock = new ItemFilterBlock();
            vm.Initialise(newBlock, this);

            if (targetBlockViewModel != null)
            {
                Script.ItemFilterBlocks.Insert(Script.ItemFilterBlocks.IndexOf(targetBlockViewModel.Block) + 1, newBlock);
                ItemFilterBlockViewModels.Insert(ItemFilterBlockViewModels.IndexOf(targetBlockViewModel) + 1, vm);
            }
            else
            {
                Script.ItemFilterBlocks.Add(newBlock);
                ItemFilterBlockViewModels.Add(vm);
            }

            SelectedBlockViewModel = vm;
            vm.IsExpanded = true;
            IsDirty = true;
        }

        private void OnAddSectionCommand()
        {
            AddSection(SelectedBlockViewModel);
        }
        
        public void AddSection(IItemFilterBlockViewModel targetBlockViewModel)
        {
            var vm = _itemFilterBlockViewModelFactory.Create();
            var newSection = new ItemFilterSection { Description = "New Section" };
            vm.Initialise(newSection, this);

            Script.ItemFilterBlocks.Insert(Script.ItemFilterBlocks.IndexOf(targetBlockViewModel.Block), newSection);
            ItemFilterBlockViewModels.Insert(ItemFilterBlockViewModels.IndexOf(targetBlockViewModel), vm);
            IsDirty = true;
            SelectedBlockViewModel = vm;
            RaisePropertyChanged("ItemFilterSectionViewModels");
        }

        private void OnExpandAllBlocksCommand()
        {
            foreach (var blockViewModel in ItemFilterBlockViewModels)
            {
                blockViewModel.IsExpanded = true;
            }
        }

        private void OnCollapseAllBlocksCommand()
        {
            foreach (var blockViewModel in ItemFilterBlockViewModels)
            {
                blockViewModel.IsExpanded = false;
            }
        }

        private void OnDeleteBlockCommand()
        {
            DeleteBlock(SelectedBlockViewModel);
        }

        public void DeleteBlock(IItemFilterBlockViewModel targetBlockViewModel)
        {
            var result = _messageBoxService.Show("Delete Confirmation", "Are you sure you wish to delete this block?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Script.ItemFilterBlocks.Remove(targetBlockViewModel.Block);
                ItemFilterBlockViewModels.Remove(targetBlockViewModel);
                IsDirty = true;
            }
            SelectedBlockViewModel = null;
        }

        private void OnDisableBlockCommand()
        {
            DisableBlock(SelectedBlockViewModel);
        }

        private void DisableBlock(IItemFilterBlockViewModel targetBlockViewModel)
        {
            targetBlockViewModel.BlockEnabled = false;
        }

        private void OnEnableBlockCommand()
        {
            EnableBlock(SelectedBlockViewModel);
        }

        private void EnableBlock(IItemFilterBlockViewModel targetBlockViewModel)
        {
            targetBlockViewModel.BlockEnabled = true;
        }
    }
}
