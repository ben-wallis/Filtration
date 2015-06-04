using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Filtration.Models;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    internal class LootFilterScriptViewModel : FiltrationViewModelBase, ILootFilterScriptViewModel
    {
        private readonly ILootFilterBlockViewModelFactory _lootFilterBlockViewModelFactory;
        private bool _isDirty;

        public LootFilterScriptViewModel(ILootFilterBlockViewModelFactory lootFilterBlockViewModelFactory )
        {
            DeleteBlockCommand = new RelayCommand(OnDeleteBlock, () => SelectedBlockViewModel != null);
            MoveBlockToTopCommand = new RelayCommand(OnMoveBlockToTopCommand, () => SelectedBlockViewModel != null);
            MoveBlockUpCommand = new RelayCommand(OnMoveBlockUpCommand, () => SelectedBlockViewModel != null);
            MoveBlockDownCommand = new RelayCommand(OnMoveBlockDownCommand, () => SelectedBlockViewModel != null);
            MoveBlockToBottomCommand = new RelayCommand(OnMoveBlockToBottomCommand, () => SelectedBlockViewModel != null);
            AddBlockAboveCommand = new RelayCommand(OnAddBlockAboveCommand, () => SelectedBlockViewModel != null || LootFilterBlockViewModels.Count == 0);
            AddBlockBelowCommand = new RelayCommand(OnAddBlockBelowCommand, () => SelectedBlockViewModel != null);

            AddSectionAboveCommand = new RelayCommand(OnAddSectionAboveCommand, () => SelectedBlockViewModel != null);

            _lootFilterBlockViewModelFactory = lootFilterBlockViewModelFactory;
            LootFilterBlockViewModels = new ObservableCollection<ILootFilterBlockViewModel>();

        }

        public RelayCommand DeleteBlockCommand { get; private set; }
        public RelayCommand MoveBlockToTopCommand { get; private set; }
        public RelayCommand MoveBlockUpCommand { get; private set; }
        public RelayCommand MoveBlockDownCommand { get; private set; }
        public RelayCommand MoveBlockToBottomCommand { get; private set; }
        public RelayCommand AddBlockAboveCommand { get; private set; }
        public RelayCommand AddBlockBelowCommand { get; private set; }
        public RelayCommand AddSectionAboveCommand { get; private set; }

        public ObservableCollection<ILootFilterBlockViewModel> LootFilterBlockViewModels { get; private set; }

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

        public LootFilterBlockViewModel SelectedBlockViewModel { get; set; }

        public LootFilterScript Script { get; private set; }

        public bool IsDirty
        {
            get { return _isDirty || HasDirtyChildren; }
            set { _isDirty = value; }
        }

        private bool HasDirtyChildren
        {
            get { return LootFilterBlockViewModels.Any(vm => vm.IsDirty); }
        }

        private void CleanChildren()
        {
            foreach (var vm in LootFilterBlockViewModels)
            {
                vm.IsDirty = false;
            }
        }

        public void RemoveDirtyFlag()
        {
            CleanChildren();
            IsDirty = false;
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

        public void Initialise(LootFilterScript lootFilterScript)
        {
            LootFilterBlockViewModels.Clear();

            Script = lootFilterScript;
            foreach (var block in Script.LootFilterBlocks)
            {
                var vm = _lootFilterBlockViewModelFactory.Create();
                vm.Initialise(block);
                LootFilterBlockViewModels.Add(vm);
            }
        }

        private void OnMoveBlockToTopCommand()
        {
            var currentIndex = LootFilterBlockViewModels.IndexOf(SelectedBlockViewModel);

            if (currentIndex > 0)
            {
                var block = SelectedBlockViewModel.Block;
                Script.LootFilterBlocks.Remove(block);
                Script.LootFilterBlocks.Insert(0, block);
                LootFilterBlockViewModels.Move(currentIndex, 0);
                _isDirty = true;
            }
        }

        private void OnMoveBlockUpCommand()
        {
            var currentIndex = LootFilterBlockViewModels.IndexOf(SelectedBlockViewModel);

            if (currentIndex > 0)
            {
                var block = SelectedBlockViewModel.Block;
                var blockPos = Script.LootFilterBlocks.IndexOf(block);
                Script.LootFilterBlocks.RemoveAt(blockPos);
                Script.LootFilterBlocks.Insert(blockPos - 1, block);
                LootFilterBlockViewModels.Move(currentIndex, currentIndex - 1);
                _isDirty = true;
            }
        }

        private void OnMoveBlockDownCommand()
        {
            var currentIndex = LootFilterBlockViewModels.IndexOf(SelectedBlockViewModel);

            if (currentIndex < LootFilterBlockViewModels.Count - 1)
            {
                var block = SelectedBlockViewModel.Block;
                var blockPos = Script.LootFilterBlocks.IndexOf(block);
                Script.LootFilterBlocks.RemoveAt(blockPos);
                Script.LootFilterBlocks.Insert(blockPos + 1, block);
                LootFilterBlockViewModels.Move(currentIndex, currentIndex + 1);
                _isDirty = true;
            }
        }

        private void OnMoveBlockToBottomCommand()
        {
            var currentIndex = LootFilterBlockViewModels.IndexOf(SelectedBlockViewModel);

            if (currentIndex < LootFilterBlockViewModels.Count - 1)
            {
                var block = SelectedBlockViewModel.Block;
                Script.LootFilterBlocks.Remove(block);
                Script.LootFilterBlocks.Add(block);
                LootFilterBlockViewModels.Move(currentIndex, LootFilterBlockViewModels.Count - 1);
                _isDirty = true;
            }
        }

        private void OnAddBlockAboveCommand()
        {
            var vm = _lootFilterBlockViewModelFactory.Create();
            var newBlock = new LootFilterBlock();
            vm.Initialise(newBlock);

            if (LootFilterBlockViewModels.Count > 0)
            {
                Script.LootFilterBlocks.Insert(Script.LootFilterBlocks.IndexOf(SelectedBlockViewModel.Block), newBlock);
                LootFilterBlockViewModels.Insert(LootFilterBlockViewModels.IndexOf(SelectedBlockViewModel), vm);
            }
            else
            {
                Script.LootFilterBlocks.Add(newBlock);
                LootFilterBlockViewModels.Add(vm);
            }

            _isDirty = true;
        }

        private void OnAddBlockBelowCommand()
        {
            var vm = _lootFilterBlockViewModelFactory.Create();
            var newBlock = new LootFilterBlock();
            vm.Initialise(newBlock);

            Script.LootFilterBlocks.Insert(Script.LootFilterBlocks.IndexOf(SelectedBlockViewModel.Block) + 1, newBlock);
            LootFilterBlockViewModels.Insert(LootFilterBlockViewModels.IndexOf(SelectedBlockViewModel) + 1, vm);
            _isDirty = true;
        }

        private void OnAddSectionAboveCommand()
        {
            var vm = _lootFilterBlockViewModelFactory.Create();
            var newSection = new LootFilterSection { Description = "New Section" };
            vm.Initialise(newSection);

            Script.LootFilterBlocks.Insert(Script.LootFilterBlocks.IndexOf(SelectedBlockViewModel.Block), newSection);
            LootFilterBlockViewModels.Insert(LootFilterBlockViewModels.IndexOf(SelectedBlockViewModel), vm);
            _isDirty = true;
        }

        private void OnDeleteBlock()
        {
            var result = MessageBox.Show("Are you sure you wish to delete this block?", "Delete Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Script.LootFilterBlocks.Remove(SelectedBlockViewModel.Block);
                LootFilterBlockViewModels.Remove(SelectedBlockViewModel);
                _isDirty = true;
            }
            SelectedBlockViewModel = null;
        }
    }
}
