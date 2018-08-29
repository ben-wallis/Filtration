using System.Windows.Media;
using Filtration.ObjectModel.Enums;

namespace Filtration.ThemeEditor.ViewModels
{
    public interface IThemeComponentViewModel
    {
        string ComponentName { get; set; }
        ThemeComponentType ComponentType { get; set; }
    }

    public class ThemeComponentViewModel : IThemeComponentViewModel
    {
        public string ComponentName { get; set; }
        public ThemeComponentType ComponentType { get; set; }
    }
}
