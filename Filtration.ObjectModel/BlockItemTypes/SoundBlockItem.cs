using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class SoundBlockItem : DualIntegerBlockItem
    {
        public SoundBlockItem()
        {
            Value = 1;
            SecondValue = 79;
        }

        public SoundBlockItem(int value, int secondValue) : base(value, secondValue)
        {
        }

        public override string PrefixText
        {
            get { return "PlayAlertSound"; }
        }

        public override int MaximumAllowed
        {
            get { return 1; }
        }

        public override string DisplayHeading
        {
            get
            {
                return "Play Alert Sound";
            }
        }

        public override int SortOrder
        {
            get { return 16; }
        }
    }
}
