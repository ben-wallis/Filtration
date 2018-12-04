using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class FontSizeBlockItem : IntegerBlockItem
    {
        public FontSizeBlockItem()
        {
            Value = 35;
        }

        public FontSizeBlockItem(int value) : base(value)
        {
        }

        public override string PrefixText => "SetFontSize";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Font Size";
        public override BlockItemOrdering SortOrder => BlockItemOrdering.SetFontSize;
        public override int Minimum => 11;
        public override int Maximum => 45;
    }
}
