using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public sealed class CorruptedBlockItem : BooleanBlockItem
    {
        public CorruptedBlockItem()
        {
        }

        public CorruptedBlockItem(bool booleanValue) : base(booleanValue)
        {
        }

        public override string PrefixText => "Corrupted";
        public override string DisplayHeading => "Corrupted";
        public override Color SummaryBackgroundColor => Colors.DarkRed;
        public override Color SummaryTextColor => Colors.White;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.Corrupted;
    }
}
