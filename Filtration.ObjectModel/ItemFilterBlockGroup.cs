using System;
using System.Collections.Generic;

namespace Filtration.ObjectModel
{
    public class ItemFilterBlockGroup
    {
        private bool _isChecked;

        public ItemFilterBlockGroup(string groupName, ItemFilterBlockGroup parent, bool advanced = false)
        {
            GroupName = groupName;
            ParentGroup = parent;
            Advanced = advanced;
            ChildGroups = new List<ItemFilterBlockGroup>();
        }

        public event EventHandler BlockGroupStatusChanged;

        public string GroupName { get; }
        public ItemFilterBlockGroup ParentGroup { get; }
        public List<ItemFilterBlockGroup> ChildGroups { get; }
        public bool Advanced { get; }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value != _isChecked)
                {
                    _isChecked = value;
                    // Raise an event to let blocks that have this block group assigned that
                    // they might need to change their Action due to the block group status changing.
                    BlockGroupStatusChanged?.Invoke(null, null);
                }
            }
        }

        public override string ToString()
        {
            var currentBlockGroup = this;

            var outputString = (Advanced ? "~" : string.Empty) + GroupName;

            // TODO: This is retarded, fix this.
            if (currentBlockGroup.ParentGroup != null)
            {
                while (currentBlockGroup.ParentGroup.ParentGroup != null)
                {
                    outputString = (currentBlockGroup.ParentGroup.Advanced ? "~" : string.Empty) + currentBlockGroup.ParentGroup.GroupName + " - " + outputString;
                    currentBlockGroup = currentBlockGroup.ParentGroup;
                }
            }

            return outputString;
        }
    }
}
