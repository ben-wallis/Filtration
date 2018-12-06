using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Filtration.Converters
{
    class DisabledDefaultSoundTooltipConverter : IMultiValueConverter
    {
        private static readonly string appendage = "\nNote: the default drop sound is disabled for this block.";

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue ||
                values[1] == DependencyProperty.UnsetValue)
            {
                return string.Empty;
            }

            var baseText = (string)(values[0]);
            var hasDisabledDefaultSound = (bool)(values[1]);

            if (hasDisabledDefaultSound)
            {
                return $"{baseText}{appendage}";
            }
            else
            {
                return baseText;
            }
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
