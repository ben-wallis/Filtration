using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class BaseTypeBlockItem : StringListBlockItem
    {
        public override string PrefixText => "BaseType";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Base Type";

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

        public override Color SummaryBackgroundColor => Colors.MediumTurquoise;
        public override Color SummaryTextColor => Colors.Black;
        public override int SortOrder => 11;
    }
}
