namespace Filtration.ViewModels.Factories
{
    internal interface IItemFilterCommentBlockViewModelFactory
    {
        IItemFilterCommentBlockViewModel Create();
        void Release(IItemFilterCommentBlockViewModel itemFilterCommentBlockViewModel);
    }
}