using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.ThemeEditor;

namespace Filtration.Converters
{
    public class AvailableThemeComponentsConverter :IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var themeComponentsList = values[0] as ThemeComponentCollection;
            if (themeComponentsList == null || themeComponentsList.Count == 0) return null;

            var blockItem = values[1] as BlockItemBase;
            if (blockItem == null) return null;

            ThemeComponentType themeComponentType;

            if (blockItem.GetType() == typeof(BackgroundColorBlockItem))
            {
                themeComponentType = ThemeComponentType.BackgroundColor;
            }
            else if (blockItem.GetType() == typeof(TextColorBlockItem))
            {
                themeComponentType = ThemeComponentType.TextColor;
            }
            else if (blockItem.GetType() == typeof(BorderColorBlockItem))
            {
                themeComponentType = ThemeComponentType.BorderColor;
            }
            else if (blockItem.GetType() == typeof(FontSizeBlockItem))
            {
                themeComponentType = ThemeComponentType.FontSize;
            }
            else if (blockItem.GetType() == typeof(SoundBlockItem) || blockItem.GetType() == typeof(PositionalSoundBlockItem))
            {
                themeComponentType = ThemeComponentType.AlertSound;
            }
            else if (blockItem.GetType() == typeof(CustomSoundBlockItem))
            {
                themeComponentType = ThemeComponentType.CustomSound;
            }
            else if (blockItem.GetType() == typeof(MapIconBlockItem))
            {
                themeComponentType = ThemeComponentType.Icon;
            }
            else if (blockItem.GetType() == typeof(PlayEffectBlockItem))
            {
                themeComponentType = ThemeComponentType.Effect;
            }
            else
            {
                return null;
            }

            return themeComponentsList.Where(t => t.ComponentType == themeComponentType).ToList();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
