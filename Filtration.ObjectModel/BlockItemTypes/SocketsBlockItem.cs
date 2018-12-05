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

        public override string PrefixText => "Sockets";
        public override int MaximumAllowed => 2;
        public override string DisplayHeading => "Sockets";
        public override string SummaryText => "Sockets " + FilterPredicate;
        public override Color SummaryBackgroundColor => Colors.LightGray;
        public override Color SummaryTextColor => Colors.Black;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.Sockets;
        public override int Minimum => 0;
        public override int Maximum => 6;
    }
}
