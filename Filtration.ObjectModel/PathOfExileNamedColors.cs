using System.Collections.Generic;
using System.Windows.Media;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel
{
    public static class PathOfExileNamedColors
    {
        public static Dictionary<PathOfExileNamedColor, Color> Colors => new Dictionary<PathOfExileNamedColor, Color>
        {
            {PathOfExileNamedColor.Default, new Color {A = 240, R = 127, G = 127, B = 127}},
            {PathOfExileNamedColor.ValueDefault, new Color {A = 240, R = 255, G = 255, B = 255}},
            {PathOfExileNamedColor.Pink, new Color {A = 240, R = 255, G = 192, B = 203}},
            {PathOfExileNamedColor.DodgerBlue, new Color {A = 240, R = 30, G = 144, B = 255}},
            {PathOfExileNamedColor.Fire, new Color {A = 240, R = 150, G = 0, B = 0}},
            {PathOfExileNamedColor.Cold, new Color {A = 240, R = 54, G = 100, B = 146}},
            {PathOfExileNamedColor.Lightning, new Color {A = 240, R = 255, G = 215, B = 0}},
            {PathOfExileNamedColor.Chaos, new Color {A = 240, R = 208, G = 32, B = 144}},
            {PathOfExileNamedColor.Augmented, new Color {A = 240, R = 136, G = 136, B = 255}},
            {PathOfExileNamedColor.Crafted, new Color {A = 240, R = 184, G = 218, B = 242}},
            {PathOfExileNamedColor.Unmet, new Color {A = 240, R = 210, G = 0, B = 0}},
            {PathOfExileNamedColor.UniqueItem, new Color {A = 240, R = 175, G = 96, B = 37}},
            {PathOfExileNamedColor.RareItem, new Color {A = 240, R = 255, G = 255, B = 119}},
            {PathOfExileNamedColor.MagicItem, new Color {A = 240, R = 136, G = 136, B = 255}},
            {PathOfExileNamedColor.WhiteItem, new Color {A = 240, R = 200, G = 200, B = 200}},
            {PathOfExileNamedColor.GemItem, new Color {A = 240, R = 27, G = 162, B = 155}},
            {PathOfExileNamedColor.CurrencyItem, new Color {A = 240, R = 170, G = 158, B = 130}},
            {PathOfExileNamedColor.QuestItem, new Color {A = 240, R = 74, G = 230, B = 58}},
            {PathOfExileNamedColor.NemesisMod, new Color {A = 240, R = 255, G = 200, B = 0}},
            {PathOfExileNamedColor.NemesisModOutline, new Color {A = 220, R = 255, G = 40, B = 0}},
            {PathOfExileNamedColor.Title, new Color {A = 240, R = 231, G = 180, B = 120}},
            {PathOfExileNamedColor.Corrupted, new Color {A = 240, R = 210, G = 0, B = 0}},
            {PathOfExileNamedColor.Favour, new Color {A = 240, R = 170, G = 158, B = 130}},
            {PathOfExileNamedColor.SupporterPackNewItem, new Color {A = 240, R = 180, G = 96, B = 0}},
            {PathOfExileNamedColor.SupporterPackItem, new Color {A = 240, R = 163, G = 141, B = 109}},
            {PathOfExileNamedColor.BloodlineMod, new Color {A = 240, R = 210, G = 0, B = 220}},
            {PathOfExileNamedColor.BloodlineModOutline, new Color {A = 200, R = 74, G = 0, B = 160}},
            {PathOfExileNamedColor.TormentMod, new Color {A = 240, R = 50, G = 230, B = 100}},
            {PathOfExileNamedColor.TormentModOutline, new Color {A = 200, R = 0, G = 100, B = 150}},
            {PathOfExileNamedColor.CantTradeorModify, new Color {A = 240, R = 210, G = 0, B = 0}},
        };

    }
}
