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

        public string GroupName { get; private set; }
        public ItemFilterBlockGroup ParentGroup { get; private set; }
        public List<ItemFilterBlockGroup> ChildGroups { get; private set; }
        public bool Advanced { get; private set; }

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
                    if (BlockGroupStatusChanged != null)
                    {
                        BlockGroupStatusChanged.Invoke(null, null);
                    }
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
