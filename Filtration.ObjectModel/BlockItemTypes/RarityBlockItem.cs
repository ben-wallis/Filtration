using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Extensions;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class RarityBlockItem : NumericFilterPredicateBlockItem
    {
        public RarityBlockItem()
        {
        }

        public RarityBlockItem(FilterPredicateOperator predicateOperator, int predicateOperand)
            : base(predicateOperator, predicateOperand)
        {
        }
        
        public override string PrefixText => "Rarity";
        public override string OutputText => PrefixText + " " + FilterPredicate.PredicateOperator
            .GetAttributeDescription() + " " + ((ItemRarity) FilterPredicate.PredicateOperand).GetAttributeDescription();
        public override int MaximumAllowed => 2;
        public override string DisplayHeading => "Item Rarity";
        public override string SummaryText => "Rarity " + FilterPredicate.PredicateOperator.GetAttributeDescription() + " " +
                                              ((ItemRarity) FilterPredicate.PredicateOperand).GetAttributeDescription();
        public override Color SummaryBackgroundColor => Colors.LightCoral;
        public override Color SummaryTextColor => Colors.White;
        public override int SortOrder => 4;
        public override int Minimum => 0;
        public override int Maximum => (int)ItemRarity.Unique;
    }
}
