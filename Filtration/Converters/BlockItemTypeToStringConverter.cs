using System;
using System.Globalization;
using System.Windows.Data;
using Filtration.ObjectModel;

namespace Filtration.Converters
{
    internal class BlockItemTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var test = (IItemFilterBlockItem)Activator.CreateInstance((Type) value);
            return test.DisplayHeading;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
