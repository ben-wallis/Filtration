using System;
using System.Collections.ObjectModel;
using System.Linq;
using Filtration.Enums;
using Filtration.Models.BlockItemBaseTypes;

namespace Filtration.Models
{
    internal class ItemFilterBlock
    {
        public ItemFilterBlock()
        {
            BlockItems = new ObservableCollection<IItemFilterBlockItem> {new ActionBlockItem(BlockAction.Show)};
        }
        
        public string Description { get; set; }
        public ItemFilterBlockGroup BlockGroup { get; set; }

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

        public ObservableCollection<IItemFilterBlockItem> BlockItems { get; private set; }

        public int BlockCount(Type type)
        {
            return BlockItems != null ? BlockItems.Count(b => b.GetType() == type) : 0;
        }

        public bool AddBlockItemAllowed(Type type)
        {
            var blockItem = (IItemFilterBlockItem)Activator.CreateInstance(type);
            return BlockCount(type) < blockItem.MaximumAllowed;
        }

        public bool HasBlockItemOfType<T>()
        {
            return BlockItems.Count(b => b is T) > 0; 
        }
    }
}
