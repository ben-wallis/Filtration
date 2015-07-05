namespace Filtration.ThemeEditor.ViewModels
{
    public interface IThemeViewModelFactory
    {
        IThemeEditorViewModel Create();
        void Release(IThemeEditorViewModel themeEditorViewModel);
    }
}
