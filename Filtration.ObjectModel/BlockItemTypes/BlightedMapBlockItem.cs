using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public sealed class BlightedMapBlockItem : BooleanBlockItem
    {
        public BlightedMapBlockItem()
        {
        }

        public BlightedMapBlockItem(bool booleanValue) : base(booleanValue)
        {
        }

        public override string PrefixText => "BlightedMap";
        public override string DisplayHeading => "Blighted Map";
        public override Color SummaryBackgroundColor => Colors.Khaki;
        public override Color SummaryTextColor => Colors.Black;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.BligtedMap;
    }
}
