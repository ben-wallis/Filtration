using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Filtration.ItemFilterPreview.Model;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Extensions;

namespace Filtration.ItemFilterPreview.Services
{
    internal interface IItemBlockItemMatcher
    {
        bool BaseTypeBlockItemMatch(BaseTypeBlockItem baseTypeBlockItem, IItem item);
        bool ClassBlockItemMatch(ClassBlockItem classBlockItem, IItem item);
        bool DropLevelBlockItemMatch(DropLevelBlockItem dropLevelBlockItem, IItem item);
    }

    internal class ItemBlockItemMatcher : IItemBlockItemMatcher
    {
        public bool BaseTypeBlockItemMatch(BaseTypeBlockItem baseTypeBlockItem, IItem item)
        {
            return baseTypeBlockItem.Items.Any(b => item.BaseType.StartsWith(b));
        }

        public bool ClassBlockItemMatch(ClassBlockItem classBlockItem, IItem item)
        {
            return classBlockItem.Items.Any(c => item.ItemClass.StartsWith(c));
        }

        public bool DropLevelBlockItemMatch(DropLevelBlockItem dropLevelBlockItem, IItem item)
        {
            return NumericFilterPredicateBlockItemMatch(dropLevelBlockItem, item.DropLevel);
        }

        public bool HeightBlockItemMatch(HeightBlockItem heightBlockItem, IItem item)
        {
            return NumericFilterPredicateBlockItemMatch(heightBlockItem, item.Height);
        }

        public bool ItemLevelBlockItemMatch(ItemLevelBlockItem itemLevelBlockItem, IItem item)
        {
            return NumericFilterPredicateBlockItemMatch(itemLevelBlockItem, item.ItemLevel);
        }

        public bool LinkedSocketsBlockItemMatch(LinkedSocketsBlockItem linkedSocketsBlockItem, IItem item)
        {
            return NumericFilterPredicateBlockItemMatch(linkedSocketsBlockItem, item.LinkedSockets);
        }

        public bool QualityBlockItemMatch(QualityBlockItem qualityBlockItem, IItem item)
        {
            return NumericFilterPredicateBlockItemMatch(qualityBlockItem, item.Quality);
        }

        public bool RarityBlockItemMatch(RarityBlockItem qualityBlockItem, IItem item)
        {
            return NumericFilterPredicateBlockItemMatch(qualityBlockItem, (int)item.ItemRarity);
        }

        public bool SocketsBlockItemMatch(SocketsBlockItem socketsBlockItem, IItem item)
        {
            return NumericFilterPredicateBlockItemMatch(socketsBlockItem, item.Sockets);
        }

        public bool WidthBlockItemMatch(WidthBlockItem widthBlockItem, IItem item)
        {
            return NumericFilterPredicateBlockItemMatch(widthBlockItem, item.Width);
        }

        public bool SocketGroupBlockItemMatch(SocketGroupBlockItem socketGroupBlockItem, IItem item)
        {

            foreach (var blockItemSocketGroup in socketGroupBlockItem.SocketGroups) // for each group of sockets in the block item
            {
                foreach (var itemLinkedSocketGroup in item.LinkedSocketGroups) // for each linked socket group in the item
                {
                    if (SocketGroupHasRequiredSocketColors(itemLinkedSocketGroup, blockItemSocketGroup))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool SocketGroupHasRequiredSocketColors(SocketGroup itemLinkedSocketGroup, SocketGroup blockItemSocketGroup)
        {

            var blockSocketGroupColorCounts = blockItemSocketGroup.GroupBy(i => i.Color, (key, values) => new { SocketColor = key, Count = values.Count() }).ToList();
            var itemSocketGroupColorCounts = itemLinkedSocketGroup.GroupBy(i => i.Color, (key, values) => new {SocketColor = key, Count = values.Count()}).ToList();

            foreach (var blockItemSocketColorCount in blockSocketGroupColorCounts)
            {
                var match = itemSocketGroupColorCounts.FirstOrDefault(i => i.SocketColor == blockItemSocketColorCount.SocketColor && i.Count >= blockItemSocketColorCount.Count);
                if (match == null)
                {
                    return false;
                }
            }

            return true;
        }

        private bool NumericFilterPredicateBlockItemMatch<T>(T numericFilterPredicateBlockItem, int matchValue) where T : NumericFilterPredicateBlockItem
        {
            return numericFilterPredicateBlockItem.FilterPredicate.CompareUsing(matchValue);
        }
    }
}
