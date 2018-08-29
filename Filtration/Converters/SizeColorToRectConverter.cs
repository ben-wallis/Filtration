using Filtration.ObjectModel.Enums;
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
            var size = (int)(values[0]);
            var color = (int)(values[1]);

            if (size < 0 || color < 0)
                return new Rect(0, 0, 0, 0);

            Rect cropArea = new Rect();
            cropArea.Width = 64;
            cropArea.Height = 64;
            cropArea.X = 0 + size * 64;
            cropArea.Y = 0 + color * 64;

            return cropArea;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}