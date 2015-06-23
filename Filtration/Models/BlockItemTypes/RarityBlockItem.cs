using System.Windows.Media;
using Filtration.Enums;
using Filtration.Extensions;
using Filtration.Models.BlockItemBaseTypes;

namespace Filtration.Models.BlockItemTypes
{
    internal class RarityBlockItem : NumericFilterPredicateBlockItem
    {
        public RarityBlockItem()
        {
        }

        public RarityBlockItem(FilterPredicateOperator predicateOperator, int predicateOperand)
            : base(predicateOperator, predicateOperand)
        {
        }
        
        public override string PrefixText
        {
            get { return "Rarity"; }
        }

        public override string OutputText
        {
            get
            {
                return PrefixText + " " + FilterPredicate.PredicateOperator
                    .GetAttributeDescription() +
                       " " +
                       ((ItemRarity) FilterPredicate.PredicateOperand)
                           .GetAttributeDescription();
            }
        }

        public override int MaximumAllowed
        {
            get { return 2; }
        }

        public override string DisplayHeading
        {
            get
            {
                return "Item Rarity";
            }
        }

        public override string SummaryText
        {
            get
            {
                return "Rarity " + FilterPredicate.PredicateOperator.GetAttributeDescription() + " " +
                       ((ItemRarity) FilterPredicate.PredicateOperand).GetAttributeDescription();
            }
        }

        public override Color SummaryBackgroundColor
        {
            get { return Colors.LightCoral; }
        }

        public override Color SummaryTextColor
        {
            get { return Colors.White; }
        }

        public override int SortOrder
        {
            get { return 4; }
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
                return (int)ItemRarity.Unique;
            }
        }
    }
}
