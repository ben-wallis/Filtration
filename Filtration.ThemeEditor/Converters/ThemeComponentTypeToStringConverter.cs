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
                case "Text":
                {
                    return "Text Color Theme Components";
                }
                case "Border":
                {
                    return "Border Color Theme Components";
                }
                case "Background":
                {
                    return "Background Color Theme Components";
                }
                case "Font Size":
                {
                    return "Font Size Theme Components";
                }
                case "Alert Sound":
                {
                    return "Alert Sound Theme Components";
                }
                case "Custom Sound":
                {
                    return "Custom Alert Sound Theme Components";
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
