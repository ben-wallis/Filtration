using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class MapTierBlockItem : NumericFilterPredicateBlockItem
    {
        public override string PrefixText => "MapTier";
        public override int MaximumAllowed => 2;
        public override string DisplayHeading => "Map Tier";
        public override string SummaryText => "Map Tier " + FilterPredicate;
        public override Color SummaryBackgroundColor => Colors.DarkSlateGray;
        public override Color SummaryTextColor => Colors.White;
        public override int SortOrder => 8;
        public override int Minimum => 1;
        public override int Maximum => 16;
    }
}
