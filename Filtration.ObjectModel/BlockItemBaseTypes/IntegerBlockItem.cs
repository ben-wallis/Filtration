using System;
using System.Windows.Media;
using Filtration.ObjectModel.ThemeEditor;

namespace Filtration.ObjectModel.BlockItemBaseTypes
{
    public abstract class IntegerBlockItem : BlockItemBase, IAudioVisualBlockItem, IBlockItemWithTheme
    {
        private int _value;
        private ThemeComponent _themeComponent;

        protected IntegerBlockItem()
        {
        }

        protected IntegerBlockItem(int value)
        {
            Value = value;
        }

        public override string OutputText => PrefixText + " " + Value + (ThemeComponent != null ? " # " + ThemeComponent.ComponentName : string.Empty);

        public override string SummaryText => string.Empty;
        public override Color SummaryBackgroundColor => Colors.Transparent;
        public override Color SummaryTextColor => Colors.Transparent;

        public abstract int Minimum { get; }
        public abstract int Maximum { get; }

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

        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                IsDirty = true;
                OnPropertyChanged();
            }
        }

        private void OnThemeComponentUpdated(object sender, EventArgs e)
        {
            Value = ((IntegerBlockItem)sender).Value;
        }

        private void OnThemeComponentDeleted(object sender, EventArgs e)
        {
            ThemeComponent = null;
        }
    }
}
