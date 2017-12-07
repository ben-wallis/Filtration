using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Commands;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Extensions;

namespace Filtration.ObjectModel
{
    public interface IItemFilterBlock : IItemFilterBlockBase
    {
        bool Enabled { get; set; }
        string Description { get; set; }
        ItemFilterBlockGroup BlockGroup { get; set; }
        BlockAction Action { get; set; }
        ActionBlockItem ActionBlockItem { get; }
        ObservableCollection<IItemFilterBlockItem> BlockItems { get; }
        Color DisplayBackgroundColor { get; }
        Color DisplayTextColor { get; }
        Color DisplayBorderColor { get; }
        double DisplayFontSize { get; }
        bool HasBlockItemOfType<T>();
        bool HasBlockGroupInParentHierarchy(ItemFilterBlockGroup targetBlockGroup, ItemFilterBlockGroup startingBlockGroup);
    }

    public interface IItemFilterBlockBase
    {
    }

    public abstract class ItemFilterBlockBase : IItemFilterBlockBase
    {
        protected ItemFilterBlockBase()
        {
        }

        protected ItemFilterBlockBase(IItemFilterScript parentScript)
        {
            CommandManager = parentScript.CommandManager;
            ParentScript = parentScript;
        }

        public ICommandManager CommandManager { get; }
        public IItemFilterScript ParentScript { get; set; }
    }

    public interface IItemFilterCommentBlock : IItemFilterBlockBase
    {
        string Comment { get; set; }
    }

    public class ItemFilterCommentBlock : ItemFilterBlockBase, IItemFilterCommentBlock
    {
        public ItemFilterCommentBlock(IItemFilterScript parentScript) : base(parentScript)
        {
        }

        public string Comment { get; set; }
    }

    public class ItemFilterBlock : ItemFilterBlockBase, IItemFilterBlock
    {
        private ItemFilterBlockGroup _blockGroup;

        internal ItemFilterBlock()
        {
            BlockItems = new ObservableCollection<IItemFilterBlockItem> { ActionBlockItem };
        }

        public ItemFilterBlock(IItemFilterScript parentScript) : base(parentScript)
        {
            BlockItems = new ObservableCollection<IItemFilterBlockItem> { ActionBlockItem };
        }

        public ICommandManager CommandManager { get; }

        public bool Enabled { get; set; } = true;
        public string Description { get; set; }

        public ItemFilterBlockGroup BlockGroup
        {
            get => _blockGroup;
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

        public ActionBlockItem ActionBlockItem { get; } = new ActionBlockItem(BlockAction.Show);

        public ObservableCollection<IItemFilterBlockItem> BlockItems { get; }

        public bool AddBlockItemAllowed(Type type)
        {
            int BlockCount()
            {
                return BlockItems?.Count(b => b.GetType() == type) ?? 0;
            }

            var blockItem = (IItemFilterBlockItem)Activator.CreateInstance(type);
            return BlockCount() < blockItem.MaximumAllowed;
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

        public Color DisplayTextColor
        {
            get
            {
                var textColorBlockItem = BlockItems.OfType<TextColorBlockItem>().FirstOrDefault();
                if (textColorBlockItem != null)
                {
                    return textColorBlockItem.Color;
                }

                var itemClassBlockItem = BlockItems.OfType<ClassBlockItem>().FirstOrDefault();
                if (itemClassBlockItem != null)
                {
                    if (itemClassBlockItem.Items.All(i => i.Contains("Gems")))
                    {
                        return PathOfExileNamedColors.Colors[PathOfExileNamedColor.GemItem];
                    }
                    if (itemClassBlockItem.Items.All(i => i.Contains("Quest")))
                    {
                        return PathOfExileNamedColors.Colors[PathOfExileNamedColor.QuestItem];
                    }
                }
                
                var rarityBlockItem = BlockItems.OfType<RarityBlockItem>().FirstOrDefault();
                return rarityBlockItem != null
                    ? ((ItemRarity) rarityBlockItem.FilterPredicate.PredicateOperand).DefaultRarityTextColor()
                    : PathOfExileNamedColors.Colors[PathOfExileNamedColor.WhiteItem];
            }
        }

        public Color DisplayBackgroundColor
        {
            get
            {
                var backgroundColorBlockItem = BlockItems.OfType<BackgroundColorBlockItem>().FirstOrDefault();
                return backgroundColorBlockItem?.Color ?? new Color { A = 255, R = 0, G = 0, B = 0 };
            }
        }

        public Color DisplayBorderColor
        {
            get
            {
                var borderColorBlockItem = BlockItems.OfType<BorderColorBlockItem>().FirstOrDefault();
                return borderColorBlockItem?.Color ?? new Color { A = 255, R = 0, G = 0, B = 0 };
            }
        }

        public double DisplayFontSize
        {
            get
            {
                var fontSizeBlockItem = BlockItems.OfType<FontSizeBlockItem>().FirstOrDefault();
                return fontSizeBlockItem?.Value ?? 34;
            }
        }
    }
}
