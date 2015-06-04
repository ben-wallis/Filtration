using System.Windows.Media;
using Filtration.Models.BlockItemBaseTypes;

namespace Filtration.Models.BlockItemTypes
{
    internal class BorderColorBlockItem : ColorBlockItem
    {
        public BorderColorBlockItem()
        {
        }

        public BorderColorBlockItem(Color color) : base(color)
        {
        }

        public override string PrefixText
        {
            get { return "SetBorderColor"; }
        }

        public override int MaximumAllowed
        {
            get { return 1; }
        }

        public override string DisplayHeading
        {
            get
            {
                return "Border Color";
            }
        }

        public override int SortOrder
        {
            get { return 14; }
        }
    }
}
