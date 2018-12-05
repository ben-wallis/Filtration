using System.Collections.ObjectModel;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Filtration.Common.Utilities
{
    public static class PathOfExileColors
    {
        static PathOfExileColors()
        {
            DefaultColors = new ObservableCollection<ColorItem>
            {
                 new ColorItem(new Color {A = 240, R=127, G = 127, B = 127}, "Default"),
                        new ColorItem(new Color {A = 240, R=255, G = 255, B = 255}, "Value Default"),
                        new ColorItem(new Color {A = 240, R=255, G = 192, B = 203}, "Pink"),
                        new ColorItem(new Color {A = 240, R=30, G = 144, B = 255}, "Dodger Blue"),
                        new ColorItem(new Color {A = 240, R=150, G = 0, B = 0}, "Fire"),
                        new ColorItem(new Color {A = 240, R=54, G = 100, B = 146}, "Cold"),
                        new ColorItem(new Color {A = 240, R=255, G = 215, B = 0}, "Lightning"),
                        new ColorItem(new Color {A = 240, R=208, G = 32, B = 144}, "Chaos"),
                        new ColorItem(new Color {A = 240, R=136, G = 136, B = 255}, "Augmented"),
                        new ColorItem(new Color {A = 240, R=184, G = 218, B = 242}, "Crafted"),
                        new ColorItem(new Color {A = 240, R=210, G = 0, B = 0}, "Unmet"),
                        new ColorItem(new Color {A = 240, R=175, G = 96, B = 37}, "Unique Item"),
                        new ColorItem(new Color {A = 240, R=255, G = 255, B = 119}, "Rare Item"),
                        new ColorItem(new Color {A = 240, R=136, G = 136, B = 255}, "Magic Item"),
                        new ColorItem(new Color {A = 240, R=200, G = 200, B = 200}, "White Item"),
                        new ColorItem(new Color {A = 240, R=27, G = 162, B = 155}, "Gem Item"),
                        new ColorItem(new Color {A = 240, R=170, G = 158, B = 130}, "Currency Item"),
                        new ColorItem(new Color {A = 240, R=74, G = 230, B = 58}, "Quest Item"),
                        new ColorItem(new Color {A = 240, R=255, G = 200, B = 0}, "Nemesis Mod"),
                        new ColorItem(new Color {A = 220, R = 255, G = 40, B = 0}, "Nemesis Mod Outline"),
                        new ColorItem(new Color {A = 240, R=231, G = 180, B = 120}, "Title"),
                        new ColorItem(new Color {A = 240, R=210, G = 0, B = 0}, "Corrupted"),
                        new ColorItem(new Color {A = 240, R=170, G = 158, B = 130}, "Favour"),
                        new ColorItem(new Color {A = 240, R=180, G = 96, B = 0}, "Supporter Pack New Item"),
                        new ColorItem(new Color {A = 240, R=163, G = 141, B = 109}, "Supporter Pack Item"),
                        new ColorItem(new Color {A = 240, R=210, G = 0, B = 220}, "Bloodline Mod"),
                        new ColorItem(new Color {A = 200, R = 74, G = 0, B = 160}, "Bloodline Mod Outline"),
                        new ColorItem(new Color {A = 240, R=50, G = 230, B = 100}, "Torment Mod"),
                        new ColorItem(new Color {A = 200, R = 0, G = 100, B = 150}, "Torment Mod Outline"),
                        new ColorItem(new Color {A = 240, R=210, G = 0, B = 0}, "Can't Trade or Modify")
            };
        }

        public static ObservableCollection<ColorItem> DefaultColors { get; private set; }
    }
}
