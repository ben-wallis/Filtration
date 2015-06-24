using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class SocketsBlockItem : NumericFilterPredicateBlockItem
    {
        public SocketsBlockItem()
        {
        }

        public SocketsBlockItem(FilterPredicateOperator predicateOperator, int predicateOperand)
            : base(predicateOperator, predicateOperand)
        {
        }

        public override string PrefixText
        {
            get { return "Sockets"; }
        }

        public override int MaximumAllowed
        {
            get { return 2; }
        }

        public override string DisplayHeading
        {
            get
            {
                return "Sockets";
            }
        }

        public override string SummaryText
        {
            get { return "Sockets " + FilterPredicate; }
        }

        public override Color SummaryBackgroundColor
        {
            get { return Colors.LightGray; }
        }

        public override Color SummaryTextColor
        {
            get { return Colors.Black; }
        }

        public override int SortOrder
        {
            get { return 5; }
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
