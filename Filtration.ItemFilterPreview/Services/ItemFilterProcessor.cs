using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Filtration.ObjectModel;

namespace Filtration.ItemFilterPreview.Services
{
    internal interface IItemFilterProcessor
    {
        List<IFilteredItem> ProcessItemsAgainstItemFilterScript(IItemFilterScript itemFilterScript, IEnumerable<IItem> items);
    }

    internal class ItemFilterProcessor : IItemFilterProcessor
    {
        private readonly IBlockItemMatcher _blockItemMatcher;

        public ItemFilterProcessor(IBlockItemMatcher blockItemMatcher)
        {
            _blockItemMatcher = blockItemMatcher;
        }

        public List<IFilteredItem> ProcessItemsAgainstItemFilterScript(IItemFilterScript itemFilterScript, IEnumerable<IItem> items)
        {
            var overallsw = Stopwatch.StartNew();
            
            var filteredItems = new List<IFilteredItem>();

            var sw = Stopwatch.StartNew();
            foreach (var item in items)
            {
                sw.Restart();

                var matchedBlock = itemFilterScript.ItemFilterBlocks
                                                   .OfType<IItemFilterBlock>()
                                                   .FirstOrDefault(block => _blockItemMatcher.ItemBlockMatch(block, item));

                filteredItems.Add(new FilteredItem(item, matchedBlock));

                Debug.WriteLine("Processed Item in {0}ms", sw.ElapsedMilliseconds);
            }
            sw.Stop();

            overallsw.Stop();
            Debug.WriteLine("Total processing time: {0}ms", overallsw.ElapsedMilliseconds);
            return filteredItems;
        }
    }
}
