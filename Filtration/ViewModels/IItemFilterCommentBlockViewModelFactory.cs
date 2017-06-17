namespace Filtration.ViewModels
{
    internal interface IItemFilterCommentBlockViewModelFactory
    {
        IItemFilterCommentBlockViewModel Create();
        void Release(IItemFilterCommentBlockViewModel itemFilterCommentBlockViewModel);
    }
}