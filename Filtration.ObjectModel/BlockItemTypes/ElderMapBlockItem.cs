using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;

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
        public override int SortOrder => 9;

    }
}
