using Filtration.Models.BlockItemBaseTypes;

namespace Filtration.Models.BlockItemTypes
{
    internal class FontSizeBlockItem : IntegerBlockItem
    {
        public FontSizeBlockItem()
        {
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
