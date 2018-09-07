using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Filtration.ObjectModel.Enums;

namespace Filtration.Converters
{
	internal class MinimapIconToCroppedBitmapConverter : IMultiValueConverter
	{
		private static readonly int cellHeight = 64;
		private static readonly int cellWidth = 64;
		private static readonly int gridWidth = 14;
		private static readonly int startColumn = 4;
		private static readonly int startRow = 3;
		private static readonly int emptyColumn = 2;
		private static readonly int emptyRow = 11;
		private static readonly int colorCount = 6;
		private static readonly int shapeCount = 6;
		private static readonly int sizeCount = 3;

		private static readonly Uri uri;
		private static readonly CroppedBitmap empty;
		private static readonly List<CroppedBitmap> bitmaps;

		static MinimapIconToCroppedBitmapConverter()
		{
			uri = new Uri("pack://application:,,,/Filtration;component/Resources/minimap_icons.png", UriKind.Absolute);
			var sourceImage = new BitmapImage(uri);

			var emptyRect = new Int32Rect
			{
				Width = cellWidth,
				Height = cellHeight,
				X = emptyColumn * cellWidth,
				Y = emptyRow * cellHeight
			};
			
			empty = new CroppedBitmap(new BitmapImage(uri), emptyRect);
			bitmaps = new List<CroppedBitmap>(shapeCount * colorCount * sizeCount);

			var row = startRow;
			var column = startColumn;
			for (var i = 0; i < shapeCount; i++) {
				for (var j = 0; j < colorCount; j++) {
					for (var k = 0; k < sizeCount; k++) {
						if (column == gridWidth) {
							column = 0;
							row++;
						}

						var bitmapRect = new Int32Rect
						{
							Width = cellWidth,
							Height = cellHeight,
							X = column * cellWidth,
							Y = row * cellHeight
						};

						bitmaps.Add(new CroppedBitmap(sourceImage, bitmapRect));
						column++;
					}
				}
			}
		}
		
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values[0] == DependencyProperty.UnsetValue ||
				values[1] == DependencyProperty.UnsetValue ||
				values[2] == DependencyProperty.UnsetValue)
			{
				return empty;
			}

			var iconSize = (int)(values[0]);
			var iconColor = (int)(values[1]);
			var iconShape = (int)(values[2]);

			if (!Enum.IsDefined(typeof(IconSize), iconSize) ||
				!Enum.IsDefined(typeof(IconColor), iconColor) ||
				!Enum.IsDefined(typeof(IconShape), iconShape))
			{
				return empty;
			}

			var shapeOffset = iconShape * (sizeCount * colorCount);
			var colorOffset = iconColor * sizeCount;
			var iconIndex = shapeOffset + colorOffset + iconSize;

			if (iconIndex >= bitmaps.Count)
			{
				return empty;
			}
			else
			{
				return bitmaps[iconIndex];
			}
		}

		public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
