using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class LinkedSocketsBlockItem : NumericFilterPredicateBlockItem
    {
        public LinkedSocketsBlockItem()
        {
        }

        public LinkedSocketsBlockItem(FilterPredicateOperator predicateOperator, int predicateOperand)
            : base(predicateOperator, predicateOperand)
        {
        }

        public override string PrefixText => "LinkedSockets";
        public override int MaximumAllowed => 2;
        public override string DisplayHeading => "Linked Sockets";
        public override string SummaryText => "Linked Sockets " + FilterPredicate;
        public override Color SummaryBackgroundColor => Colors.Gold;
        public override Color SummaryTextColor => Colors.Black;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.LinkedSockets;
        public override int Minimum => 0;
        public override int Maximum => 6;
    }
}
