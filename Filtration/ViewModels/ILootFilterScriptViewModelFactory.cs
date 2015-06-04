namespace Filtration.ViewModels
{
    internal interface ILootFilterScriptViewModelFactory
    {
        ILootFilterScriptViewModel Create();
        void Release(ILootFilterScriptViewModel lootFilterScriptViewModel);
    }
}
