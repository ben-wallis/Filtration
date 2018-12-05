using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Extensions;

namespace Filtration.ObjectModel
{
    public interface IItem
    {
        string Description { get; set; }
        string ItemClass { get; set; }
        string BaseType { get; set; }
        int DropLevel { get; set; }
        int ItemLevel { get; set; }
        int Height { get; set; }
        int Width { get; set; }
        int Quality { get; set; }
        ItemRarity ItemRarity { get; set; }
        int SocketCount { get; }
        int LinkedSockets { get; }
        IEnumerable<SocketGroup> LinkedSocketGroups { get; }
        List<SocketGroup> SocketGroups { get; set; }
        Color DefaultTextColor { get; }
    }

    [Table("Item")]
    public class Item : IItem
    {
        private List<SocketGroup> _socketGroups;

        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string BaseType { get; set; }

        [Required]
        [StringLength(100)]
        public string ItemClass { get; set; }

        public int DropLevel { get; set; }

        public int ItemLevel { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public int Quality { get; set; }
        
        public ItemRarity ItemRarity { get; set; }

        [StringLength(20)]
        public string Sockets { get; set; }

        public long ItemSetId { get; set; }

        public virtual ItemSet ItemSet { get; set; }

        public int SocketCount { get; private set; }
        public int LinkedSockets { get; private set; }

        public IEnumerable<SocketGroup> LinkedSocketGroups
        {
            get { return SocketGroups.Where(s => s.Linked); }
        } 

        [NotMapped]
        public List<SocketGroup> SocketGroups
        {
            get { return _socketGroups; }
            set
            {
                var socketCount = value.Sum(s => s.Count);
                if (socketCount < 0 || socketCount > 6)
                {
                    throw new InvalidOperationException("An item must have between 0 and 6 sockets");
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
                SocketCount = socketCount;

                var linkedSocketGroups = value.Where(s => s.Linked).ToList();
                LinkedSockets = linkedSocketGroups.Any() ? linkedSocketGroups.Max(s => s.Count) : 0;
                
            }
        }

        public Color DefaultTextColor
        {
            get
            {
                if (ItemClass.Contains("Gems"))
                {
                    return PathOfExileNamedColors.Colors[PathOfExileNamedColor.GemItem];
                }
                if (ItemClass.Contains("Quest"))
                {
                    return PathOfExileNamedColors.Colors[PathOfExileNamedColor.QuestItem];
                }

                return ItemRarity.DefaultRarityTextColor();
            }
        }
    }
}
