using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Filtration.Converters
{
    public class BooleanToBlockMarginConverter : IValueConverter
    {
        public double Length { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = (bool)value;

            if (!item)
                return new Thickness(2, 0, 2, 0);

            return new Thickness(2, 2, 2, 2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Thickness)value) == new Thickness(0, 0, 0, 0);
        }
    }
}
