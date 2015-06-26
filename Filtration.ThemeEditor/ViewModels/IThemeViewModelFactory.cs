namespace Filtration.ThemeEditor.ViewModels
{
    public interface IThemeViewModelFactory
    {
        IThemeViewModel Create();
        void Release(IThemeViewModel themeViewModel);
    }
}
