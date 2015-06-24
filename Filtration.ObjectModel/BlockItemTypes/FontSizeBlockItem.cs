using Filtration.ObjectModel.BlockItemBaseTypes;

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

        public override string PrefixText
        {
            get { return "SetFontSize"; }
        }

        public override int MaximumAllowed
        {
            get { return 1; }
        }

        public override string DisplayHeading
        {
            get
            {
                return "Font Size";
            }
        }

        public override int SortOrder
        {
            get { return 15; }
        }

        public override int Minimum
        {
            get
            {
                return 11;
            }
        }

        public override int Maximum
        {
            get
            {
                return 45;
            }
        }
    }
}
