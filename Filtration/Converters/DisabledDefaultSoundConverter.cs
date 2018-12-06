using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Filtration.Converters
{
    public class DisabledDefaultSoundConverter : IValueConverter
    {
        private static readonly BitmapImage _soundIcon;
        private static readonly BitmapImage _soundDDSIcon;

        static DisabledDefaultSoundConverter()
        {
            var soundUri = new Uri("pack://application:,,,/Filtration;component/Resources/Icons/sound.png", UriKind.Absolute);
            var soundDDSUri = new Uri("pack://application:,,,/Filtration;component/Resources/Icons/sound_dds.png", UriKind.Absolute);
            _soundIcon = new BitmapImage(soundUri);
            _soundDDSIcon = new BitmapImage(soundDDSUri);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? _soundDDSIcon : _soundIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ReferenceEquals(value, _soundDDSIcon);
        }
    }
}
