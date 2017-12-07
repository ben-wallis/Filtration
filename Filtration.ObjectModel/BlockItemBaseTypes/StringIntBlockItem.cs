using System.Windows.Media;

namespace Filtration.ObjectModel.BlockItemBaseTypes
{
    public abstract class StrIntBlockItem : BlockItemBase, IAudioVisualBlockItem
    {
        private string _value;
        private int _secondValue;

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

        public override string OutputText => PrefixText + " " + Value + " " + SecondValue;

        public override string SummaryText => string.Empty;
        public override Color SummaryBackgroundColor => Colors.Transparent;
        public override Color SummaryTextColor => Colors.Transparent;

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
    }
}