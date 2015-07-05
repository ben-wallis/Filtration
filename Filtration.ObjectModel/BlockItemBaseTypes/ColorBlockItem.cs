using System.ComponentModel;
using System.Windows.Media;
using Filtration.ObjectModel.ThemeEditor;

namespace Filtration.ObjectModel.BlockItemBaseTypes
{
    public abstract class ColorBlockItem : BlockItemBase, IAudioVisualBlockItem
    {
        private Color _color;
        private ThemeComponent _themeComponent;

        protected ColorBlockItem()
        {
        }
        
        protected ColorBlockItem(Color color)
        {
            Color = color;
        }

        public override string OutputText
        {
            get
            {
                return PrefixText + " " + +Color.R + " " + Color.G + " "
                       + Color.B + (Color.A < 255 ? " " + Color.A : string.Empty) +
                       (ThemeComponent != null ? " # " + ThemeComponent.ComponentName : string.Empty);
            }
        }

        public override string SummaryText
        {
            get { return string.Empty; }
        }

        public ThemeComponent ThemeComponent
        {
            get { return _themeComponent; }
            set
            {
                _themeComponent = value;
                OnPropertyChanged();
            }
        }

        public override Color SummaryBackgroundColor { get { return Colors.Transparent; } }
        public override Color SummaryTextColor { get { return Colors.Transparent; } }

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged();
            }
        }
    }
}
