using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class SocketGroupBlockItem : StringListBlockItem
    {
        public override string PrefixText => "SocketGroup";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Socket Group";
        public override string SummaryText
        {
            get
            {
                var summaryItemText = " " + Items.Aggregate(string.Empty, (current, i) => current + " " + i);
                return "Socket Group " + summaryItemText.TrimStart(' ');
            }
        }
        public override Color SummaryBackgroundColor => Colors.GhostWhite;
        public override Color SummaryTextColor => Colors.Black;
        public override int SortOrder => 9;
    }
}
