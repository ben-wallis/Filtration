using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class HasExplicitModBlockItem : StringListBlockItem
    {
        public override string PrefixText => "HasExplicitMod";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Has Explicit Mod";

        public override string SummaryText
        {
            get
            {
                if (Items.Count > 0 && Items.Count < 4)
                {
                    return "Item Explicit Mods: " +
                           Items.Aggregate(string.Empty, (current, i) => current + i + ", ").TrimEnd(' ').TrimEnd(',');
                }
                if (Items.Count >= 4)
                {
                    var remaining = Items.Count - 3;
                    return "Item Explicit Mods: " + Items.Take(3)
                        .Aggregate(string.Empty, (current, i) => current + i + ", ")
                        .TrimEnd(' ')
                        .TrimEnd(',') + " (+" + remaining + " more)";
                }
                return "Item Explicit Mods: (none)";
            }
        }

        public override Color SummaryBackgroundColor => Colors.MidnightBlue;
        public override Color SummaryTextColor => Colors.White;
        public override int SortOrder => 19;
    }
}
