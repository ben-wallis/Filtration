using System.Windows.Media;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ItemFilterPreview.Model
{
    public interface IFilteredItem
    {
        IItem Item { get; }
        IItemFilterBlock ItemFilterBlock { get; }
        BlockAction BlockAction { get; }
        Color BackgroundColor { get; }
        Color BorderColor { get; }
        Color TextColor { get; }
    }

    public class FilteredItem : IFilteredItem
    {
        public FilteredItem(IItem item, IItemFilterBlock itemFilterBlock)
        {
            Item = item;
            ItemFilterBlock = itemFilterBlock;
        }

        public IItem Item { get; private set; }
        public IItemFilterBlock ItemFilterBlock { get; private set; }

        public BlockAction BlockAction => ItemFilterBlock?.Action ?? BlockAction.Show;
        public Color BackgroundColor => ItemFilterBlock.HasBlockItemOfType<BackgroundColorBlockItem>() ? ItemFilterBlock.DisplayBackgroundColor : new Color { A = 255, R = 0, G = 0, B = 0 };
        public Color BorderColor => ItemFilterBlock.HasBlockItemOfType<BorderColorBlockItem>() ? ItemFilterBlock.DisplayBorderColor : new Color { A = 255, R = 0, G = 0, B = 0 };
        public Color TextColor
        {
            get
            {
                if (ItemFilterBlock.HasBlockItemOfType<TextColorBlockItem>())
                {
                    return ItemFilterBlock.DisplayTextColor;
                }

                return Item.DefaultTextColor;
            }
        }
    }
}
