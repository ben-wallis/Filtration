using System.Windows.Media;
using Filtration.Enums;
using Filtration.Models.BlockItemBaseTypes;

namespace Filtration.Models.BlockItemTypes
{
    internal class ItemLevelBlockItem : NumericFilterPredicateBlockItem
    {
        public ItemLevelBlockItem()
        {
        }

        public ItemLevelBlockItem(FilterPredicateOperator predicateOperator, int predicateOperand) : base (predicateOperator, predicateOperand)
        {
        }
        
        public override string PrefixText
        {
            get { return "ItemLevel"; }
        }

        public override int MaximumAllowed
        {
            get { return 2; }
        }

        public override string DisplayHeading
        {
            get
            {
                return "Item Level";
            }
        }

        public override string SummaryText
        {
            get { return "Item Level " + FilterPredicate; }
        }

        public override Color SummaryBackgroundColor
        {
            get { return Colors.DarkSlateGray; }
        }

        public override Color SummaryTextColor
        {
            get { return Colors.White; }
        }

        public override int SortOrder
        {
            get { return 1; }
        }

        public override int Minimum
        {
            get
            {
                return 0;
            }
        }

        public override int Maximum
        {
            get
            {
                return 100;
            }
        }
    }
}
