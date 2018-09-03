using Filtration.ObjectModel.BlockItemBaseTypes;
using System;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class FontSizeBlockItem : IntegerBlockItem
    {
        public FontSizeBlockItem()
        {
            Value = 35;
        }

        public FontSizeBlockItem(int value) : base(value)
        {
        }

        public override string PrefixText => "SetFontSize";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Font Size";
        public override int SortOrder => 25;
        public override int Minimum => 11;
        public override int Maximum => 45;

        public override string OutputText => (ThemeComponent != null ? "# " + ThemeComponent.ComponentName + Environment.NewLine : string.Empty) +
                                             PrefixText + " " + Value;
    }
}
