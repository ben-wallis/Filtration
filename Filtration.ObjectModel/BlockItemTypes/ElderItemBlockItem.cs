using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public sealed class ElderItemBlockItem : BooleanBlockItem
    {
        public ElderItemBlockItem()
        {
        }

        public ElderItemBlockItem(bool booleanValue) : base(booleanValue)
        {
        }

        public override string PrefixText => "ElderItem";
        public override string DisplayHeading => "Elder Item";
        public override Color SummaryBackgroundColor => Colors.DarkGray;
        public override Color SummaryTextColor => Colors.White;
        public override int SortOrder => 6;

    }
}
