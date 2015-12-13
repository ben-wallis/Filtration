using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Filtration.Common.Converters
{
    public class ColorToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? new SolidColorBrush((Color)value) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((SolidColorBrush) value)?.Color;
        }
    }
}
