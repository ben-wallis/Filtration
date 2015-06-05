using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Filtration.Models;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    internal interface ILootFilterScriptViewModel
    {
        LootFilterScript Script { get; }
        bool IsDirty { get; }
        string Description { get; set; }
        void Initialise(LootFilterScript lootFilterScript);
        void RemoveDirtyFlag();
        void AddSection(ILootFilterBlockViewModel blockViewModel);
        void AddBlock(ILootFilterBlockViewModel blockViewModel);
    }

    internal class LootFilterScriptViewModel : FiltrationViewModelBase, ILootFilterScriptViewModel
    {
        private readonly ILootFilterBlockViewModelFactory _lootFilterBlockViewModelFactory;
        private bool _isDirty;
        private ILootFilterBlockViewModel _selectedBlockViewModel;

        public LootFilterScriptViewModel(ILootFilterBlockViewModelFactory lootFilterBlockViewModelFactory )
        {
            DeleteBlockCommand = new RelayCommand(OnDeleteBlockCommand, () => SelectedBlockViewModel != null);
            MoveBlockToTopCommand = new RelayCommand(OnMoveBlockToTopCommand, () => SelectedBlockViewModel != null);
            MoveBlockUpCommand = new RelayCommand(OnMoveBlockUpCommand, () => SelectedBlockViewModel != null);
            MoveBlockDownCommand = new RelayCommand(OnMoveBlockDownCommand, () => SelectedBlockViewModel != null);
            MoveBlockToBottomCommand = new RelayCommand(OnMoveBlockToBottomCommand, () => SelectedBlockViewModel != null);
            AddBlockCommand = new RelayCommand(OnAddBlockCommand, () => SelectedBlockViewModel != null);
            AddSectionCommand = new RelayCommand(OnAddSectionCommand, () => SelectedBlockViewModel != null);

            _lootFilterBlockViewModelFactory = lootFilterBlockViewModelFactory;
            LootFilterBlockViewModels = new ObservableCollection<ILootFilterBlockViewModel>();

        }

        public RelayCommand DeleteBlockCommand { get; private set; }
        public RelayCommand MoveBlockToTopCommand { get; private set; }
        public RelayCommand MoveBlockUpCommand { get; private set; }
        public RelayCommand MoveBlockDownCommand { get; private set; }
        public RelayCommand MoveBlockToBottomCommand { get; private set; }
        public RelayCommand AddBlockCommand { get; private set; }
        public RelayCommand AddSectionCommand { get; private set; }

        public ObservableCollection<ILootFilterBlockViewModel> LootFilterBlockViewModels { get; private set; }

        public IEnumerable<ILootFilterBlockViewModel> LootFilterSectionViewModels
        {
            get { return LootFilterBlockViewModels.Where(b => b.Block.GetType() == typeof (LootFilterSection)); }
        }

        public ILootFilterBlockViewModel SectionBrowserSelectedViewModel { get; set; }

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

        public ILootFilterBlockViewModel SelectedBlockViewModel
        {
            get { return _selectedBlockViewModel; }
            set
            {
                _selectedBlockViewModel = value;
                RaisePropertyChanged();
            }
        }

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
                vm.Initialise(block, this);
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
                RaisePropertyChanged("LootFilterSectionViewModels");
            }
        }

        private void OnMoveBlockUpCommand()
        {
            MoveBlockUp(SelectedBlockViewModel);
        }

        public void MoveBlockUp(ILootFilterBlockViewModel blockViewModel)
        {
            var currentIndex = LootFilterBlockViewModels.IndexOf(blockViewModel);

            if (currentIndex > 0)
            {
                var block = blockViewModel.Block;
                var blockPos = Script.LootFilterBlocks.IndexOf(block);
                Script.LootFilterBlocks.RemoveAt(blockPos);
                Script.LootFilterBlocks.Insert(blockPos - 1, block);
                LootFilterBlockViewModels.Move(currentIndex, currentIndex - 1);
                _isDirty = true;
                RaisePropertyChanged("LootFilterSectionViewModels");
            }
        }

        private void OnMoveBlockDownCommand()
        {
            MoveBlockDown(SelectedBlockViewModel);
        }

        public void MoveBlockDown(ILootFilterBlockViewModel blockViewModel)
        {
            var currentIndex = LootFilterBlockViewModels.IndexOf(blockViewModel);

            if (currentIndex < LootFilterBlockViewModels.Count - 1)
            {
                var block = blockViewModel.Block;
                var blockPos = Script.LootFilterBlocks.IndexOf(block);
                Script.LootFilterBlocks.RemoveAt(blockPos);
                Script.LootFilterBlocks.Insert(blockPos + 1, block);
                LootFilterBlockViewModels.Move(currentIndex, currentIndex + 1);
                _isDirty = true;
                RaisePropertyChanged("LootFilterSectionViewModels");
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
                RaisePropertyChanged("LootFilterSectionViewModels");
            }
        }

        private void OnAddBlockCommand()
        {
            AddBlock(SelectedBlockViewModel);
        }

        public void AddBlock(ILootFilterBlockViewModel blockViewModel)
        {
            var vm = _lootFilterBlockViewModelFactory.Create();
            var newBlock = new LootFilterBlock();
            vm.Initialise(newBlock, this);

            if (LootFilterBlockViewModels.Count > 0)
            {
                Script.LootFilterBlocks.Insert(Script.LootFilterBlocks.IndexOf(blockViewModel.Block) + 1, newBlock);
                LootFilterBlockViewModels.Insert(LootFilterBlockViewModels.IndexOf(blockViewModel) + 1, vm);
            }
            else
            {
                Script.LootFilterBlocks.Add(newBlock);
                LootFilterBlockViewModels.Add(vm);
            }

            SelectedBlockViewModel = vm;
            _isDirty = true;
        }

        private void OnAddSectionCommand()
        {
            AddSection(SelectedBlockViewModel);
        }

        public void AddSection(ILootFilterBlockViewModel blockViewModel)
        {
            var vm = _lootFilterBlockViewModelFactory.Create();
            var newSection = new LootFilterSection { Description = "New Section" };
            vm.Initialise(newSection, this);

            Script.LootFilterBlocks.Insert(Script.LootFilterBlocks.IndexOf(blockViewModel.Block) + 1, newSection);
            LootFilterBlockViewModels.Insert(LootFilterBlockViewModels.IndexOf(blockViewModel) + 1, vm);
            _isDirty = true;
            SelectedBlockViewModel = vm;
            RaisePropertyChanged("LootFilterSectionViewModels");
        }

        private void OnDeleteBlockCommand()
        {
            DeleteBlock(SelectedBlockViewModel);
        }

        public void DeleteBlock(ILootFilterBlockViewModel blockViewModel)
        {
            var result = MessageBox.Show("Are you sure you wish to delete this block?", "Delete Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Script.LootFilterBlocks.Remove(blockViewModel.Block);
                LootFilterBlockViewModels.Remove(blockViewModel);
                _isDirty = true;
            }
            SelectedBlockViewModel = null;
        }
    }
}
