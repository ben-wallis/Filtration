using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class TextColorBlockItem : ColorBlockItem
    {
        public TextColorBlockItem()
        {
        }

        public TextColorBlockItem(Color color) : base(color)
        {
        }

        public override string PrefixText => "SetTextColor";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Text Color";
        public override int SortOrder => 14;
    }
}
