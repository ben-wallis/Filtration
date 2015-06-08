namespace Filtration.ViewModels
{
    internal interface IItemFilterBlockViewModelFactory
    {
        IItemFilterBlockViewModel Create();
        void Release(IItemFilterBlockViewModel itemFilterBlockViewModel);
    }
}
