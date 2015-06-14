using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Filtration.Converters
{
    internal class BlockGroupAdvancedColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var advanced = (bool) value;

            return advanced ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
