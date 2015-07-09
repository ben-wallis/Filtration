using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.LootExplosionStudio;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class SocketGroupBlockItem : StringListBlockItem
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

        public override string GetLootItemProperty(LootItem lootItem)
        {
            return lootItem.SocketGroups.ToString();
        }

        public override bool MatchesLootItem(LootItem lootItem)
        {
            foreach (var socketGroupString in Items)
            {
                var socketColorList = SocketGroupStringToSocketColors(socketGroupString);
                if (
                    lootItem.SocketGroups.Any(
                        g =>
                            g.Sockets.Count(s => s == SocketColor.Red) >=
                            socketColorList.Count(l => l == SocketColor.Red) &&
                            g.Sockets.Count(s => s == SocketColor.Green) >=
                            socketColorList.Count(l => l == SocketColor.Green) &&
                            g.Sockets.Count(s => s == SocketColor.Blue) >=
                            socketColorList.Count(l => l == SocketColor.Blue) &&
                            g.Sockets.Count(s => s == SocketColor.White) >=
                            socketColorList.Count(l => l == SocketColor.White)))
                {
                    return true;
                }
            }

            return false;
        }

        private List<SocketColor> SocketGroupStringToSocketColors(string socketGroupString)
        {
            var socketColorList = new List<SocketColor>();

            foreach (var c in socketGroupString.ToCharArray())
            {
                switch (c)
                {
                    case 'R':
                    {
                        socketColorList.Add(SocketColor.Red);
                        break;
                    }
                    case 'G':
                    {
                        socketColorList.Add(SocketColor.Green);
                        break;
                    }
                    case 'B':
                    {
                        socketColorList.Add(SocketColor.Blue);
                        break;
                    }
                    case 'W':
                    {
                        socketColorList.Add(SocketColor.White);
                        break;
                    }
                }
            }

            return socketColorList;
        }
    }
}
