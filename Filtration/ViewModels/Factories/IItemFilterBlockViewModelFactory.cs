namespace Filtration.ViewModels.Factories
{
    internal interface IItemFilterBlockViewModelFactory
    {
        IItemFilterBlockViewModel Create();
        void Release(IItemFilterBlockViewModel itemFilterBlockViewModel);
    }
}
