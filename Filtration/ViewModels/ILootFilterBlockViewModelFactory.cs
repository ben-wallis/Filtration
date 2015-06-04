namespace Filtration.ViewModels
{
    internal interface ILootFilterBlockViewModelFactory
    {
        ILootFilterBlockViewModel Create();
        void Release(ILootFilterBlockViewModel lootFilterBlockViewModel);
    }
}
