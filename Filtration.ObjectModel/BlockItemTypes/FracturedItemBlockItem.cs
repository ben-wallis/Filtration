using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public sealed class FracturedItemBlockItem : BooleanBlockItem
    {
        public FracturedItemBlockItem()
        {
        }

        public FracturedItemBlockItem(bool booleanValue) : base(booleanValue)
        {
        }

        public override string PrefixText => "FracturedItem";
        public override string DisplayHeading => "Fractured Item";
        public override Color SummaryBackgroundColor => Colors.Salmon;
        public override Color SummaryTextColor => Colors.Black;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.FracturedItem;
    }
}
