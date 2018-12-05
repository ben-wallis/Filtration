using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class PlayEffectBlockItem : EffectColorBlockItem
    {
        public PlayEffectBlockItem()
        {
            Color = EffectColor.Red;
            Temporary = false;
        }

        public PlayEffectBlockItem(EffectColor effectColor, bool temporary) : base(effectColor, temporary)
        {
        }

        public override string PrefixText => "PlayEffect";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Play Effect";
        public override BlockItemOrdering SortOrder => BlockItemOrdering.PlayEffect;
    }
}
