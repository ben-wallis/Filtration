using System;
using System.Collections.Generic;

namespace Filtration.ObjectModel
{
    public class ItemFilterBlockGroup
    {
        private bool? _isShowChecked;
        private bool? _isEnableChecked;

        public ItemFilterBlockGroup(string groupName, ItemFilterBlockGroup parent, bool advanced = false, bool isLeafNode = false)
        {
            GroupName = groupName;
            ParentGroup = parent;
            Advanced = advanced;
            ChildGroups = new List<ItemFilterBlockGroup>();
            IsLeafNode = isLeafNode;
        }

        public string GroupName { get; }
        public ItemFilterBlockGroup ParentGroup { get; set; }
        public List<ItemFilterBlockGroup> ChildGroups { get; }
        public bool Advanced { get; }
        public bool IsLeafNode { get; }

        public event EventHandler BlockGroupStatusChanged;

        public bool? IsShowChecked
        {
            get { return _isShowChecked; }
            set
            {
                if (value != _isShowChecked)
                {
                    _isShowChecked = value;
                    // Raise an event to let blocks that have this block group assigned that
                    // they might need to change their Action due to the block group status changing.
                    BlockGroupStatusChanged?.Invoke(null, null);
                }
            }
        }

        public bool? IsEnableChecked
        {
            get { return _isEnableChecked; }
            set
            {
                if (value != _isEnableChecked)
                {
                    _isEnableChecked = value;
                    // Raise an event to let blocks that have this block group assigned that
                    // they might need to change their Enabled due to the block group status changing.
                    BlockGroupStatusChanged?.Invoke(null, null);
                }
            }
        }

        public void ClearStatusChangeSubscribers()
        {
            BlockGroupStatusChanged = null;
        }

        public void AddOrJoinBlockGroup(ItemFilterBlockGroup blockGroup)
        {
            var childIndex = ChildGroups.FindIndex(item => item.GroupName.Equals(blockGroup.GroupName));
            if (!blockGroup.IsLeafNode && childIndex >= 0)
            {
                while(blockGroup.ChildGroups.Count > 0)
                {
                    ChildGroups[childIndex].AddOrJoinBlockGroup(blockGroup.ChildGroups[0]);
                }
            }
            else
            {
                if(blockGroup.ParentGroup != null)
                {
                    blockGroup.ParentGroup.ChildGroups.Remove(blockGroup);
                }
                blockGroup.ParentGroup = this;
                ChildGroups.Add(blockGroup);
            }
        }

        public void DetachSelf(bool keepChildren)
        {
            if(ParentGroup == null)
                return;

            if(IsLeafNode && ParentGroup.ParentGroup != null && ParentGroup.ChildGroups.Count < 2)
            {
                ParentGroup.DetachSelf(false);
            }
            else
            {
                ParentGroup.ChildGroups.Remove(this);

                if (keepChildren)
                {
                    foreach(var child in ChildGroups)
                    {
                        ParentGroup.AddOrJoinBlockGroup(child);
                    }
                }

                ParentGroup = null;
            }
        }

        public override string ToString()
        {
            if(ParentGroup == null)
            {
                return string.Empty;
            }

            var outputString = (Advanced ? "~" : string.Empty) + GroupName;

            var parentOutput = ParentGroup.ToString();
            if(!string.IsNullOrWhiteSpace(parentOutput))
            {
                outputString = parentOutput + (IsLeafNode ? string.Empty : " - " + outputString);
            }

            return outputString;
        }
    }
}
