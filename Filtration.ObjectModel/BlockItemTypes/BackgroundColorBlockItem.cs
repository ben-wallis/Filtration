using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class BackgroundColorBlockItem : ColorBlockItem
    {
        public BackgroundColorBlockItem()
        {
        }

        public BackgroundColorBlockItem(Color color) : base(color)
        {
        }

        public override string PrefixText => "SetBackgroundColor";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Background Color";
        public override int SortOrder => 21;
    }
}
