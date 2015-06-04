using System;
using System.Collections.ObjectModel;
using System.Linq;
using Filtration.Enums;
using Filtration.Models.BlockItemBaseTypes;

namespace Filtration.Models
{
    internal class LootFilterBlock
    {
        public LootFilterBlock()
        {
            BlockItems = new ObservableCollection<ILootFilterBlockItem> {new ActionBlockItem(BlockAction.Show)};
        }
        
        public string Description { get; set; }

        public BlockAction Action
        {
            get
            {
                var actionBlock = BlockItems.OfType<ActionBlockItem>().First();
                return actionBlock.Action;
            }
            set
            {
                var actionBlock = BlockItems.OfType<ActionBlockItem>().First();
                actionBlock.Action = value;
            }
        }

        public ObservableCollection<ILootFilterBlockItem> BlockItems { get; private set; }

        public int BlockCount(Type type)
        {
            return BlockItems != null ? BlockItems.Count(b => b.GetType() == type) : 0;
        }

        public bool AddBlockItemAllowed(Type type)
        {
            var blockItem = (ILootFilterBlockItem)Activator.CreateInstance(type);
            return BlockCount(type) < blockItem.MaximumAllowed;
        }
    }
}
