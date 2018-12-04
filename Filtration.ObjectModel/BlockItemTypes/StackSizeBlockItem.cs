using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class StackSizeBlockItem : NumericFilterPredicateBlockItem
    {
        public StackSizeBlockItem()
        {
        }

        public StackSizeBlockItem(FilterPredicateOperator predicateOperator, int predicateOperand)
            : base(predicateOperator, predicateOperand)
        {
        }

        public override string PrefixText => "StackSize";
        public override int MaximumAllowed => 2;
        public override string DisplayHeading => "Stack Size";
        public override string SummaryText => "Stack Size " + FilterPredicate;
        public override Color SummaryBackgroundColor => Colors.DarkSlateGray;
        public override Color SummaryTextColor => Colors.White;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.StackSize;
        public override int Minimum => 0;
        public override int Maximum => 1000;
    }
}
