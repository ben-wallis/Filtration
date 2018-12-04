using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class SocketGroupBlockItem : StringListBlockItem
    {
        public SocketGroupBlockItem()
        {

        }

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

        public IEnumerable<SocketGroup> SocketGroups
        {
            get
            {
                return
                    Items.Select(socketGroup => socketGroup.Select(socketChar => new Socket(StringToSocketColor(socketChar))).ToList())
                        .Select(socketList => new SocketGroup(socketList, true))
                        .ToList();
            }
        }

        public override Color SummaryBackgroundColor => Colors.GhostWhite;
        public override Color SummaryTextColor => Colors.Black;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.SocketGroup;

        private SocketColor StringToSocketColor(char socketColorString)
        {
            switch (socketColorString)
            {
                case 'R':
                {
                    return SocketColor.Red;
                }
                case 'G':
                {
                    return SocketColor.Green;
                }
                case 'B':
                {
                    return SocketColor.Blue;
                }
                case 'W':
                {
                    return SocketColor.White;
                }
                default:
                {
                    throw new InvalidOperationException("Invalid socket color");
                }
            }
        }
    }
}
