using System;
using System.Windows.Media;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Extensions;
using Filtration.ObjectModel.ThemeEditor;

namespace Filtration.ObjectModel.BlockItemBaseTypes
{
    public abstract class IconBlockItem : BlockItemBase, IAudioVisualBlockItem, IBlockItemWithTheme
    {
        private IconSize _size;
        private IconColor _color;
        private IconShape _shape;
        private ThemeComponent _themeComponent;

        protected IconBlockItem()
        {
        }

        protected IconBlockItem(IconSize size, IconColor color, IconShape shape)
        {
            Size = size;
            Color = color;
            Shape = shape;
        }

        public override string OutputText => PrefixText + " " + (int)Size + " " + Color.GetAttributeDescription() + " " + Shape.GetAttributeDescription() +
                                             (ThemeComponent != null ? " # " + ThemeComponent.ComponentName : string.Empty);

        public override string SummaryText => string.Empty;

        public ThemeComponent ThemeComponent
        {
            get { return _themeComponent; }
            set
            {
                if (_themeComponent == value) { return; }

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

        public IconSize Size
        {
            get { return _size; }
            set
            {
                _size = value;
                IsDirty = true;
                OnPropertyChanged();
            }
        }

        public IconColor Color
        {
            get { return _color; }
            set
            {
                _color = value;
                IsDirty = true;
                OnPropertyChanged();
            }
        }

        public IconShape Shape
        {
            get { return _shape; }
            set
            {
                _shape = value;
                IsDirty = true;
                OnPropertyChanged();
            }
        }

        private void OnThemeComponentUpdated(object sender, EventArgs e)
        {
            Size = ((IconThemeComponent)sender).IconSize;
            Color = ((IconThemeComponent)sender).IconColor;
            Shape = ((IconThemeComponent)sender).IconShape;
        }

        private void OnThemeComponentDeleted(object sender, EventArgs e)
        {
            ThemeComponent = null;
        }
    }
}
