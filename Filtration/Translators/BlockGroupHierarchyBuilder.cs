using System;
using System.Collections.Generic;
using System.Linq;
using Filtration.Models;

namespace Filtration.Translators
{
    internal interface IBlockGroupHierarchyBuilder
    {
        void Initialise(ItemFilterBlockGroup rootBlockGroup);
        ItemFilterBlockGroup IntegrateStringListIntoBlockGroupHierarchy(IEnumerable<string> groupStrings);
    }

    internal class BlockGroupHierarchyBuilder : IBlockGroupHierarchyBuilder
    {
        private ItemFilterBlockGroup _rootBlockGroup;

        public void Initialise(ItemFilterBlockGroup rootBlockGroup)
        {
            _rootBlockGroup = rootBlockGroup;
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

            ItemFilterBlockGroup matchingChildItemGroup = null;
            if (startItemGroup.ChildGroups.Count(g => g.GroupName == firstGroup) > 0)
            {
                matchingChildItemGroup = startItemGroup.ChildGroups.First(c => c.GroupName == firstGroup);
            }

            if (matchingChildItemGroup == null)
            {
                var newItemGroup = new ItemFilterBlockGroup(inputGroups.First().Trim(), startItemGroup);
                startItemGroup.ChildGroups.Add(newItemGroup);
                inputGroups = inputGroups.Skip(1).ToList();
                return inputGroups.Count > 0 ? IntegrateStringListIntoBlockGroupHierarchy(inputGroups, newItemGroup) : newItemGroup;
            }
            inputGroups = inputGroups.Skip(1).ToList();
            return inputGroups.Count > 0 ? IntegrateStringListIntoBlockGroupHierarchy(inputGroups, matchingChildItemGroup) : matchingChildItemGroup;
        }
    }
}
