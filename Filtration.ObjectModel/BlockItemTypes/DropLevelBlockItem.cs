using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class DropLevelBlockItem : NumericFilterPredicateBlockItem
    {
        public DropLevelBlockItem()
        {
        }

        public DropLevelBlockItem(FilterPredicateOperator predicateOperator, int predicateOperand)
            : base(predicateOperator, predicateOperand)
        {
        }

        public override string PrefixText
        {
            get { return "DropLevel"; }
        }

        public override int MaximumAllowed
        {
            get { return 2; }
        }

        public override string DisplayHeading
        {
            get
            {
                return "Drop Level";
            }
        }

        public override string SummaryText
        {
            get { return "Drop Level " + FilterPredicate; }
        }

        public override Color SummaryBackgroundColor
        {
            get { return Colors.DodgerBlue; }
        }

        public override Color SummaryTextColor
        {
            get { return Colors.White; }
        }

        public override int SortOrder
        {
            get { return 2; }
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
