using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Filtration.Converters
{
    internal class BlockGroupVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var blockGroupAdvanced = (bool) values[0];
            var showAdvanced = (bool)values[1];

            if (!blockGroupAdvanced || showAdvanced)
            {
                return Visibility.Visible;
            }
            return Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
