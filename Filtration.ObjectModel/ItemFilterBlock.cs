using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Filtration.ObjectModel.Annotations;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Extensions;


namespace Filtration.ObjectModel
{
    public interface IItemFilterBlock
    {
        bool Enabled { get; set; }
        // Rather than toggling between Hide/Show when a group is toggled, Disable/Enable
        bool DisableWithGroup { get; set; }
        string Description { get; set; }
        ItemFilterBlockGroup BlockGroup { get; set; }
        BlockAction Action { get; set; }
        ObservableCollection<IItemFilterBlockItem> BlockItems { get; }
        Color DisplayBackgroundColor { get; }
        Color DisplayTextColor { get; }
        Color DisplayBorderColor { get; }
        double DisplayFontSize { get; }
        int BlockCount(Type type);
        bool AddBlockItemAllowed(Type type);
        bool HasBlockItemOfType<T>();
        bool HasBlockGroupInParentHierarchy(ItemFilterBlockGroup targetBlockGroup, ItemFilterBlockGroup startingBlockGroup);
    }

    public class ItemFilterBlock : IItemFilterBlock, INotifyPropertyChanged
    {
        protected bool _Enabled;

        private ItemFilterBlockGroup _blockGroup;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ItemFilterBlock()
        {
            BlockItems = new ObservableCollection<IItemFilterBlockItem> {new ActionBlockItem(BlockAction.Show)};
            DisableWithGroup = false;
            _Enabled = true;
        }

        public bool Enabled
        {
            get { return _Enabled; }
            set
            {
                if (_Enabled != value)
                {
                    _Enabled = value;
                    OnPropertyChanged(nameof(Enabled));
                }
            }
        }
            
        public string Description { get; set; }
        public bool DisableWithGroup { get; set; }

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
            if (DisableWithGroup)
            {
                if (BlockGroup.IsChecked != Enabled)
                {
                    Enabled = BlockGroup.IsChecked;
                }
            }
            else
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
