using System.Linq;
using System.Windows.Media;
using Filtration.Models.BlockItemBaseTypes;

namespace Filtration.Models.BlockItemTypes
{
    internal class SocketGroupBlockItem : StringListBlockItem
    {
        public override string PrefixText
        {
            get { return "SocketGroup"; }
        }

        public override int MaximumAllowed
        {
            get { return 1; }
        }

        public override string DisplayHeading
        {
            get
            {
                return "Socket Group";
            }
        }

        public override string SummaryText
        {
            get
            {
                var summaryItemText = " " + Items.Aggregate(string.Empty, (current, i) => current + " " + i);
                return "Socket Group " + summaryItemText.TrimStart(' ');
            }
        }

        public override Color SummaryBackgroundColor
        {
            get { return Colors.GhostWhite; }
        }

        public override Color SummaryTextColor
        {
            get { return Colors.Black; }
        }

        public override int SortOrder
        {
            get { return 9; }
        }
    }
}
