using System.Linq;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.LootExplosionStudio;

namespace Filtration.LootExplosionStudio.Services
{
    internal interface IItemFilterBlockFinderService
    {
        ItemFilterBlock FindBlockForLootItem(LootItem lootItem, ItemFilterScript script);
    }

    internal class ItemFilterBlockFinderService : IItemFilterBlockFinderService
    {
        public ItemFilterBlock FindBlockForLootItem(LootItem lootItem, ItemFilterScript script)
        {
            return script.ItemFilterBlocks.FirstOrDefault(block => BlockMatchesLootItem(lootItem, block));
        }

        private static bool BlockMatchesLootItem(LootItem lootItem, ItemFilterBlock block)
        {
            if (!block.BlockItems.OfType<StringListBlockItem>().All(blockItem => blockItem.MatchesLootItem(lootItem)))
            {
                return false;
            }

            if (
                !block.BlockItems.OfType<NumericFilterPredicateBlockItem>()
                    .All(blockItem => blockItem.MatchesLootItem(lootItem)))
            {
                return false;
            }

            return true;
        }
    }
}
