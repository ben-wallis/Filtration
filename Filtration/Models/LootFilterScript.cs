using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Filtration.Models
{
    internal class LootFilterScript
    {
        public LootFilterScript()
        {
            LootFilterBlocks = new ObservableCollection<LootFilterBlock>();
        }

        public ObservableCollection<LootFilterBlock> LootFilterBlocks { get; set; }
        public string FilePath { get; set; }
        public string Description { get; set; }
        public DateTime DateModified { get; set; }

        public List<string> Validate()
        {
            var validationErrors = new List<string>();

            if (LootFilterBlocks.Count == 0)
            {
                validationErrors.Add("A script must have at least one block");
            }

            return validationErrors;
        }
    }
}
