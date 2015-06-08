using System.Collections.Generic;

namespace Filtration.Models
{
    internal class ItemFilterBlockGroup
    {
        public ItemFilterBlockGroup(string groupName, ItemFilterBlockGroup parent)
        {
            GroupName = groupName;
            ParentGroup = parent;
            ChildGroups = new List<ItemFilterBlockGroup>();
        }

        public override string ToString()
        {
            var currentBlockGroup = this;
            
            var outputString = GroupName;
            while (currentBlockGroup.ParentGroup.ParentGroup != null)
            {
                outputString = currentBlockGroup.ParentGroup.GroupName + " - " + outputString;
                currentBlockGroup = currentBlockGroup.ParentGroup;
            }

            return outputString;
        }

        public string GroupName { get; private set; }
        public ItemFilterBlockGroup ParentGroup { get; private set; }
        public List<ItemFilterBlockGroup> ChildGroups { get; private set; } 
    }
}
