namespace Filtration.ViewModels
{
    internal interface IItemFilterScriptViewModelFactory
    {
        IItemFilterScriptViewModel Create();
        void Release(IItemFilterScriptViewModel itemFilterScriptViewModel);
    }
}
