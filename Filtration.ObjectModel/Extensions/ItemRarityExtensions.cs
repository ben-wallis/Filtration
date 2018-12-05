using System;
using System.Windows.Media;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.Extensions
{
    public static class ItemRarityExtensions
    {
        public static Color DefaultRarityTextColor(this ItemRarity itemRarity)
        {
            switch (itemRarity)
            {
                case ItemRarity.Magic:
                {
                    return PathOfExileNamedColors.Colors[PathOfExileNamedColor.MagicItem];
                }
                case ItemRarity.Normal:
                {
                    return PathOfExileNamedColors.Colors[PathOfExileNamedColor.WhiteItem];
                }
                case ItemRarity.Rare:
                {
                    return PathOfExileNamedColors.Colors[PathOfExileNamedColor.RareItem];
                }
                case ItemRarity.Unique:
                {
                    return PathOfExileNamedColors.Colors[PathOfExileNamedColor.UniqueItem];
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(itemRarity), itemRarity, null);
                }
            }
        }
    }
}
