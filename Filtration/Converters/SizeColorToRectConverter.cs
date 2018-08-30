using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Filtration.Converters
{
    internal class SizeColorToRectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue ||
                values[1] == DependencyProperty.UnsetValue)
                return null;

            var size = (int)(values[0]);
            var color = (int)(values[1]);

            if (size < 0 || color < 0)
                return new Rect(0, 0, 0, 0);

            var cropArea = new Rect
            {
                Width = 64,
                Height = 64,
                X = 0 + size * 64,
                Y = 0 + color * 64
            };

            return cropArea;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}