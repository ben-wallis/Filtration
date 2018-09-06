using System;
using System.Collections.Generic;
using System.Linq;
using Filtration.ObjectModel;
using Filtration.Parser.Interface.Services;

namespace Filtration.Parser.Services
{
    internal class BlockGroupHierarchyBuilder : IBlockGroupHierarchyBuilder
    {
        private ItemFilterBlockGroup _rootBlockGroup;

        public void Initialise(ItemFilterBlockGroup rootBlockGroup)
        {
            _rootBlockGroup = rootBlockGroup;
        }

        public void Cleanup()
        {
            _rootBlockGroup = null;
        }

        public ItemFilterBlockGroup IntegrateStringListIntoBlockGroupHierarchy(IEnumerable<string> groupStrings, bool show, bool enabled)
        {
            if (_rootBlockGroup == null)
            {
                throw new Exception("BlockGroupHierarchyBuilder must be initialised with root BlockGroup before use");
            }
            return IntegrateStringListIntoBlockGroupHierarchy(groupStrings, _rootBlockGroup, show, enabled);
        }

        public ItemFilterBlockGroup IntegrateStringListIntoBlockGroupHierarchy(IEnumerable<string> groupStrings, ItemFilterBlockGroup startItemGroup, bool show, bool enabled)
        {
            var inputGroups = groupStrings.ToList();
            var firstGroup = inputGroups.First().Trim();
            if (firstGroup.StartsWith("~"))
            {
                firstGroup = firstGroup.Substring(1);
            }

            ItemFilterBlockGroup matchingChildItemGroup = null;
            if (startItemGroup.ChildGroups.Count(g => g.GroupName == firstGroup) > 0)
            {
                matchingChildItemGroup = startItemGroup.ChildGroups.First(c => c.GroupName == firstGroup);
            }

            if (matchingChildItemGroup == null)
            {
                var newItemGroup = CreateBlockGroup(inputGroups.First().Trim(), startItemGroup);
                newItemGroup.IsShowChecked = show;
                newItemGroup.IsEnableChecked = enabled;
                startItemGroup.ChildGroups.Add(newItemGroup);
                inputGroups = inputGroups.Skip(1).ToList();
                if (inputGroups.Count > 0)
                {
                    return IntegrateStringListIntoBlockGroupHierarchy(inputGroups, newItemGroup, show, enabled);
                }
                else
                {
                    var leafNode = new ItemFilterBlockGroup("", newItemGroup, false, true);
                    leafNode.IsShowChecked = show;
                    leafNode.IsEnableChecked = enabled;
                    newItemGroup.ChildGroups.Add(leafNode);
                    return leafNode;
                }
            }
            else
            {
                if(matchingChildItemGroup.IsShowChecked != show)
                {
                    matchingChildItemGroup.IsShowChecked = null;
                }
                if (matchingChildItemGroup.IsEnableChecked != enabled)
                {
                    matchingChildItemGroup.IsEnableChecked = null;
                }
            }
            inputGroups = inputGroups.Skip(1).ToList();
            if(inputGroups.Count > 0)
            {
                return IntegrateStringListIntoBlockGroupHierarchy(inputGroups, matchingChildItemGroup, show, enabled);
            }
            else
            {
                var leafNode = new ItemFilterBlockGroup("", matchingChildItemGroup, false, true);
                leafNode.IsShowChecked = show;
                leafNode.IsEnableChecked = enabled;
                matchingChildItemGroup.ChildGroups.Add(leafNode);
                return leafNode;
            }
        }

        private ItemFilterBlockGroup CreateBlockGroup(string groupNameString, ItemFilterBlockGroup parentGroup)
        {
            var advanced = false;

            if (groupNameString.StartsWith("~"))
            {
                groupNameString = groupNameString.Substring(1);
                advanced = true;
            }

            if (parentGroup.Advanced)
            {
                advanced = true;
            }

            return new ItemFilterBlockGroup(groupNameString, parentGroup, advanced);
        }
    }
}
