using System.Windows.Media;
using Filtration.Enums;
using Filtration.Models.BlockItemBaseTypes;

namespace Filtration.Models.BlockItemTypes
{
    internal class LinkedSocketsBlockItem : NumericFilterPredicateBlockItem
    {
        public LinkedSocketsBlockItem()
        {
        }

        public LinkedSocketsBlockItem(FilterPredicateOperator predicateOperator, int predicateOperand)
            : base(predicateOperator, predicateOperand)
        {
        }

        public override string PrefixText
        {
            get { return "LinkedSockets"; }
        }

        public override int MaximumAllowed
        {
            get { return 2; }
        }

        public override string DisplayHeading
        {
            get
            {
                return "Linked Sockets";
            }
        }

        public override string SummaryText
        {
            get { return "Linked Sockets " + FilterPredicate; }
        }

        public override Color SummaryBackgroundColor
        {
            get { return Colors.Gold; }
        }

        public override Color SummaryTextColor
        {
            get { return Colors.Black; }
        }

        public override int SortOrder
        {
            get { return 6; }
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
