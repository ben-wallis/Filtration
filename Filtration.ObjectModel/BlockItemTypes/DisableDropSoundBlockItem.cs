using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public sealed class DisableDropSoundBlockItem : BooleanBlockItem, IAudioVisualBlockItem
    {
        public DisableDropSoundBlockItem()
        {
        }

        public DisableDropSoundBlockItem(bool booleanValue) : base(booleanValue)
        {
        }

        public override string PrefixText => "DisableDropSound";
        public override string DisplayHeading => "Disable Drop Sound";
        public override Color SummaryBackgroundColor => Colors.Transparent;
        public override Color SummaryTextColor => Colors.Transparent;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.DisableDropSound;
    }
}
