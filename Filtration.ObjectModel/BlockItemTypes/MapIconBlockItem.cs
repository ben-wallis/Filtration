using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class MapIconBlockItem : IconBlockItem
    {
        public MapIconBlockItem()
        {
            Size = IconSize.Largest;
            Color = IconColor.Red;
            Shape = IconShape.Circle;
        }

        public MapIconBlockItem(IconSize size, IconColor iconColor, IconShape iconShape) : base(size, iconColor, iconShape)
        {
        }

        public override string PrefixText => "MinimapIcon";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Minimap Icon";
        public override BlockItemOrdering SortOrder => BlockItemOrdering.MinimapIcon;
    }
}
