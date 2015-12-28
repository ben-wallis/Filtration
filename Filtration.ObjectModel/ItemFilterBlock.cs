using System;
using System.Collections.ObjectModel;
using System.Linq;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel
{
    public interface IItemFilterBlock
    {
        bool Enabled { get; set; }
        string Description { get; set; }
        ItemFilterBlockGroup BlockGroup { get; set; }
        BlockAction Action { get; set; }
        ObservableCollection<IItemFilterBlockItem> BlockItems { get; }
        int BlockCount(Type type);
        bool AddBlockItemAllowed(Type type);
        bool HasBlockItemOfType<T>();
        bool HasBlockGroupInParentHierarchy(ItemFilterBlockGroup targetBlockGroup, ItemFilterBlockGroup startingBlockGroup);
    }

    public class ItemFilterBlock : IItemFilterBlock
    {
        private ItemFilterBlockGroup _blockGroup;

        public ItemFilterBlock()
        {
            BlockItems = new ObservableCollection<IItemFilterBlockItem> {new ActionBlockItem(BlockAction.Show)};
            Enabled = true;
        }

        public bool Enabled { get; set; }
        public string Description { get; set; }

        public ItemFilterBlockGroup BlockGroup
        {
            get { return _blockGroup; }
            set
            {
                var oldBlockGroup = _blockGroup;
                _blockGroup = value;

                if (_blockGroup != null)
                {
                    _blockGroup.BlockGroupStatusChanged += OnBlockGroupStatusChanged;
                    if (oldBlockGroup != null)
                    {
                        oldBlockGroup.BlockGroupStatusChanged -= OnBlockGroupStatusChanged;
                    }
                }
                else
                {
                    if (oldBlockGroup != null)
                    {
                        oldBlockGroup.BlockGroupStatusChanged -= OnBlockGroupStatusChanged;
                    }
                }
            }
        }

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

        public ObservableCollection<IItemFilterBlockItem> BlockItems { get; }

        public int BlockCount(Type type)
        {
            return BlockItems?.Count(b => b.GetType() == type) ?? 0;
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

        public bool HasBlockGroupInParentHierarchy(ItemFilterBlockGroup targetBlockGroup, ItemFilterBlockGroup startingBlockGroup)
        {
            if (startingBlockGroup == targetBlockGroup)
            {
                return true;
            }
            if (BlockGroup == null)
            {
                return false;
            }

            return startingBlockGroup.ParentGroup != null && HasBlockGroupInParentHierarchy(targetBlockGroup, startingBlockGroup.ParentGroup);
        }
        
        private void OnBlockGroupStatusChanged(object sender, EventArgs e)
        {
            if (BlockGroup.IsChecked == false && Action == BlockAction.Show)
            {
                Action = BlockAction.Hide;
            }
            else if (BlockGroup.IsChecked && Action == BlockAction.Hide)
            {
                Action = BlockAction.Show;
            }
        }
    }
}
