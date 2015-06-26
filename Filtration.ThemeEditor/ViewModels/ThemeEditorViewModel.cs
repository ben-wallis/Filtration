using Filtration.Interface;

namespace Filtration.ThemeEditor.ViewModels
{
    public interface IThemeEditorViewModel : IDocument
    {
    }

    public class ThemeEditorViewModel : IThemeEditorViewModel
    {
        public bool IsScript { get { return false; }}
        public string Title { get { return "Theme Editor"; } }
    }
}
