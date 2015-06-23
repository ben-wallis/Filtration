using System.Windows.Media;

namespace Filtration.Models.BlockItemBaseTypes
{
    internal abstract class DualIntegerBlockItem : BlockItemBase, IAudioVisualBlockItem
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

        public override string OutputText
        {
            get { return PrefixText + " " + Value + " " + SecondValue; }
        }
        
        public override string SummaryText { get { return string.Empty; } }
        public override Color SummaryBackgroundColor { get { return Colors.Transparent; } }
        public override Color SummaryTextColor { get { return Colors.Transparent; } }

        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public int SecondValue
        {
            get { return _secondValue; }
            set
            {
                _secondValue = value;
                OnPropertyChanged();
            }
        }
    }
}
