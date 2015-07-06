using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Filtration.Common.Converters
{
    internal class InverseBooleanVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && targetType == typeof (Visibility))
            {
                var val = (bool) value;
                if (val)
                {
                    return Visibility.Collapsed;
                }
                if (parameter is Visibility)
                {
                    return parameter;
                }
                return Visibility.Visible;
            }
            if (value != null)
            {
                return Visibility.Collapsed;
            }

            if (parameter is Visibility)
            {
                return parameter;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
