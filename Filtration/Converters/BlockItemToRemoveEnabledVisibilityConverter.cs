using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.Converters
{
    public class BlockItemToRemoveEnabledVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var actionBlock = value as ActionBlockItem;

            return actionBlock != null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
