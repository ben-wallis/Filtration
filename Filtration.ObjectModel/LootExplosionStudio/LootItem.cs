using System.Collections.Generic;
using System.Windows.Media;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.LootExplosionStudio
{
    public class LootItem
    {
        public string Name { get; set; }
        public int ItemLevel { get; set; }
        public int DropLevel { get; set; }
        public int Quality { get; set; }
        public ItemRarity Rarity { get; set; }
        public string Class { get; set; }
        public string BaseType { get; set; }
        public List<SocketGroup> SocketGroups { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        
        public Color TextColor { get; set; }
        public Color BackgroundColor { get; set; }
        public Color BorderColor { get; set; }
        public int FontSize { get; set; }
    }
}
