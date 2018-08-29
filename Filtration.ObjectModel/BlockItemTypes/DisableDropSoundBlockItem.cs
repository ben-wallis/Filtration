using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;

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
        public override int SortOrder => 28;

    }
}
