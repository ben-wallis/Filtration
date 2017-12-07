namespace Filtration.ViewModels.Factories
{
    internal interface IItemFilterScriptViewModelFactory
    {
        IItemFilterScriptViewModel Create();
        void Release(IItemFilterScriptViewModel itemFilterScriptViewModel);
    }
}
