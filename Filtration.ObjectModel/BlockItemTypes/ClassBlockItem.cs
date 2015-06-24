using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class ClassBlockItem : StringListBlockItem
    {
        public override string PrefixText { get { return "Class"; } }

        public override int MaximumAllowed
        {
            get { return 1; }
        }

        public override string DisplayHeading { get { return "Class"; } }

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

        public override Color SummaryBackgroundColor
        {
            get { return Colors.MediumSeaGreen; }
        }

        public override Color SummaryTextColor
        {
            get { return Colors.White; }
        }

        public override int SortOrder
        {
            get { return 10; }
        }
    }
}
