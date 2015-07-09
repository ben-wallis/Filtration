using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.LootExplosionStudio;

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

        public override int GetLootItemProperty(LootItem lootItem)
        {
            return lootItem.ItemLevel;
        }
    }
}
