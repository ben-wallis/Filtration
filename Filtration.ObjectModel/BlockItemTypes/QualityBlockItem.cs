using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.LootExplosionStudio;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class QualityBlockItem : NumericFilterPredicateBlockItem
    {
        public QualityBlockItem()
        {
        }
        
        public QualityBlockItem(FilterPredicateOperator predicateOperator, int predicateOperand)
            : base(predicateOperator, predicateOperand)
        {
        }

        public override string PrefixText
        {
            get { return "Quality"; }
        }

        public override int MaximumAllowed
        {
            get { return 2; }
        }

        public override string DisplayHeading
        {
            get
            {
                return "Quality";
            }
        }

        public override string SummaryText
        {
            get { return "Quality " + FilterPredicate; }
        }

        public override Color SummaryBackgroundColor
        {
            get { return Colors.DarkOrange; }
        }

        public override Color SummaryTextColor
        {
            get { return Colors.White; }
        }

        public override int SortOrder
        {
            get { return 3; }
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
                return 20;
            }
        }

        public override int GetLootItemProperty(LootItem lootItem)
        {
            return lootItem.Quality;
        }
    }
}
