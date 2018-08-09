using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class GemLevelBlockItem : NumericFilterPredicateBlockItem
    {
        public override string PrefixText => "GemLevel";
        public override int MaximumAllowed => 2;
        public override string DisplayHeading => "Gem Level";
        public override string SummaryText => "Gem Level " + FilterPredicate;
        public override Color SummaryBackgroundColor => Colors.DarkSlateGray;
        public override Color SummaryTextColor => Colors.White;
        public override int SortOrder => 14;
        public override int Minimum => 0;
        public override int Maximum => 40;
    }
}
