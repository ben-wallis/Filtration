using System.Windows.Media;
using Filtration.Enums;
using Filtration.Models.BlockItemBaseTypes;

namespace Filtration.Models.BlockItemTypes
{
    internal class HeightBlockItem : NumericFilterPredicateBlockItem
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
