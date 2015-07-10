using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.LootExplosionStudio;

namespace Filtration.LootExplosionStudio.Services
{
    internal interface ILootItemAppearanceService
    {
        void ProcessLootItemAgainstFilterScript(LootItem lootItem, ItemFilterScript script);
    }

    internal class LootItemAppearanceService : ILootItemAppearanceService
    {
        private readonly IItemFilterBlockFinderService _blockFinderService;

        public LootItemAppearanceService(IItemFilterBlockFinderService blockFinderService)
        {
            _blockFinderService = blockFinderService;
        }

        public void ProcessLootItemAgainstFilterScript(LootItem lootItem, ItemFilterScript script)
        {
            var matchedBlock = _blockFinderService.FindBlockForLootItem(lootItem, script);
            if (matchedBlock == null)
            {
                lootItem.TextColor = GetDefaultTextColorForRarity(lootItem.Rarity);
                lootItem.BackgroundColor = DefaultLootItemAppearanceConstants.BackgroundColor;
                lootItem.BorderColor = DefaultLootItemAppearanceConstants.BorderColor;
                lootItem.FontSize = 35;
                return;
            }

            lootItem.TextColor = matchedBlock.HasBlockItemOfType<TextColorBlockItem>()
                ? matchedBlock.BlockItems.OfType<TextColorBlockItem>().First().Color
                : GetDefaultTextColorForRarity(lootItem.Rarity);

            lootItem.BackgroundColor = matchedBlock.HasBlockItemOfType<BackgroundColorBlockItem>()
                ? matchedBlock.BlockItems.OfType<BackgroundColorBlockItem>().First().Color
                : DefaultLootItemAppearanceConstants.BackgroundColor;

            lootItem.BorderColor = matchedBlock.HasBlockItemOfType<BorderColorBlockItem>()
                ? matchedBlock.BlockItems.OfType<BorderColorBlockItem>().First().Color
                : DefaultLootItemAppearanceConstants.BorderColor;

            lootItem.FontSize = matchedBlock.HasBlockItemOfType<FontSizeBlockItem>()
                ? matchedBlock.BlockItems.OfType<FontSizeBlockItem>().First().Value
                : 35;
        }

        private Color GetDefaultTextColorForRarity(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Normal:
                {
                    return DefaultLootItemAppearanceConstants.NormalTextColor;
                }
                case ItemRarity.Magic:
                {
                    return DefaultLootItemAppearanceConstants.MagicTextColor;
                }
                case ItemRarity.Rare:
                {
                    return DefaultLootItemAppearanceConstants.RareTextColor;
                }
                case ItemRarity.Unique:
                {
                    return DefaultLootItemAppearanceConstants.UniqueTextColor;
                }
                default:
                {
                    return DefaultLootItemAppearanceConstants.NormalTextColor;
                }
            }
        }
    }
}
