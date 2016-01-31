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

        public ItemFilterBlockGroup IntegrateStringListIntoBlockGroupHierarchy(IEnumerable<string> groupStrings)
        {
            if (_rootBlockGroup == null)
            {
                throw new Exception("BlockGroupHierarchyBuilder must be initialised with root BlockGroup before use");
            }
            return IntegrateStringListIntoBlockGroupHierarchy(groupStrings, _rootBlockGroup);
        }

        public ItemFilterBlockGroup IntegrateStringListIntoBlockGroupHierarchy(IEnumerable<string> groupStrings, ItemFilterBlockGroup startItemGroup)
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
                startItemGroup.ChildGroups.Add(newItemGroup);
                inputGroups = inputGroups.Skip(1).ToList();
                return inputGroups.Count > 0 ? IntegrateStringListIntoBlockGroupHierarchy(inputGroups, newItemGroup) : newItemGroup;
            }
            inputGroups = inputGroups.Skip(1).ToList();
            return inputGroups.Count > 0 ? IntegrateStringListIntoBlockGroupHierarchy(inputGroups, matchingChildItemGroup) : matchingChildItemGroup;
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
