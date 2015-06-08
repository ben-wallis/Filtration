using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Castle.Core.Internal;
using Filtration.Models;
using Filtration.Translators;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    internal interface IItemFilterScriptViewModel
    {
        ItemFilterScript Script { get; }
        bool IsDirty { get; }
        string Description { get; set; }
        string DisplayName { get; }
        void Initialise(ItemFilterScript itemFilterScript);
        IItemFilterBlockViewModel SelectedBlockViewModel { get; set; }
        void RemoveDirtyFlag();
        void AddSection(IItemFilterBlockViewModel targetBlockViewModel);
        void AddBlock(IItemFilterBlockViewModel targetBlockViewModel);
        void CopyBlock(IItemFilterBlockViewModel targetBlockViewModel);
        void PasteBlock(IItemFilterBlockViewModel targetBlockViewModel);
    }

    internal class ItemFilterScriptViewModel : FiltrationViewModelBase, IItemFilterScriptViewModel
    {
        private readonly IItemFilterBlockViewModelFactory _itemFilterBlockViewModelFactory;
        private readonly IItemFilterBlockTranslator _blockTranslator;
        private bool _isDirty;
        private IItemFilterBlockViewModel _selectedBlockViewModel;

        public ItemFilterScriptViewModel(IItemFilterBlockViewModelFactory itemFilterBlockViewModelFactory, IItemFilterBlockTranslator blockTranslator)
        {
            DeleteBlockCommand = new RelayCommand(OnDeleteBlockCommand, () => SelectedBlockViewModel != null);
            MoveBlockToTopCommand = new RelayCommand(OnMoveBlockToTopCommand, () => SelectedBlockViewModel != null);
            MoveBlockUpCommand = new RelayCommand(OnMoveBlockUpCommand, () => SelectedBlockViewModel != null);
            MoveBlockDownCommand = new RelayCommand(OnMoveBlockDownCommand, () => SelectedBlockViewModel != null);
            MoveBlockToBottomCommand = new RelayCommand(OnMoveBlockToBottomCommand, () => SelectedBlockViewModel != null);
            AddBlockCommand = new RelayCommand(OnAddBlockCommand);
            AddSectionCommand = new RelayCommand(OnAddSectionCommand, () => SelectedBlockViewModel != null);
            CopyBlockCommand = new RelayCommand(OnCopyBlockCommand, () => SelectedBlockViewModel != null);
            PasteBlockCommand = new RelayCommand(OnPasteBlockCommand, () => SelectedBlockViewModel != null);
            _itemFilterBlockViewModelFactory = itemFilterBlockViewModelFactory;
            _blockTranslator = blockTranslator;
            ItemFilterBlockViewModels = new ObservableCollection<IItemFilterBlockViewModel>();

        }

        public RelayCommand DeleteBlockCommand { get; private set; }
        public RelayCommand MoveBlockToTopCommand { get; private set; }
        public RelayCommand MoveBlockUpCommand { get; private set; }
        public RelayCommand MoveBlockDownCommand { get; private set; }
        public RelayCommand MoveBlockToBottomCommand { get; private set; }
        public RelayCommand AddBlockCommand { get; private set; }
        public RelayCommand AddSectionCommand { get; private set; }
        public RelayCommand CopyBlockCommand { get; private set; }
        public RelayCommand PasteBlockCommand { get; private set; }

        public ObservableCollection<IItemFilterBlockViewModel> ItemFilterBlockViewModels { get; private set; }

        public IEnumerable<IItemFilterBlockViewModel> ItemFilterSectionViewModels
        {
            get { return ItemFilterBlockViewModels.Where(b => b.Block.GetType() == typeof (ItemFilterSection)); }
        }

        public IItemFilterBlockViewModel SectionBrowserSelectedViewModel { get; set; }

        public string Description
        {
            get { return Script.Description; }
            set
            {
                Script.Description = value;
                _isDirty = true;
                RaisePropertyChanged();
            }
        }

        public IItemFilterBlockViewModel SelectedBlockViewModel
        {
            get { return _selectedBlockViewModel; }
            set
            {
                _selectedBlockViewModel = value;
                RaisePropertyChanged();
            }
        }

        public ItemFilterScript Script { get; private set; }

        public bool IsDirty
        {
            get { return _isDirty || HasDirtyChildren; }
            set
            {
                _isDirty = value;
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

        public void RemoveDirtyFlag()
        {
            CleanChildren();
            IsDirty = false;
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

        public void Initialise(ItemFilterScript itemFilterScript)
        {
            ItemFilterBlockViewModels.Clear();

            Script = itemFilterScript;
            foreach (var block in Script.ItemFilterBlocks)
            {
                var vm = _itemFilterBlockViewModelFactory.Create();
                vm.Initialise(block, this);
                ItemFilterBlockViewModels.Add(vm);
            }
        }

        private void OnCopyBlockCommand()
        {
            CopyBlock(SelectedBlockViewModel);
        }

        public void CopyBlock(IItemFilterBlockViewModel targetBlockViewModel)
        {
            Clipboard.SetText(_blockTranslator.TranslateItemFilterBlockToString(SelectedBlockViewModel.Block));
        }

        private void OnPasteBlockCommand()
        {
            PasteBlock(SelectedBlockViewModel);
        }

        public void PasteBlock(IItemFilterBlockViewModel targetBlockViewModel)
        {
            var clipboardText = Clipboard.GetText();
            if (clipboardText.IsNullOrEmpty()) return;
            
            var translatedBlock = _blockTranslator.TranslateStringToItemFilterBlock(clipboardText);
            if (translatedBlock == null) return;

            var vm = _itemFilterBlockViewModelFactory.Create();
            vm.Initialise(translatedBlock, this);

            if (ItemFilterBlockViewModels.Count > 0)
            {
                Script.ItemFilterBlocks.Insert(Script.ItemFilterBlocks.IndexOf(targetBlockViewModel.Block) + 1, translatedBlock);
                ItemFilterBlockViewModels.Insert(ItemFilterBlockViewModels.IndexOf(targetBlockViewModel) + 1, vm);
            }
            else
            {
                Script.ItemFilterBlocks.Add(translatedBlock);
                ItemFilterBlockViewModels.Add(vm);
            }

            SelectedBlockViewModel = vm;
            _isDirty = true;

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
                _isDirty = true;
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
                _isDirty = true;
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
                _isDirty = true;
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
                _isDirty = true;
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
            _isDirty = true;
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

            Script.ItemFilterBlocks.Insert(Script.ItemFilterBlocks.IndexOf(targetBlockViewModel.Block) + 1, newSection);
            ItemFilterBlockViewModels.Insert(ItemFilterBlockViewModels.IndexOf(targetBlockViewModel) + 1, vm);
            _isDirty = true;
            SelectedBlockViewModel = vm;
            RaisePropertyChanged("ItemFilterSectionViewModels");
        }

        private void OnDeleteBlockCommand()
        {
            DeleteBlock(SelectedBlockViewModel);
        }

        public void DeleteBlock(IItemFilterBlockViewModel targetBlockViewModel)
        {
            var result = MessageBox.Show("Are you sure you wish to delete this block?", "Delete Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Script.ItemFilterBlocks.Remove(targetBlockViewModel.Block);
                ItemFilterBlockViewModels.Remove(targetBlockViewModel);
                _isDirty = true;
            }
            SelectedBlockViewModel = null;
        }
    }
}
