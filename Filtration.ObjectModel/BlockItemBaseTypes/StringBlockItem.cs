using System.Windows.Media;

namespace Filtration.ObjectModel.BlockItemBaseTypes
{
    public abstract class StringBlockItem : BlockItemBase, IAudioVisualBlockItem
    {
        private string _value;

        protected StringBlockItem()
        {
        }

        protected StringBlockItem(string value)
        {
            Value = value;
        }

        public override string OutputText => PrefixText + " " + Value;

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
    }
}