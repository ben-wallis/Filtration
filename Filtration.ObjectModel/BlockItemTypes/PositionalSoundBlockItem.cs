using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class PositionalSoundBlockItem : StrIntBlockItem
    {
        public PositionalSoundBlockItem()
        {
            Value = "1";
            SecondValue = 79;
        }

        public PositionalSoundBlockItem(string value, int secondValue) : base(value, secondValue)
        {
        }

        public override string PrefixText => "PlayAlertSoundPositional";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Play Positional Alert Sound";
        public override BlockItemOrdering SortOrder => BlockItemOrdering.PlayAlertSoundPositional;
    }
}
