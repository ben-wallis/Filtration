using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Extensions;
using System;

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
        public override int SortOrder => 30;

        public override string OutputText => (ThemeComponent != null ? "# " + ThemeComponent.ComponentName + Environment.NewLine : string.Empty) +
                                             PrefixText + " " + Color.GetAttributeDescription() +
                                             (Temporary ? " " + "Temp" : string.Empty);
    }
}
