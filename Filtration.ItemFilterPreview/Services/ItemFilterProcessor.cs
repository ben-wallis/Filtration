using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Filtration.ItemFilterPreview.Model;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemBaseTypes;

namespace Filtration.ItemFilterPreview.Services
{
    internal class ItemFilterProcessor
    {
        private readonly IBlockItemMatcher _blockItemMatcher;

        internal ItemFilterProcessor(IBlockItemMatcher blockItemMatcher)
        {
            _blockItemMatcher = blockItemMatcher;
        }

        public IReadOnlyDictionary<IItem, IItemFilterBlock> ProcessItemsAgainstItemFilterScript(IItemFilterScript itemFilterScript, IEnumerable<IItem> items)
        {
            var matchedItemBlockPairs = new Dictionary<IItem, IItemFilterBlock>();

            var sw = Stopwatch.StartNew();
            foreach (var item in items)
            {
                sw.Restart();

                var matchedBlock = itemFilterScript.ItemFilterBlocks.FirstOrDefault(block => _blockItemMatcher.ItemBlockMatch(block, item));
                matchedItemBlockPairs.Add(item, matchedBlock);

                Debug.WriteLine("Processed Item in {0}ms", sw.ElapsedMilliseconds);
            }
            sw.Stop();

            return matchedItemBlockPairs;
        }
    }
}
