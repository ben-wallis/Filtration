using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Filtration.Annotations;

namespace Filtration.Models.BlockItemBaseTypes
{
    internal abstract class DualIntegerBlockItem : ILootFilterBlockItem, IAudioVisualBlockItem
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

        public abstract string PrefixText { get; }
        public abstract int MaximumAllowed { get; }
        public abstract string DisplayHeading { get; }

        public string SummaryText { get { return string.Empty; } }
        public Color SummaryBackgroundColor { get { return Colors.Transparent; } }
        public Color SummaryTextColor { get { return Colors.Transparent; } }
        public abstract int SortOrder { get; }

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

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
