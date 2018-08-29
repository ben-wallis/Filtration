using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class BorderColorBlockItem : ColorBlockItem
    {
        public BorderColorBlockItem()
        {
        }

        public BorderColorBlockItem(Color color) : base(color)
        {
        }

        public override string PrefixText => "SetBorderColor";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Border Color";
        public override int SortOrder => 24;
    }
}
