using Filtration.ObjectModel;
using Filtration.ObjectModel.LootExplosionStudio;

namespace Filtration.LootExplosionStudio.Services
{
    internal interface ILootItemCollectionItemFilterCombinerService
    {
        void CombineLootItemCollectionWithItemFilterScript(LootItemCollection lootItemCollection,
            ItemFilterScript script);
    }

    internal class LootItemCollectionItemFilterCombinerService : ILootItemCollectionItemFilterCombinerService
    {
        private readonly ILootItemAppearanceService _lootItemAppearanceService;

        public LootItemCollectionItemFilterCombinerService(ILootItemAppearanceService lootItemAppearanceService)
        {
            _lootItemAppearanceService = lootItemAppearanceService;
        }

        public void CombineLootItemCollectionWithItemFilterScript(LootItemCollection lootItemCollection,
            ItemFilterScript script)
        {
            foreach (var lootItem in lootItemCollection)
            {
                _lootItemAppearanceService.ProcessLootItemAgainstFilterScript(lootItem, script);
            }
        }
    }
}
