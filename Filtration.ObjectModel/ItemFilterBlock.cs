using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        event EventHandler EnabledStatusChanged;
        string Description { get; set; }
        ItemFilterBlockGroup BlockGroup { get; set; }
        BlockAction Action { get; set; }
        ActionBlockItem ActionBlockItem { get; }
        ObservableCollection<IItemFilterBlockItem> BlockItems { get; }
        Color DisplayBackgroundColor { get; }
        Color DisplayTextColor { get; }
        Color DisplayBorderColor { get; }
        double DisplayFontSize { get; }
        int DisplayIconSize { get; }
        int DisplayIconColor { get; }
        int DisplayIconShape { get; }
        Color DisplayEffectColor { get; }
        bool HasBlockItemOfType<T>();
        bool HasBlockGroupInParentHierarchy(ItemFilterBlockGroup targetBlockGroup, ItemFilterBlockGroup startingBlockGroup);
    }

    public interface IItemFilterBlockBase
    {
        bool IsEdited { get; set; }
        string OriginalText { get; set; } 
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
        public bool IsEdited { get; set; }
        public string OriginalText { get; set; }
    }

    public interface IItemFilterCommentBlock : IItemFilterBlockBase
    {
        string Comment { get; set; }
    }

    public class ItemFilterCommentBlock : ItemFilterBlockBase, IItemFilterCommentBlock
    {
        private string _comment;
        public ItemFilterCommentBlock(IItemFilterScript parentScript) : base(parentScript)
        {
        }

        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
                IsEdited = true;
            }
        }
    }

    public class ItemFilterBlock : ItemFilterBlockBase, IItemFilterBlock
    {
        private ItemFilterBlockGroup _blockGroup;
        private bool _enabled;
        private string _description;

        internal ItemFilterBlock()
        {
            BlockItems = new ObservableCollection<IItemFilterBlockItem> { ActionBlockItem };
            BlockItems.CollectionChanged += new NotifyCollectionChangedEventHandler(OnBlockItemsChanged);
            ActionBlockItem.PropertyChanged += OnActionBlockItemChanged;
            _enabled = true;
        }

        public ItemFilterBlock(IItemFilterScript parentScript) : base(parentScript)
        {
            BlockItems = new ObservableCollection<IItemFilterBlockItem> { ActionBlockItem };
            BlockItems.CollectionChanged += new NotifyCollectionChangedEventHandler(OnBlockItemsChanged);
            ActionBlockItem.PropertyChanged += OnActionBlockItemChanged;
            _enabled = true;
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                IsEdited = true;
                EnabledStatusChanged?.Invoke(null, null);
                if(BlockGroup != null && BlockGroup.IsEnableChecked != value)
                {
                    BlockGroup.IsEnableChecked = value;
                }
            }
        }

        public event EventHandler EnabledStatusChanged;

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                IsEdited = true;
            }
        }

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
                IsEdited = true;
            }
        }

        public ActionBlockItem ActionBlockItem { get; } = new ActionBlockItem(BlockAction.Show);

        public ObservableCollection<IItemFilterBlockItem> BlockItems { get; }
        private void OnBlockItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsEdited = true;
        }
        private void OnActionBlockItemChanged(object sender, EventArgs e)
        {
            if (BlockGroup != null && BlockGroup.IsShowChecked != (Action == BlockAction.Show))
            {
                BlockGroup.IsShowChecked = (Action == BlockAction.Show);
            }
        }

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
            if (BlockGroup.IsShowChecked == false && Action == BlockAction.Show)
            {
                Action = BlockAction.Hide;
            }
            else if (BlockGroup.IsShowChecked == true && Action == BlockAction.Hide)
            {
                Action = BlockAction.Show;
            }

            if (BlockGroup.IsEnableChecked == false && Enabled)
            {
                Enabled = false;
            }
            else if (BlockGroup.IsEnableChecked == true && !Enabled)
            {
                Enabled = true;
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
                return backgroundColorBlockItem?.Color ?? new Color { A = 240, R = 0, G = 0, B = 0 };
            }
        }

        public Color DisplayBorderColor
        {
            get
            {
                var borderColorBlockItem = BlockItems.OfType<BorderColorBlockItem>().FirstOrDefault();
                return borderColorBlockItem?.Color ?? new Color { A = 0, R = 255, G = 255, B = 255 };
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

        public int DisplayIconSize
        {
            get
            {
                var mapIconBlockItem = BlockItems.OfType<MapIconBlockItem>().FirstOrDefault();
                if (mapIconBlockItem != null)
                    return (int)mapIconBlockItem.Size;

                return -1;
            }
        }

        public int DisplayIconColor
        {
            get
            {
                var mapIconBlockItem = BlockItems.OfType<MapIconBlockItem>().FirstOrDefault();
                if (mapIconBlockItem != null)
                    return (int)mapIconBlockItem.Color;

                return -1;
            }
        }

        public int DisplayIconShape
        {
            get
            {
                var mapIconBlockItem = BlockItems.OfType<MapIconBlockItem>().FirstOrDefault();
                if (mapIconBlockItem != null)
                    return (int)mapIconBlockItem.Shape;

                return -1;
            }
        }

        public Color DisplayEffectColor
        {
            get
            {
                var beamBlockItem = BlockItems.OfType<PlayEffectBlockItem>().FirstOrDefault();

                if (beamBlockItem != null)
                {
                    switch (beamBlockItem.Color)
                    {
                        case EffectColor.Red:
                            return Colors.Red;
                        case EffectColor.Green:
                            return Colors.Green;
                        case EffectColor.Blue:
                            return Colors.Blue;
                        case EffectColor.Brown:
                            return Colors.Brown;
                        case EffectColor.White:
                            return Colors.White;
                        case EffectColor.Yellow:
                            return Colors.Yellow;
                    }
                }

                return Colors.Transparent;
            }
        }
    }
}
