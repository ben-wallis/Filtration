using System;
using System.Windows.Media;
using Filtration.ObjectModel.ThemeEditor;

namespace Filtration.ObjectModel.BlockItemBaseTypes
{
    public abstract class ColorBooleanBlockItem : BlockItemBase, IAudioVisualBlockItem
    {
        private Color _color;
        private bool _booleanValue;

        protected ColorBooleanBlockItem()
        {
        }

        protected ColorBooleanBlockItem(Color color, bool booleanValue)
        {
            Color = color;
            BooleanValue = booleanValue;
        }

        public override string OutputText => PrefixText + " " + +Color.R + " " + Color.G + " "
                                             + Color.B + (Color.A < 255 ? " " + Color.A : string.Empty) +
                                             (BooleanValue ? " True" : " False");

        public override string SummaryText => string.Empty;

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

        public bool BooleanValue
        {
            get { return _booleanValue; }
            set
            {
                _booleanValue = value;
                IsDirty = true;
                OnPropertyChanged();
            }
        }
    }
}
