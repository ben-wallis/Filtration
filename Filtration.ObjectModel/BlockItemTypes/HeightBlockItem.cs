using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class HeightBlockItem : NumericFilterPredicateBlockItem
    {
        public HeightBlockItem()
        {
        }

        public HeightBlockItem(FilterPredicateOperator predicateOperator, int predicateOperand)
            : base(predicateOperator, predicateOperand)
        {
        }

        public override string PrefixText => "Height";
        public override int MaximumAllowed => 2;
        public override string DisplayHeading => "Height";
        public override string SummaryText => "Height " + FilterPredicate;
        public override Color SummaryBackgroundColor => Colors.LightBlue;
        public override Color SummaryTextColor => Colors.Black;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.Height;
        public override int Minimum => 0;
        public override int Maximum => 6;
    }
}
