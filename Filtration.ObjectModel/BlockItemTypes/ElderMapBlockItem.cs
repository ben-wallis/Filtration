using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public sealed class ElderMapBlockItem : BooleanBlockItem
    {
        public ElderMapBlockItem()
        {
        }

        public ElderMapBlockItem(bool booleanValue) : base(booleanValue)
        {
        }

        public override string PrefixText => "ElderMap";
        public override string DisplayHeading => "Elder Map";
        public override Color SummaryBackgroundColor => Colors.DarkGoldenrod;
        public override Color SummaryTextColor => Colors.White;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.ElderMap;
    }
}
