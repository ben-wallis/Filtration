using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.LootExplosionStudio;

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

        public override string PrefixText
        {
            get { return "Width"; }
        }

        public override int MaximumAllowed
        {
            get { return 2; }
        }

        public override string DisplayHeading
        {
            get
            {
                return "Width";
            }
        }

        public override string SummaryText
        {
            get { return "Width " + FilterPredicate; }
        }

        public override Color SummaryBackgroundColor
        {
            get { return Colors.MediumPurple; }
        }

        public override Color SummaryTextColor
        {
            get { return Colors.White; }
        }

        public override int SortOrder
        {
            get { return 7; }
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
                return 2;
            }
        }

        public override int GetLootItemProperty(LootItem lootItem)
        {
            return lootItem.Width;
        }
    }
}
