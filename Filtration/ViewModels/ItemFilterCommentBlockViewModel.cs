using Filtration.ObjectModel;

namespace Filtration.ViewModels
{
    internal interface IItemFilterCommentBlockViewModel : IItemFilterBlockViewModelBase
    {
        IItemFilterCommentBlock ItemFilterCommentBlock { get; }
        string Comment { get; }
    }

    internal class ItemFilterCommentBlockViewModel : ItemFilterBlockViewModelBase, IItemFilterCommentBlockViewModel
    {
        public ItemFilterCommentBlockViewModel()
        {
        }

        public override void Initialise(IItemFilterBlockBase itemfilterBlock, IItemFilterScriptViewModel itemFilterScriptViewModel)
        {
            ItemFilterCommentBlock = itemfilterBlock as IItemFilterCommentBlock;
            BaseBlock = ItemFilterCommentBlock;

            base.Initialise(itemfilterBlock, itemFilterScriptViewModel);
        }

        public IItemFilterCommentBlock ItemFilterCommentBlock { get; private set; }

        public string Comment => ItemFilterCommentBlock.Comment;
    }
}