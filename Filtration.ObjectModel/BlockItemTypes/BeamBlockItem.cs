using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class BeamBlockItem : ColorBooleanBlockItem
    {
        public BeamBlockItem()
        {
        }

        public BeamBlockItem(Color color, bool booleanValue) : base(color, booleanValue)
        {
        }

        public override string PrefixText => "BeamColor";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Beam Color";
        public override int SortOrder => 29;
    }
}
