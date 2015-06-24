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

        public override string PrefixText
        {
            get { return "SetBackgroundColor"; }
        }

        public override int MaximumAllowed
        {
            get { return 1; }
        }

        public override string DisplayHeading
        {
            get
            {
                return "Background Color";
            }
        }

        public override int SortOrder
        {
            get { return 13; }
        }
    }
}
