using System;
using Filtration.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    internal interface IItemFilterBlockViewModelBase
    {
        void Initialise(IItemFilterBlockBase itemFilterBlock, IItemFilterScriptViewModel itemFilterScriptViewModel);
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
            _isVisible = true;
        }


        public virtual void Initialise(IItemFilterBlockBase itemFilterBlock, IItemFilterScriptViewModel itemFilterScriptViewModel)
        {
            BaseBlock = itemFilterBlock;
            _parentScriptViewModel = itemFilterScriptViewModel;

            CopyBlockCommand = new RelayCommand(OnCopyBlockCommand);
            PasteBlockCommand = new RelayCommand(OnPasteBlockCommand);
            AddBlockCommand = new RelayCommand(OnAddBlockCommand);
            AddSectionCommand = new RelayCommand(OnAddSectionCommand);
            DeleteBlockCommand = new RelayCommand(OnDeleteBlockCommand, () => _parentScriptViewModel.CanModifyBlock(this));
            MoveBlockUpCommand = new RelayCommand(OnMoveBlockUpCommand);
            MoveBlockDownCommand = new RelayCommand(OnMoveBlockDownCommand);
            MoveBlockToTopCommand = new RelayCommand(OnMoveBlockToTopCommand);
            MoveBlockToBottomCommand = new RelayCommand(OnMoveBlockToBottomCommand);
        }

        public event EventHandler BlockBecameDirty;

        public IItemFilterBlockBase BaseBlock { get; protected set; }
        public IItemFilterScriptViewModel _parentScriptViewModel;

        public RelayCommand CopyBlockCommand { get; private set; }
        public RelayCommand PasteBlockCommand { get; private set; }
        public RelayCommand AddBlockCommand { get; private set; }
        public RelayCommand AddSectionCommand { get; private set; }
        public RelayCommand DeleteBlockCommand { get; private set; }
        public RelayCommand MoveBlockUpCommand { get; private set; }
        public RelayCommand MoveBlockDownCommand { get; private set; }
        public RelayCommand MoveBlockToTopCommand { get; private set; }
        public RelayCommand MoveBlockToBottomCommand { get; private set; }
        
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