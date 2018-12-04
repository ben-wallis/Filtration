using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class WidthBlockItem : NumericFilterPredicateBlockItem
    {
        public WidthBlockItem()
        {
        }

        public WidthBlockItem(FilterPredicateOperator predicateOperator, int predicateOperand)
            : base(predicateOperator, predicateOperand)
        {
        }

        public override string PrefixText => "Width";
        public override int MaximumAllowed => 2;
        public override string DisplayHeading => "Width";
        public override string SummaryText => "Width " + FilterPredicate;
        public override Color SummaryBackgroundColor => Colors.MediumPurple;
        public override Color SummaryTextColor => Colors.White;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.Width;
        public override int Minimum => 0;
        public override int Maximum => 2;
    }
}
