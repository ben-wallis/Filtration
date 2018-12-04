using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class GemLevelBlockItem : NumericFilterPredicateBlockItem
    {
        public GemLevelBlockItem()
        {
        }

        public GemLevelBlockItem(FilterPredicateOperator predicateOperator, int predicateOperand)
            : base(predicateOperator, predicateOperand)
        {
        }

        public override string PrefixText => "GemLevel";
        public override int MaximumAllowed => 2;
        public override string DisplayHeading => "Gem Level";
        public override string SummaryText => "Gem Level " + FilterPredicate;
        public override Color SummaryBackgroundColor => Colors.DarkSlateGray;
        public override Color SummaryTextColor => Colors.White;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.GemLevel;
        public override int Minimum => 0;
        public override int Maximum => 21;
    }
}
