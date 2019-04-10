using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public sealed class SynthesisedItemBlockItem : BooleanBlockItem
    {
        public SynthesisedItemBlockItem()
        {
        }

        public SynthesisedItemBlockItem(bool booleanValue) : base(booleanValue)
        {
        }

        public override string PrefixText => "SynthesisedItem";
        public override string DisplayHeading => "Synthesised Item";
        public override Color SummaryBackgroundColor => Colors.Salmon;
        public override Color SummaryTextColor => Colors.Black;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.SynthesisedItem;
    }
}
