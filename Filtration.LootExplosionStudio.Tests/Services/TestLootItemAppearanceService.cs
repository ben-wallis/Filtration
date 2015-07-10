using System.Windows.Media;
using Filtration.LootExplosionStudio.Services;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.LootExplosionStudio;
using Moq;
using NUnit.Framework;

namespace Filtration.LootExplosionStudio.Tests.Services
{
    [TestFixture]
    class TestLootItemAppearanceService
    {
        private LootItemAppearanceServiceTestUtility _testUtility;

        [SetUp]
        public void ItemFilterProcessingServiceTestSetup()
        {
            _testUtility = new LootItemAppearanceServiceTestUtility();
        }

        [Test]
        public void ProcessLootItemAgainstScript_NoMatchingBlocks_NormalItem_SetsCorrectTextColor()
        {
            // Arrange
            _testUtility.TestLootItem.Rarity = ItemRarity.Normal;
            
            // Act
            _testUtility.Service.ProcessLootItemAgainstFilterScript(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(DefaultLootItemAppearanceConstants.NormalTextColor, _testUtility.TestLootItem.TextColor);
        }

        [Test]
        public void ProcessLootItemAgainstScript_NoMatchingBlocks_MagicItem_SetsCorrectTextColor()
        {
            // Arrange
            _testUtility.TestLootItem.Rarity = ItemRarity.Magic;
            _testUtility.TestBlock.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.GreaterThan, 99));

            // Act
            _testUtility.Service.ProcessLootItemAgainstFilterScript(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(DefaultLootItemAppearanceConstants.MagicTextColor, _testUtility.TestLootItem.TextColor);
        }

        [Test]
        public void ProcessLootItemAgainstScript_NoMatchingBlocks_RareItem_SetsCorrectTextColor()
        {
            // Arrange
            _testUtility.TestLootItem.Rarity = ItemRarity.Rare;
            _testUtility.TestBlock.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.GreaterThan, 99));

            // Act
            _testUtility.Service.ProcessLootItemAgainstFilterScript(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(DefaultLootItemAppearanceConstants.RareTextColor, _testUtility.TestLootItem.TextColor);
        }

