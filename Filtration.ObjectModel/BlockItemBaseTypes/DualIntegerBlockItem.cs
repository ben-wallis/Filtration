using System.Windows.Media;

namespace Filtration.ObjectModel.BlockItemBaseTypes
{
    public abstract class DualIntegerBlockItem : BlockItemBase, IAudioVisualBlockItem
    {
        private int _value;
        private int _secondValue;

        protected DualIntegerBlockItem()
        {
        }

        protected DualIntegerBlockItem(int value, int secondValue)
        {
            Value = value;
            SecondValue = secondValue;
        }

        public override string OutputText => PrefixText + " " + Value + " " + SecondValue;

        public override string SummaryText => string.Empty;
        public override Color SummaryBackgroundColor => Colors.Transparent;
        public override Color SummaryTextColor => Colors.Transparent;

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
