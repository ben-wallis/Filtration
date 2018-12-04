using System.Windows.Media;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class BackgroundColorBlockItem : ColorBlockItem
    {
        public BackgroundColorBlockItem()
        {
            Color = new Color { A = 240, R = 0, G = 0, B = 0 };
        }

        public BackgroundColorBlockItem(Color color) : base(color)
        {
        }

        public override string PrefixText => "SetBackgroundColor";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Background Color";
        public override BlockItemOrdering SortOrder => BlockItemOrdering.SetBackgroundColor;
    }
}
