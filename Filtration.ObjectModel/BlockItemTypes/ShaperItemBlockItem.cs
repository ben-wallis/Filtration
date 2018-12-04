using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public sealed class ShaperItemBlockItem : BooleanBlockItem
    {
        public ShaperItemBlockItem()
        {
        }

        public ShaperItemBlockItem(bool booleanValue) : base(booleanValue)
        {
        }

        public override string PrefixText => "ShaperItem";
        public override string DisplayHeading => "Shaper Item";
        public override Color SummaryBackgroundColor => Colors.DimGray;
        public override Color SummaryTextColor => Colors.White;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.ShaperItem;
    }
}
