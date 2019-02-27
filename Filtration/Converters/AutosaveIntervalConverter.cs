using System;
using System.Globalization;
using System.Windows.Data;
using Filtration.Enums;

namespace Filtration.Converters
{
    public class IntToAutosaveIntervalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (AutosaveInterval)(int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)(AutosaveInterval)value;
        }
    }
}
