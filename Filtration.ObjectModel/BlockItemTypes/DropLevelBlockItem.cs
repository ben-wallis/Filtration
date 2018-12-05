using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class DropLevelBlockItem : NumericFilterPredicateBlockItem
    {
        public DropLevelBlockItem()
        {
        }

        public DropLevelBlockItem(FilterPredicateOperator predicateOperator, int predicateOperand)
            : base(predicateOperator, predicateOperand)
        {
        }

        public override string PrefixText => "DropLevel";
        public override int MaximumAllowed => 2;
        public override string DisplayHeading => "Drop Level";
        public override string SummaryText => "Drop Level " + FilterPredicate;
        public override Color SummaryBackgroundColor => Colors.DodgerBlue;
        public override Color SummaryTextColor => Colors.White;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.DropLevel;
        public override int Minimum => 0;
        public override int Maximum => 100;
    }
}
