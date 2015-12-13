using System.Windows.Media;

namespace Filtration.ObjectModel.BlockItemBaseTypes
{
    public abstract class IntegerBlockItem : BlockItemBase, IAudioVisualBlockItem
    {
        private int _value;

        protected IntegerBlockItem()
        {
        }

        protected IntegerBlockItem(int value)
        {
            Value = value;
        }

        public override string OutputText => PrefixText + " " + Value;

        public override string SummaryText => string.Empty;
        public override Color SummaryBackgroundColor => Colors.Transparent;
        public override Color SummaryTextColor => Colors.Transparent;

        public abstract int Minimum { get; }
        public abstract int Maximum { get; }

        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }
    }
}
