using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

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

        public override string PrefixText => "Quality";
        public override int MaximumAllowed => 2;
        public override string DisplayHeading => "Quality";
        public override string SummaryText => "Quality " + FilterPredicate;
        public override Color SummaryBackgroundColor => Colors.DarkOrange;
        public override Color SummaryTextColor => Colors.White;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.Quality;
        public override int Minimum => 0;
        public override int Maximum => 99;
    }
}
