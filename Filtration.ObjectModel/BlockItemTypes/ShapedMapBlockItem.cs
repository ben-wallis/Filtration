using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;

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
        public override int SortOrder => 9;

    }
}
