using System;
using Filtration.ObjectModel;
using GalaSoft.MvvmLight;

namespace Filtration.ViewModels
{
    internal interface IItemFilterBlockViewModelBase
    {
        void Initialise(IItemFilterBlockBase itemfilterBlock, IItemFilterScriptViewModel itemFilterScriptViewModel);
        IItemFilterBlockBase BaseBlock { get; }
        bool IsDirty { get; set; }
        event EventHandler BlockBecameDirty;
    }

    internal abstract class ItemFilterBlockViewModelBase : ViewModelBase, IItemFilterBlockViewModelBase
    {
        private bool _isDirty;

        public virtual void Initialise(IItemFilterBlockBase itemfilterBlock, IItemFilterScriptViewModel itemFilterScriptViewModel)
        {
            BaseBlock = itemfilterBlock;
        }

        public event EventHandler BlockBecameDirty;

        public IItemFilterBlockBase BaseBlock { get; protected set; }
        
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
    }
}