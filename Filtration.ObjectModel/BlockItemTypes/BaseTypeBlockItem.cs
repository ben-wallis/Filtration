using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class BaseTypeBlockItem : StringListBlockItem
    {
        public override string PrefixText { get { return "BaseType"; } }

        public override int MaximumAllowed
        {
            get { return 1; }
        }

        public override string DisplayHeading
        {
            get
            {
                return "Base Type";
            }
        }

        public override string SummaryText
        {
            get
            {
                if (Items.Count > 0 && Items.Count < 4)
                {
                    return "Item Base Types: " +
                           Items.Aggregate(string.Empty, (current, i) => current + i + ", ").TrimEnd(' ').TrimEnd(',');
                }
                if (Items.Count >= 4)
                {
                    var remaining = Items.Count - 3;
                    return "Item Base Types: " + Items.Take(3)
                        .Aggregate(string.Empty, (current, i) => current + i + ", ")
                        .TrimEnd(' ')
                        .TrimEnd(',') + " (+" + remaining + " more)";
                }
                return "Item Base Types: (none)";
            }
        }

        public override Color SummaryBackgroundColor
        {
            get { return Colors.MediumTurquoise; }
        }

        public override Color SummaryTextColor
        {
            get { return Colors.Black; }
        }

        public override int SortOrder
        {
            get { return 11; }
        }
    }
}
