using System;
using System.Windows.Media;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Extensions;
using Filtration.ObjectModel.ThemeEditor;

namespace Filtration.ObjectModel.BlockItemBaseTypes
{
    public abstract class EffectColorBlockItem : BlockItemBase, IAudioVisualBlockItem, IBlockItemWithTheme
    {
        private EffectColor _color;
        private bool _temporary;
        private ThemeComponent _themeComponent;

        protected EffectColorBlockItem()
        {
        }

        protected EffectColorBlockItem(EffectColor color, bool temporary)
        {
            Color = color;
            Temporary = temporary;
        }

        public override string OutputText => PrefixText + " " + Color.GetAttributeDescription() +
                                             (Temporary ? " " + "Temp" : string.Empty) +
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

        public EffectColor Color
        {
            get { return _color; }
            set
            {
                _color = value;
                IsDirty = true;
                OnPropertyChanged();
            }
        }

        public bool Temporary
        {
            get { return _temporary; }
            set
            {
                _temporary = value;
                IsDirty = true;
                OnPropertyChanged();
            }
        }

        private void OnThemeComponentUpdated(object sender, EventArgs e)
        {
            Color = ((EffectColorThemeComponent)sender).EffectColor;
            Temporary = ((EffectColorThemeComponent)sender).Temporary;
        }

        private void OnThemeComponentDeleted(object sender, EventArgs e)
        {
            ThemeComponent = null;
        }
    }
}
