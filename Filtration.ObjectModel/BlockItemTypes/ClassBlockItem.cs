using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class ClassBlockItem : StringListBlockItem
    {
        public override string PrefixText => "Class";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Class";

        public override string SummaryText
        {
            get
            {
                if (Items.Count > 0 && Items.Count < 4)
                {
                    return "Item Classes: " +
                           Items.Aggregate(string.Empty, (current, i) => current + i + ", ").TrimEnd(' ').TrimEnd(',');
                }
                if (Items.Count >= 4)
                {
                    var remaining = Items.Count - 3;
                    return "Item Classes: " + Items.Take(3)
                        .Aggregate(string.Empty, (current, i) => current + i + ", ")
                        .TrimEnd(' ')
                        .TrimEnd(',') + " (+" + remaining + " more)";
                }
                return "Item Classes: (none)";
            }
        }

        public override Color SummaryBackgroundColor => Colors.MediumSeaGreen;
        public override Color SummaryTextColor => Colors.White;
        public override int SortOrder => 19;
    }
}
