using System;
using Filtration.ObjectModel;

namespace Filtration.ViewModels.Factories
{
    internal interface IItemFilterBlockBaseViewModelFactory
    {
        IItemFilterBlockViewModelBase Create(IItemFilterBlockBase itemFilterBlockBase);
    }

    internal class ItemFilterBlockBaseViewModelFactory : IItemFilterBlockBaseViewModelFactory
    {
        private readonly IItemFilterBlockViewModelFactory _itemFilterBlockViewModelFactory;
        private readonly IItemFilterCommentBlockViewModelFactory _itemFilterCommentBlockViewModelFactory;

        public ItemFilterBlockBaseViewModelFactory(IItemFilterBlockViewModelFactory itemFilterBlockViewModelFactory,
                                                   IItemFilterCommentBlockViewModelFactory itemFilterCommentBlockViewModelFactory)
        {
            _itemFilterBlockViewModelFactory = itemFilterBlockViewModelFactory;
            _itemFilterCommentBlockViewModelFactory = itemFilterCommentBlockViewModelFactory;
        }

        public IItemFilterBlockViewModelBase Create(IItemFilterBlockBase itemFilterBlockBase)
        {
            if (itemFilterBlockBase is IItemFilterBlock)
            {
                return _itemFilterBlockViewModelFactory.Create();
            }
            if (itemFilterBlockBase is IItemFilterCommentBlock)
            {
                return _itemFilterCommentBlockViewModelFactory.Create();
            }

            throw new InvalidOperationException("Unknown IItemFilterBlockBase type");
        }
    }
}
