using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Filtration.ItemFilterPreview.Model;
using Filtration.ObjectModel;

namespace Filtration.ItemFilterPreview.Services
{
    internal interface IItemFilterProcessor
    {
        IReadOnlyDictionary<IItem, IItemFilterBlock> ProcessItemsAgainstItemFilterScript(IItemFilterScript itemFilterScript, IEnumerable<IItem> items);
    }

    internal class ItemFilterProcessor : IItemFilterProcessor
    {
        private readonly IBlockItemMatcher _blockItemMatcher;

        internal ItemFilterProcessor(IBlockItemMatcher blockItemMatcher)
        {
            _blockItemMatcher = blockItemMatcher;
        }

        public IReadOnlyDictionary<IItem, IItemFilterBlock> ProcessItemsAgainstItemFilterScript(IItemFilterScript itemFilterScript, IEnumerable<IItem> items)
        {
            var overallsw = Stopwatch.StartNew();

            var matchedItemBlockPairs = new Dictionary<IItem, IItemFilterBlock>();

            var sw = Stopwatch.StartNew();
            foreach (var item in items)
            {
                sw.Restart();

                var matchedBlock = itemFilterScript.ItemFilterBlocks
                    .Where(b => !(b is ItemFilterSection))
                    .FirstOrDefault(block => _blockItemMatcher.ItemBlockMatch(block, item));

                matchedItemBlockPairs.Add(item, matchedBlock);

                Debug.WriteLine("Processed Item in {0}ms", sw.ElapsedMilliseconds);
            }
            sw.Stop();

            overallsw.Stop();
            Debug.WriteLine("Total processing time: {0}ms", overallsw.ElapsedMilliseconds);
            return matchedItemBlockPairs;
        }
    }
}
