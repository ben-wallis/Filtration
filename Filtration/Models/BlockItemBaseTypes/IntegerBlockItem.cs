using System.Windows.Media;

namespace Filtration.Models.BlockItemBaseTypes
{
    internal abstract class IntegerBlockItem : BlockItemBase, IAudioVisualBlockItem
    {
        private int _value;

        protected IntegerBlockItem()
        {
        }

        protected IntegerBlockItem(int value)
        {
            Value = value;
        }

        public override string SummaryText { get { return string.Empty; } }
        public override Color SummaryBackgroundColor { get { return Colors.Transparent; } }
        public override Color SummaryTextColor { get { return Colors.Transparent; } }

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
