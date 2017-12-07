using Filtration.ObjectModel.ThemeEditor;

namespace Filtration.ObjectModel
{
    public interface IItemFilterScriptSettings
    {
        bool BlockGroupsEnabled { get; set; }
        ThemeComponentCollection ThemeComponentCollection { get; }
    }

    public class ItemFilterScriptSettings : IItemFilterScriptSettings
    {
        public ItemFilterScriptSettings(ThemeComponentCollection themeComponentCollection)
        {
            ThemeComponentCollection = themeComponentCollection;
        }

        public bool BlockGroupsEnabled { get; set; }
        public ThemeComponentCollection ThemeComponentCollection { get; }
    }
}