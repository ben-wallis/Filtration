using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class BorderColorBlockItem : ColorBlockItem
    {
        public BorderColorBlockItem()
        {
            Color = new Color {A = 240, R = 0, G = 0, B = 0};
        }

        public BorderColorBlockItem(Color color) : base(color)
        {
        }

        public override string PrefixText => "SetBorderColor";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Border Color";
        public override BlockItemOrdering SortOrder => BlockItemOrdering.SetBorderColor;
    }
}
