using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Xml.Serialization;
using Filtration.ObjectModel;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Extensions;

namespace Filtration.ItemFilterPreview.Model
{
    public interface IItem
    {
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
        IEnumerable<SocketGroup> LinkedSocketGroups { get; }
        List<SocketGroup> SocketGroups { get; set; }
        Color DefaultTextColor { get; }
    }

    [Serializable]
    public class Item : IItem
    {
        private List<SocketGroup> _socketGroups;
        
        public string ItemClass { get; set; }
        public string BaseType { get; set; }
        public int DropLevel { get; set; }
        public int ItemLevel { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Quality { get; set; }
        public ItemRarity ItemRarity { get; set; }

        [XmlIgnore]
        public int Sockets { get; private set; }
        [XmlIgnore]
        public int LinkedSockets { get; private set; }

        [XmlIgnore]
        public IEnumerable<SocketGroup> LinkedSocketGroups
        {
            get { return SocketGroups.Where(s => s.Linked); }
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
                var maxSocketGroups = Math.Max(1, evenSocketCount ? socketCount / 2 : socketCount - 1);
                var maxLinkedSocketGroups = Math.Max(1, evenSocketCount ? maxSocketGroups : maxSocketGroups - 1);

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

                var linkedSocketGroups = value.Where(s => s.Linked).ToList();
                LinkedSockets = linkedSocketGroups.Any() ? linkedSocketGroups.Max(s => s.Count) : 0;
                
            }
        }

        public Color DefaultTextColor => ItemRarity.DefaultRarityTextColor();
    }
}
