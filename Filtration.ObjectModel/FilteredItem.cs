using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel
{
    public interface IFilteredItem
    {
        IItem Item { get; }
        IItemFilterBlock ItemFilterBlock { get; }
        BlockAction BlockAction { get; }
        Color BackgroundColor { get; }
        Color BorderColor { get; }
        Color TextColor { get; }
        double FontSize { get; }
    }

    public class FilteredItem : IFilteredItem
    {
        public FilteredItem(IItem item, IItemFilterBlock itemFilterBlock)
        {
            Item = item;
            ItemFilterBlock = itemFilterBlock;

            BlockAction = itemFilterBlock?.Action ?? BlockAction.Show;
            BackgroundColor = itemFilterBlock?.DisplayBackgroundColor ?? new Color { A = 240, R = 0, G = 0, B = 0 };
            BorderColor = itemFilterBlock?.DisplayBorderColor ?? new Color {A = 240, R = 0, G = 0, B = 0};
            FontSize = (itemFilterBlock?.DisplayFontSize ?? 34) / 1.8;

            SetTextColor();
        }

        private void SetTextColor()
        {
            if (ItemFilterBlock == null)
            {
                TextColor = Item.DefaultTextColor;
                return;
            }

            var textColorBlockItem = ItemFilterBlock.BlockItems?.OfType<TextColorBlockItem>().FirstOrDefault();
            TextColor = textColorBlockItem?.Color ?? Item.DefaultTextColor;
        }


        public IItem Item { get; private set; }
        public IItemFilterBlock ItemFilterBlock { get; private set; }

        public BlockAction BlockAction { get; private set; }
        public Color BackgroundColor { get; private set; }
        public Color TextColor { get; private set; }
        public Color BorderColor { get; private set; }
        public double FontSize { get; private set; }
    }
}
