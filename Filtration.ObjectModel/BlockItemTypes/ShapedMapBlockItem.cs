using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public sealed class ShapedMapBlockItem : BooleanBlockItem
    {
        public ShapedMapBlockItem()
        {
        }

        public ShapedMapBlockItem(bool booleanValue) : base(booleanValue)
        {
        }

        public override string PrefixText => "ShapedMap";
        public override string DisplayHeading => "Shaped Map";
        public override Color SummaryBackgroundColor => Colors.DarkGoldenrod;
        public override Color SummaryTextColor => Colors.White;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.ShapedMap;
    }
}
