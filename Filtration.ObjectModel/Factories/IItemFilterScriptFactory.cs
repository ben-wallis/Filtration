namespace Filtration.ObjectModel.Factories
{
    public interface IItemFilterScriptFactory
    {
        IItemFilterScript Create();

        void Release(IItemFilterScript itemFilterScript);
    }
}