        [Test]
        public void ProcessLootItemAgainstScript_NoMatchingBlocks_UniqueItem_SetsCorrectTextColor()
        {
            // Arrange
            _testUtility.TestLootItem.Rarity = ItemRarity.Unique;
            _testUtility.TestBlock.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.GreaterThan, 99));

            // Act
            _testUtility.Service.ProcessLootItemAgainstFilterScript(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(DefaultLootItemAppearanceConstants.UniqueTextColor, _testUtility.TestLootItem.TextColor);
        }

        [Test]
        public void ProcessLootItemAgainstScript_NoMatchingBlocks_SetsCorrectBackgroundColor()
        {
            // Arrange
            _testUtility.TestBlock.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.GreaterThan, 99));

            // Act
            _testUtility.Service.ProcessLootItemAgainstFilterScript(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(DefaultLootItemAppearanceConstants.BackgroundColor, _testUtility.TestLootItem.BackgroundColor);
        }

        [Test]
        public void ProcessLootItemAgainstScript_NoMatchingBlocks_SetsCorrectBorderColor()
        {
            // Arrange
            _testUtility.TestBlock.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.GreaterThan, 99));

            // Act
            _testUtility.Service.ProcessLootItemAgainstFilterScript(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(DefaultLootItemAppearanceConstants.BorderColor, _testUtility.TestLootItem.BorderColor);
        }

        [Test]
        public void ProcessLootItemAgainstScript_MatchingTextColorOnly_SetsColorsCorrectly()
        {
            // Arrange
            var testInputTextColor = new Color {R = 123, G = 5, B = 22, A = 200};
            _testUtility.TestBlock.BlockItems.Add(new TextColorBlockItem {Color = testInputTextColor});

            // Act
            _testUtility.Service.ProcessLootItemAgainstFilterScript(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(testInputTextColor, _testUtility.TestLootItem.TextColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.BackgroundColor, _testUtility.TestLootItem.BackgroundColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.BorderColor, _testUtility.TestLootItem.BorderColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.FontSize, _testUtility.TestLootItem.FontSize );
        }

        [Test]
        public void ProcessLootItemAgainstScript_MatchingBackgroundColorOnly_RarityNormal_SetsColorsCorrectly()
        {
            // Arrange
            var testInputBackgroundColor = new Color {R = 123, G = 5, B = 22, A = 200};
            _testUtility.TestBlock.BlockItems.Add(new BackgroundColorBlockItem {Color = testInputBackgroundColor});

            // Act
            _testUtility.Service.ProcessLootItemAgainstFilterScript(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(testInputBackgroundColor, _testUtility.TestLootItem.BackgroundColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.NormalTextColor, _testUtility.TestLootItem.TextColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.BorderColor, _testUtility.TestLootItem.BorderColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.FontSize, _testUtility.TestLootItem.FontSize);
        }

        [Test]
        public void ProcessLootItemAgainstScript_MatchingBackgroundColorOnly_RarityMagic_SetsColorsCorrectly()
        {
            // Arrange
            var testInputBackgroundColor = new Color { R = 123, G = 5, B = 22, A = 200 };
            _testUtility.TestBlock.BlockItems.Add(new BackgroundColorBlockItem {Color = testInputBackgroundColor});
            _testUtility.TestLootItem.Rarity = ItemRarity.Magic;

            // Act
            _testUtility.Service.ProcessLootItemAgainstFilterScript(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(testInputBackgroundColor, _testUtility.TestLootItem.BackgroundColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.MagicTextColor, _testUtility.TestLootItem.TextColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.BorderColor, _testUtility.TestLootItem.BorderColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.FontSize, _testUtility.TestLootItem.FontSize);
        }

        [Test]
        public void ProcessLootItemAgainstScript_MatchingBackgroundColorOnly_RarityRare_SetsColorsCorrectly()
        {
            // Arrange
            var testInputBackgroundColor = new Color { R = 123, G = 5, B = 22, A = 200 };
            _testUtility.TestBlock.BlockItems.Add(new BackgroundColorBlockItem { Color = testInputBackgroundColor });
            _testUtility.TestLootItem.Rarity = ItemRarity.Rare;

            // Act
            _testUtility.Service.ProcessLootItemAgainstFilterScript(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(testInputBackgroundColor, _testUtility.TestLootItem.BackgroundColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.RareTextColor, _testUtility.TestLootItem.TextColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.BorderColor, _testUtility.TestLootItem.BorderColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.FontSize, _testUtility.TestLootItem.FontSize);
        }

        [Test]
        public void ProcessLootItemAgainstScript_MatchingBackgroundColorOnly_RarityUnique_SetsColorsCorrectly()
        {
            // Arrange
            var testInputBackgroundColor = new Color { R = 123, G = 5, B = 22, A = 200 };
            _testUtility.TestBlock.BlockItems.Add(new BackgroundColorBlockItem { Color = testInputBackgroundColor });
            _testUtility.TestLootItem.Rarity = ItemRarity.Unique;

            // Act
            _testUtility.Service.ProcessLootItemAgainstFilterScript(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(testInputBackgroundColor, _testUtility.TestLootItem.BackgroundColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.UniqueTextColor, _testUtility.TestLootItem.TextColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.BorderColor, _testUtility.TestLootItem.BorderColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.FontSize, _testUtility.TestLootItem.FontSize);
        }
        
        [Test]
        public void ProcessLootItemAgainstScript_MatchingBorderColorOnly_RarityUnique_SetsColorsCorrectly()
        {
            // Arrange
            var testInputBorderColor = new Color { R = 123, G = 5, B = 22, A = 200 };
            _testUtility.TestBlock.BlockItems.Add(new BorderColorBlockItem { Color = testInputBorderColor });

            // Act
            _testUtility.Service.ProcessLootItemAgainstFilterScript(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(testInputBorderColor, _testUtility.TestLootItem.BorderColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.NormalTextColor, _testUtility.TestLootItem.TextColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.BackgroundColor, _testUtility.TestLootItem.BackgroundColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.FontSize, _testUtility.TestLootItem.FontSize);
        }

        [Test]
        public void ProcessLootItemAgainstScript_MatchingFontSizeOnly_RarityUnique_SetsColorsCorrectly()
        {
            // Arrange
            var testInputFontSize = 22;
            _testUtility.TestBlock.BlockItems.Add(new FontSizeBlockItem(22));

            // Act
            _testUtility.Service.ProcessLootItemAgainstFilterScript(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(testInputFontSize, _testUtility.TestLootItem.FontSize);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.NormalTextColor, _testUtility.TestLootItem.TextColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.BackgroundColor, _testUtility.TestLootItem.BackgroundColor);
            Assert.AreEqual(DefaultLootItemAppearanceConstants.BorderColor, _testUtility.TestLootItem.BorderColor);
        }

        [Test]
        public void ProcessLootItemAgainstScript_AllAppearanceMatching_SetsColorsCorrectly()
        {
            // Arrange
            var testInputFontSize = 22;
            var testInputTextColor = new Color { R = 123, G = 5, B = 22, A = 200 };
            var testInputBackgroundColor = new Color { R = 123, G = 59, B = 27, A = 50 };
            var testInputBorderColor = new Color { R = 166, G = 0, B = 100, A = 255 };

            _testUtility.TestBlock.BlockItems.Add(new FontSizeBlockItem(22));
            _testUtility.TestBlock.BlockItems.Add(new BorderColorBlockItem { Color = testInputBorderColor });
            _testUtility.TestBlock.BlockItems.Add(new BackgroundColorBlockItem { Color = testInputBackgroundColor });
            _testUtility.TestBlock.BlockItems.Add(new TextColorBlockItem { Color = testInputTextColor });

            // Act
            _testUtility.Service.ProcessLootItemAgainstFilterScript(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(testInputFontSize, _testUtility.TestLootItem.FontSize);
            Assert.AreEqual(testInputTextColor, _testUtility.TestLootItem.TextColor);
            Assert.AreEqual(testInputBackgroundColor, _testUtility.TestLootItem.BackgroundColor);
            Assert.AreEqual(testInputBorderColor, _testUtility.TestLootItem.BorderColor);
        }

        private class LootItemAppearanceServiceTestUtility
        {
            public LootItemAppearanceServiceTestUtility()
            {
                TestBlock = new ItemFilterBlock();
                TestScript = new ItemFilterScript();
                TestScript.ItemFilterBlocks.Add(TestBlock);
                TestLootItem = new LootItem();

                MockItemFilterBlockFinderService = new Mock<IItemFilterBlockFinderService>();
                MockItemFilterBlockFinderService.Setup(
                    b => b.FindBlockForLootItem(TestLootItem, TestScript))
                    .Returns(TestBlock);

                Service = new LootItemAppearanceService(MockItemFilterBlockFinderService.Object);
            }

            public Mock<IItemFilterBlockFinderService> MockItemFilterBlockFinderService { get; private set; }

            public ItemFilterScript TestScript { get; private set; }
            public ItemFilterBlock TestBlock { get; private set; }
            public LootItem TestLootItem { get; private set; }
            public LootItemAppearanceService Service { get; private set; }
        }
    }
}
