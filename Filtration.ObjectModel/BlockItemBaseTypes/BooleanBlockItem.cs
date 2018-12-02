namespace Filtration.ObjectModel.BlockItemBaseTypes
{
    public abstract class BooleanBlockItem : BlockItemBase
    {
        private bool _booleanValue;

        protected BooleanBlockItem()
        {
            
        }

        protected BooleanBlockItem(bool booleanValue)
        {
            BooleanValue = booleanValue;
        }

        public bool BooleanValue
        {
            get { return _booleanValue; }
            set
            {
                _booleanValue = value;
                IsDirty = true;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SummaryText));
            }
        }

        public override string OutputText => PrefixText + " " + BooleanValue;
        public override string SummaryText => DisplayHeading + " = " + BooleanValue;
        public override int MaximumAllowed => 1;

        public void ToggleValue()
        {
            BooleanValue = !BooleanValue;
        }
    }
}