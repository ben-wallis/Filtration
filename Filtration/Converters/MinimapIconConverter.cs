using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Filtration.ObjectModel.Enums;

namespace Filtration.Converters
{
	internal class MinimapIconConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var prefix = "pack://application:,,,/Filtration;component/Resources/DropIcons";
			var noIconPath = $"{prefix}/none.png";

			if (values[0] == DependencyProperty.UnsetValue ||
				values[1] == DependencyProperty.UnsetValue ||
				values[2] == DependencyProperty.UnsetValue) {
				return new BitmapImage(new Uri(noIconPath, UriKind.Absolute));
			}

			var iconSize = (IconSize)(int)(values[0]);
			var iconColor = (IconColor)(int)(values[1]);
			var iconShape = (IconShape)(int)(values[2]);
			string iconSizeText, iconColorText, iconShapeText;

			switch (iconSize) {
				case IconSize.Largest:
					iconSizeText = "0";
					break;
				case IconSize.Medium:
					iconSizeText = "1";
					break;
				case IconSize.Small:
					iconSizeText = "2";
					break;
				default:
					return new BitmapImage(new Uri(noIconPath, UriKind.Absolute));
			}

			switch (iconColor) {
				case IconColor.Blue:
					iconColorText = "blue";
					break;
				case IconColor.Brown:
					iconColorText = "brown";
					break;
				case IconColor.Green:
					iconColorText = "green";
					break;
				case IconColor.Red:
					iconColorText = "red";
					break;
				case IconColor.White:
					iconColorText = "white";
					break;
				case IconColor.Yellow:
					iconColorText = "yellow";
					break;
				default:
					return new BitmapImage(new Uri(noIconPath, UriKind.Absolute));
			}

			switch (iconShape) {
				case IconShape.Circle:
					iconShapeText = "circle";
					break;
				case IconShape.Diamond:
					iconShapeText = "diamond";
					break;
				case IconShape.Hexagon:
					iconShapeText = "hexagon";
					break;
				case IconShape.Square:
					iconShapeText = "square";
					break;
				case IconShape.Star:
					iconShapeText = "star";
					break;
				case IconShape.Triangle:
					iconShapeText = "triangle";
					break;
				default:
					return new BitmapImage(new Uri(noIconPath, UriKind.Relative));
			}

			return new BitmapImage(new Uri($"{prefix}/{iconShapeText}_{iconColorText}_{iconSizeText}.png",
				UriKind.Absolute));
		}

		public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
