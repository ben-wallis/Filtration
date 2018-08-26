using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class IconBlockItem : StringBlockItem
    {
        public IconBlockItem()
        {
            Value = "Icon1";
        }

        public IconBlockItem(string value) : base(value)
        {
        }

        public override string PrefixText => "Icon";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Drop Icon";
        public override int SortOrder => 28;
    }
}
