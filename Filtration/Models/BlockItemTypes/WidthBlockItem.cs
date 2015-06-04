using System.Windows.Media;
using Filtration.Enums;
using Filtration.Models.BlockItemBaseTypes;

namespace Filtration.Models.BlockItemTypes
{
    internal class WidthBlockItem : NumericFilterPredicateBlockItem
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
            get { return Colors.Tan; }
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
    }
}
