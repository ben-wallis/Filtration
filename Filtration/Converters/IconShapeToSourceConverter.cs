using Filtration.ObjectModel.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Filtration.Converters
{
    internal class IconShapeToSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var iconShape = (IconShape)(int)value;
            switch (iconShape)
            {
                case IconShape.Circle:
                    return "/Filtration;component/Resources/DropIcons/Circle.png";
                case IconShape.Diamond:
                    return "/Filtration;component/Resources/DropIcons/Diamond.png";
                case IconShape.Hexagon:
                    return "/Filtration;component/Resources/DropIcons/Hexagon.png";
                case IconShape.Square:
                    return "/Filtration;component/Resources/DropIcons/Square.png";
                case IconShape.Star:
                    return "/Filtration;component/Resources/DropIcons/Star.png";
                case IconShape.Triangle:
                    return "/Filtration;component/Resources/DropIcons/Triangle.png";
            }

            return "/Filtration;component/Resources/DropIcons/NoIcon.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}