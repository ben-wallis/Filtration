using System;
using Filtration.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    internal interface IItemFilterBlockViewModelBase
    {
        void Initialise(IItemFilterBlockBase itemfilterBlock, IItemFilterScriptViewModel itemFilterScriptViewModel);
        IItemFilterBlockBase BaseBlock { get; }
        bool IsDirty { get; set; }
        bool IsVisible { get; set; }
        event EventHandler BlockBecameDirty;
    }

    internal abstract class ItemFilterBlockViewModelBase : ViewModelBase, IItemFilterBlockViewModelBase
    {
        private bool _isDirty;
        private bool _isVisible;

        public ItemFilterBlockViewModelBase()
        {
            CopyBlockCommand = new RelayCommand(OnCopyBlockCommand);
            PasteBlockCommand = new RelayCommand(OnPasteBlockCommand);
            AddBlockCommand = new RelayCommand(OnAddBlockCommand);
            AddSectionCommand = new RelayCommand(OnAddSectionCommand);
            DeleteBlockCommand = new RelayCommand(OnDeleteBlockCommand);
            MoveBlockUpCommand = new RelayCommand(OnMoveBlockUpCommand);
            MoveBlockDownCommand = new RelayCommand(OnMoveBlockDownCommand);
            MoveBlockToTopCommand = new RelayCommand(OnMoveBlockToTopCommand);
            MoveBlockToBottomCommand = new RelayCommand(OnMoveBlockToBottomCommand);

            _isVisible = true;
        }


        public virtual void Initialise(IItemFilterBlockBase itemfilterBlock, IItemFilterScriptViewModel itemFilterScriptViewModel)
        {
            BaseBlock = itemfilterBlock;
            _parentScriptViewModel = itemFilterScriptViewModel;
        }

        public event EventHandler BlockBecameDirty;

        public IItemFilterBlockBase BaseBlock { get; protected set; }
        public IItemFilterScriptViewModel _parentScriptViewModel;

        public RelayCommand CopyBlockCommand { get; }
        public RelayCommand PasteBlockCommand { get; }
        public RelayCommand AddBlockCommand { get; }
        public RelayCommand AddSectionCommand { get; }
        public RelayCommand DeleteBlockCommand { get; }
        public RelayCommand MoveBlockUpCommand { get; }
        public RelayCommand MoveBlockDownCommand { get; }
        public RelayCommand MoveBlockToTopCommand { get; }
        public RelayCommand MoveBlockToBottomCommand { get; }
        
        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (value != _isDirty)
                {
                    _isDirty = value;
                    RaisePropertyChanged();
                    BlockBecameDirty?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                RaisePropertyChanged();
            }
        }

        private void OnCopyBlockCommand()
        {
            _parentScriptViewModel.CopyBlock(this);
        }

        private void OnPasteBlockCommand()
        {
            _parentScriptViewModel.PasteBlock(this);
        }

        private void OnAddBlockCommand()
        {
            _parentScriptViewModel.AddBlock(this);
        }

        private void OnAddSectionCommand()
        {
            _parentScriptViewModel.AddCommentBlock(this);
        }

        private void OnDeleteBlockCommand()
        {
            _parentScriptViewModel.DeleteBlock(this);
        }

        private void OnMoveBlockUpCommand()
        {
            _parentScriptViewModel.MoveBlockUp(this);
        }

        private void OnMoveBlockDownCommand()
        {
            _parentScriptViewModel.MoveBlockDown(this);
        }

        private void OnMoveBlockToTopCommand()
        {
            _parentScriptViewModel.MoveBlockToTop(this);
        }

        private void OnMoveBlockToBottomCommand()
        {
            _parentScriptViewModel.MoveBlockToBottom(this);
        }
    }
}