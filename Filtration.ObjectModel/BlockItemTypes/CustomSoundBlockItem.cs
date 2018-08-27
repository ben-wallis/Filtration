using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class CustomSoundBlockItem : StringBlockItem
    {
        public CustomSoundBlockItem()
        {
            Value = "";
        }

        public CustomSoundBlockItem(string value) : base(value)
        {
        }

        public override string PrefixText => "CustomAlertSound";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Custom Alert Sound";
        public override int SortOrder => 30;
    }
}
