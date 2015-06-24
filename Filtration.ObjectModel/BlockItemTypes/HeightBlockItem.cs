using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class HeightBlockItem : NumericFilterPredicateBlockItem
    {
        public HeightBlockItem()
        {
        }

        public HeightBlockItem(FilterPredicateOperator predicateOperator, int predicateOperand)
            : base(predicateOperator, predicateOperand)
        {
        }

        public override string PrefixText
        {
            get { return "Height"; }
        }

        public override int MaximumAllowed
        {
            get { return 2; }
        }

        public override string DisplayHeading
        {
            get { return "Height"; }
        }

        public override string SummaryText
        {
            get { return "Height " + FilterPredicate; }
        }

        public override Color SummaryBackgroundColor
        {
            get { return Colors.LightBlue; }
        }

        public override Color SummaryTextColor
        {
            get { return Colors.Black; }
        }

        public override int SortOrder
        {
            get { return 8; }
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
                return 6;
            }
        }
    }
}
