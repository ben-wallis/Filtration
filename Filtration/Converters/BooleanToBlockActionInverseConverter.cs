using System;
using System.Globalization;
using System.Windows.Data;
using Filtration.ObjectModel.Enums;

namespace Filtration.Converters
{
    internal class BooleanToBlockActionInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (BlockAction)value == BlockAction.Hide;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? BlockAction.Hide : BlockAction.Show;
        }
    }
}
