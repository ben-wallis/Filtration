﻿using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public class SoundBlockItem : StrIntBlockItem
    {
        public SoundBlockItem()
        {
            Value = "1";
            SecondValue = 79;
        }

        public SoundBlockItem(string value, int secondValue) : base(value, secondValue)
        {
        }

        public override string PrefixText => "PlayAlertSound";
        public override int MaximumAllowed => 1;
        public override string DisplayHeading => "Play Alert Sound";
        public override int SortOrder => 25;
    }
}
