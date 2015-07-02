using System;
using System.Globalization;
using System.Windows.Data;

namespace Filtration.Converters
{
    internal class HashSignRemovalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var inputString = (string) value;
            return inputString.Replace("#", string.Empty);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
