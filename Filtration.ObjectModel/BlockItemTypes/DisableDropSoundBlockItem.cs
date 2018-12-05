using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public sealed class DisableDropSoundBlockItem : NilBlockItem, IAudioVisualBlockItem
    {
        public DisableDropSoundBlockItem() : base()
        {
        }

        public override string PrefixText => "DisableDropSound";
        public override string DisplayHeading => "Disable Drop Sound";
        public override string Description => "Default drop sound disabled.";
        public override Color SummaryBackgroundColor => Colors.Transparent;
        public override Color SummaryTextColor => Colors.Transparent;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.DisableDropSound;
    }
}
