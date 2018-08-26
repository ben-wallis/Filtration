using System;
using System.Windows.Media;
using Filtration.ObjectModel.ThemeEditor;

namespace Filtration.ObjectModel.BlockItemBaseTypes
{
    public abstract class StrIntBlockItem : BlockItemBase, IAudioVisualBlockItem, IBlockItemWithTheme
    {
        private string _value;
        private int _secondValue;
        private ThemeComponent _themeComponent;

        protected StrIntBlockItem()
        {
        }

        protected StrIntBlockItem(string value, int secondValue)
        {
            Value = value;
            SecondValue = secondValue;
            Value = value;
            SecondValue = secondValue;
        }

        public override string OutputText => PrefixText + " " + Value + " " + SecondValue + (ThemeComponent != null ? " # " + ThemeComponent.ComponentName : string.Empty);

        public override string SummaryText => string.Empty;
        public override Color SummaryBackgroundColor => Colors.Transparent;
        public override Color SummaryTextColor => Colors.Transparent;

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

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                IsDirty = true;
                OnPropertyChanged();
            }
        }

        public int SecondValue
        {
            get { return _secondValue; }
            set
            {
                _secondValue = value;
                IsDirty = true;
                OnPropertyChanged();
            }
        }

        private void OnThemeComponentUpdated(object sender, EventArgs e)
        {
            Value = ((StrIntBlockItem)sender).Value;
            SecondValue = ((StrIntBlockItem)sender).SecondValue;
        }

        private void OnThemeComponentDeleted(object sender, EventArgs e)
        {
            ThemeComponent = null;
        }
    }
}