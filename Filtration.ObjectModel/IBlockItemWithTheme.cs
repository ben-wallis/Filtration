using Filtration.ObjectModel.ThemeEditor;

namespace Filtration.ObjectModel
{
    public interface IBlockItemWithTheme : IItemFilterBlockItem
    {
        ThemeComponent ThemeComponent { get; set; }
    }
}
