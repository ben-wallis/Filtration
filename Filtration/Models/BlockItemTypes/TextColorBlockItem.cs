using System.Windows.Media;
using Filtration.Models.BlockItemBaseTypes;

namespace Filtration.Models.BlockItemTypes
{
    internal class TextColorBlockItem : ColorBlockItem
    {
        public TextColorBlockItem()
        {
        }

        public TextColorBlockItem(Color color) : base(color)
        {
        }

        public override string PrefixText
        {
            get { return "SetTextColor"; }
        }

        public override int MaximumAllowed
        {
            get { return 1; }
        }

        public override string DisplayHeading
        {
            get
            {
                return "Text Color";
            }
        }

        public override int SortOrder
        {
            get { return 12; }
        }
    }
}
