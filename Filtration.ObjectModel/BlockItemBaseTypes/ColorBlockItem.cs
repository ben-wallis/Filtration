using System;
using System.Windows.Media;
using Filtration.ObjectModel.ThemeEditor;

namespace Filtration.ObjectModel.BlockItemBaseTypes
{
    public abstract class ColorBlockItem : BlockItemBase, IAudioVisualBlockItem, IBlockItemWithTheme
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

        public override string OutputText => PrefixText + " " + +Color.R + " " + Color.G + " "
                                             + Color.B + (Color.A != 240 ? " " + Color.A : string.Empty) +
                                             (ThemeComponent != null ? " # " + ThemeComponent.ComponentName : string.Empty);

        public override string SummaryText => string.Empty;

        public ThemeComponent ThemeComponent
        {
            get { return _themeComponent; }
            set
            {
                if (_themeComponent == value){ return;}

                if (_themeComponent != null)
                {
                    _themeComponent.ThemeComponentUpdated -= OnThemeComponentUpdated;
                    _themeComponent.ThemeComponentDeleted -= OnThemeComponentDeleted;
                }
                if (value != null)
                {
                    value.ThemeComponentUpdated += OnThemeComponentUpdated;
                    value.ThemeComponentDeleted += OnThemeComponentDeleted;
                }
                
                _themeComponent = value;
                OnPropertyChanged();
            }
        }

        public override Color SummaryBackgroundColor => Colors.Transparent;
        public override Color SummaryTextColor => Colors.Transparent;

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                IsDirty = true;
                OnPropertyChanged();
            }
        }

        private void OnThemeComponentUpdated(object sender, EventArgs e)
        {
            Color = ((ColorThemeComponent) sender).Color;
        }

        private void OnThemeComponentDeleted(object sender, EventArgs e)
        {
            ThemeComponent = null;
        }
    }
}
