using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class ItemLevelBlockItem : NumericFilterPredicateBlockItem
    {
        public ItemLevelBlockItem()
        {
        }

        public ItemLevelBlockItem(FilterPredicateOperator predicateOperator, int predicateOperand) : base (predicateOperator, predicateOperand)
        {
        }

        public override string PrefixText => "ItemLevel";
        public override int MaximumAllowed => 2;
        public override string DisplayHeading => "Item Level";
        public override string SummaryText => "Item Level " + FilterPredicate;
        public override Color SummaryBackgroundColor => Colors.DarkSlateGray;
        public override Color SummaryTextColor => Colors.White;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.ItemLevel;
        public override int Minimum => 0;
        public override int Maximum => 100;
    }
}
