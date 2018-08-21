using System;
using System.Globalization;
using System.Windows.Data;

namespace Filtration.Converters
{
    internal class DropIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var iconString = (string)value;
            switch(iconString)
            {
                case "Icon1":
                    return "/Filtration;component/Resources/DropIcons/Icon1.png";
                case "Icon2":
                    return "/Filtration;component/Resources/DropIcons/Icon2.png";
                case "Icon3":
                    return "/Filtration;component/Resources/DropIcons/Icon3.png";
                case "Icon4":
                    return "/Filtration;component/Resources/DropIcons/Icon4.png";
                case "Icon5":
                    return "/Filtration;component/Resources/DropIcons/Icon5.png";
                case "Icon6":
                    return "/Filtration;component/Resources/DropIcons/Icon6.png";
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}