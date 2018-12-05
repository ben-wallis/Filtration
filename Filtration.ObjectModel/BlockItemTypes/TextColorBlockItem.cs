using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class TextColorBlockItem : ColorBlockItem
    {
        public TextColorBlockItem()
        {
            Color = PathOfExileNamedColors.Colors[PathOfExileNamedColor.WhiteItem];
        }

        public TextColorBlockItem(Color color) : base(color)
        {
        }

        public override string PrefixText => "SetTextColor";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Text Color";
        public override BlockItemOrdering SortOrder => BlockItemOrdering.SetTextColor;
    }
}
