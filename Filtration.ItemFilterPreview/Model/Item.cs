using System;
using System.Collections.Generic;
using System.Linq;
using Filtration.ObjectModel.Enums;

namespace Filtration.ItemFilterPreview.Model
{
    public interface IItem
    {
        List<SocketGroup> SocketGroups { get; set; }
        string ItemClass { get; set; }
        string BaseType { get; set; }
        int DropLevel { get; set; }
        int ItemLevel { get; set; }
        int Height { get; set; }
        int Width { get; set; }
        int Quality { get; set; }
        ItemRarity ItemRarity { get; set; }
        int Sockets { get; }
        int LinkedSockets { get; }
    }

    public class Item : IItem
    {
        private List<SocketGroup> _socketGroups;

        public Item(List<SocketGroup> socketGroups)
        {
           
        }

        public List<SocketGroup> SocketGroups
        {
            get { return _socketGroups; }
            set
            {
                var socketCount = value.Sum(s => s.Count);
                if (socketCount < 1 || socketCount > 6)
                {
                    throw new InvalidOperationException("An item must have between 1 and 6 sockets");
                }

                var evenSocketCount = socketCount % 2 == 0;
                var maxSocketGroups = evenSocketCount ? socketCount / 2 : socketCount - 1;
                var maxLinkedSocketGroups = evenSocketCount ? maxSocketGroups : maxSocketGroups - 1;

                if (value.Count > maxSocketGroups)
                {
                    throw new InvalidOperationException("Invalid number of socket groups for the socket count of this item");
                }

                if (value.Count(s => s.Linked) > maxLinkedSocketGroups)
                {
                    throw new InvalidOperationException("Invalid number of linked socket groups for the socket count of this item");
                }

                _socketGroups = value;
                Sockets = socketCount;
                LinkedSockets = value.Where(s => s.Linked).Max(s => s.Count);
            }
        }

        public string ItemClass { get; set; }
        public string BaseType { get; set; }
        public int DropLevel { get; set; }
        public int ItemLevel { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Quality { get; set; }
        public ItemRarity ItemRarity { get; set; }
        public int Sockets { get; private set; }
        public int LinkedSockets { get; private set; }
    }
}
