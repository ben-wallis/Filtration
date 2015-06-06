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
    internal interface ILootFilterScriptViewModel
    {
        LootFilterScript Script { get; }
        bool IsDirty { get; }
        string Description { get; set; }
        string DisplayName { get; }
        void Initialise(LootFilterScript lootFilterScript);
        ILootFilterBlockViewModel SelectedBlockViewModel { get; set; }
        void RemoveDirtyFlag();
        void AddSection(ILootFilterBlockViewModel targetBlockViewModel);
        void AddBlock(ILootFilterBlockViewModel targetBlockViewModel);
        void CopyBlock(ILootFilterBlockViewModel targetBlockViewModel);
        void PasteBlock(ILootFilterBlockViewModel targetBlockViewModel);
    }

    internal class LootFilterScriptViewModel : FiltrationViewModelBase, ILootFilterScriptViewModel
    {
        private readonly ILootFilterBlockViewModelFactory _lootFilterBlockViewModelFactory;
        private readonly ILootFilterBlockTranslator _blockTranslator;
        private bool _isDirty;
        private ILootFilterBlockViewModel _selectedBlockViewModel;

        public LootFilterScriptViewModel(ILootFilterBlockViewModelFactory lootFilterBlockViewModelFactory, ILootFilterBlockTranslator blockTranslator)
        {
            DeleteBlockCommand = new RelayCommand(OnDeleteBlockCommand, () => SelectedBlockViewModel != null);
            MoveBlockToTopCommand = new RelayCommand(OnMoveBlockToTopCommand, () => SelectedBlockViewModel != null);
            MoveBlockUpCommand = new RelayCommand(OnMoveBlockUpCommand, () => SelectedBlockViewModel != null);
            MoveBlockDownCommand = new RelayCommand(OnMoveBlockDownCommand, () => SelectedBlockViewModel != null);
            MoveBlockToBottomCommand = new RelayCommand(OnMoveBlockToBottomCommand, () => SelectedBlockViewModel != null);
            AddBlockCommand = new RelayCommand(OnAddBlockCommand);
            AddSectionCommand = new RelayCommand(OnAddSectionCommand, () => SelectedBlockViewModel != null);
            CopyBlockCommand = new RelayCommand(OnCopyBlockCommand, () => SelectedBlockViewModel != null);
            PasteBlockCommand = new RelayCommand(OnPasteBlockCommand);
            _lootFilterBlockViewModelFactory = lootFilterBlockViewModelFactory;
            _blockTranslator = blockTranslator;
            LootFilterBlockViewModels = new ObservableCollection<ILootFilterBlockViewModel>();

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
            set
            {
                _isDirty = value;
            }
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

        private void OnCopyBlockCommand()
        {
            CopyBlock(SelectedBlockViewModel);
        }

        public void CopyBlock(ILootFilterBlockViewModel targetBlockViewModel)
        {
            Clipboard.SetText(_blockTranslator.TranslateLootFilterBlockToString(SelectedBlockViewModel.Block));
        }

        private void OnPasteBlockCommand()
        {
            PasteBlock(SelectedBlockViewModel);
        }

        public void PasteBlock(ILootFilterBlockViewModel targetBlockViewModel)
        {
            var clipboardText = Clipboard.GetText();
            if (clipboardText.IsNullOrEmpty()) return;
            
            var translatedBlock = _blockTranslator.TranslateStringToLootFilterBlock(clipboardText);
            if (translatedBlock == null) return;

            var vm = _lootFilterBlockViewModelFactory.Create();
            vm.Initialise(translatedBlock, this);

            if (LootFilterBlockViewModels.Count > 0)
            {
                Script.LootFilterBlocks.Insert(Script.LootFilterBlocks.IndexOf(targetBlockViewModel.Block) + 1, translatedBlock);
                LootFilterBlockViewModels.Insert(LootFilterBlockViewModels.IndexOf(targetBlockViewModel) + 1, vm);
            }
            else
            {
                Script.LootFilterBlocks.Add(translatedBlock);
                LootFilterBlockViewModels.Add(vm);
            }

            SelectedBlockViewModel = vm;
            _isDirty = true;

        }

        private void OnMoveBlockToTopCommand()
        {
            MoveBlockToTop(SelectedBlockViewModel);
           
        }

        public void MoveBlockToTop(ILootFilterBlockViewModel targetBlockViewModel)
        {
            var currentIndex = LootFilterBlockViewModels.IndexOf(targetBlockViewModel);

            if (currentIndex > 0)
            {
                var block = targetBlockViewModel.Block;
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

        public void MoveBlockUp(ILootFilterBlockViewModel targetBlockViewModel)
        {
            var currentIndex = LootFilterBlockViewModels.IndexOf(targetBlockViewModel);

            if (currentIndex > 0)
            {
                var block = targetBlockViewModel.Block;
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

        public void MoveBlockDown(ILootFilterBlockViewModel targetBlockViewModel)
        {
            var currentIndex = LootFilterBlockViewModels.IndexOf(targetBlockViewModel);

            if (currentIndex < LootFilterBlockViewModels.Count - 1)
            {
                var block = targetBlockViewModel.Block;
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
            MoveBlockToBottom(SelectedBlockViewModel);
        }

        public void MoveBlockToBottom(ILootFilterBlockViewModel targetBlockViewModel)
        {
            var currentIndex = LootFilterBlockViewModels.IndexOf(targetBlockViewModel);

            if (currentIndex < LootFilterBlockViewModels.Count - 1)
            {
                var block = targetBlockViewModel.Block;
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

        public void AddBlock(ILootFilterBlockViewModel targetBlockViewModel)
        {
            var vm = _lootFilterBlockViewModelFactory.Create();
            var newBlock = new LootFilterBlock();
            vm.Initialise(newBlock, this);

            if (targetBlockViewModel != null)
            {
                Script.LootFilterBlocks.Insert(Script.LootFilterBlocks.IndexOf(targetBlockViewModel.Block) + 1, newBlock);
                LootFilterBlockViewModels.Insert(LootFilterBlockViewModels.IndexOf(targetBlockViewModel) + 1, vm);
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

        public void AddSection(ILootFilterBlockViewModel targetBlockViewModel)
        {
            var vm = _lootFilterBlockViewModelFactory.Create();
            var newSection = new LootFilterSection { Description = "New Section" };
            vm.Initialise(newSection, this);

            Script.LootFilterBlocks.Insert(Script.LootFilterBlocks.IndexOf(targetBlockViewModel.Block) + 1, newSection);
            LootFilterBlockViewModels.Insert(LootFilterBlockViewModels.IndexOf(targetBlockViewModel) + 1, vm);
            _isDirty = true;
            SelectedBlockViewModel = vm;
            RaisePropertyChanged("LootFilterSectionViewModels");
        }

        private void OnDeleteBlockCommand()
        {
            DeleteBlock(SelectedBlockViewModel);
        }

        public void DeleteBlock(ILootFilterBlockViewModel targetBlockViewModel)
        {
            var result = MessageBox.Show("Are you sure you wish to delete this block?", "Delete Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Script.LootFilterBlocks.Remove(targetBlockViewModel.Block);
                LootFilterBlockViewModels.Remove(targetBlockViewModel);
                _isDirty = true;
            }
            SelectedBlockViewModel = null;
        }
    }
}
