using System;
using System.Globalization;
using System.Windows.Data;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Extensions;

namespace Filtration.ThemeEditor.Converters
{
    public class ThemeComponentTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            
            }
            var type = (ThemeComponentType) value;

            switch (type.GetAttributeDescription())
            {
                case "TextColor":
                {
                    return "Text";
                }
                case "BorderColor":
                {
                    return "Border";
                }
                case "BackgroundColor":
                {
                    return "Background";
                }
            }

            return type.GetAttributeDescription();

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
