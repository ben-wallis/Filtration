using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.LootExplosionStudio;
using NUnit.Framework;

namespace Filtration.ObjectModel.Tests.BlockItemBaseTypes
{
    [TestFixture]
    public class TestItemLevelBlockItem
    {
        [Test]
        public void MatchesBlockItem_NoMatch_ReturnsFalse()
        {
            // Arrange

            var blockItem = new ItemLevelBlockItem(FilterPredicateOperator.GreaterThan, 10);
            var lootItem = new LootItem {ItemLevel = 5};

            // Act
            var result = blockItem.MatchesLootItem(lootItem);

            // Assert
            Assert.IsFalse(result);
        }
        
        [Test]
        public void MatchesBlockItem_EqualsMatch_ReturnsTrue()
        {
            // Arrange

            var blockItem = new ItemLevelBlockItem(FilterPredicateOperator.Equal, 10);
            var lootItem = new LootItem { ItemLevel = 10 };

            // Act
            var result = blockItem.MatchesLootItem(lootItem);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void MatchesBlockItem_GreaterThanMatch_ReturnsTrue()
        {
            // Arrange

            var blockItem = new ItemLevelBlockItem(FilterPredicateOperator.GreaterThan, 10);
            var lootItem = new LootItem { ItemLevel = 50 };

            // Act
            var result = blockItem.MatchesLootItem(lootItem);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void MatchesBlockItem_GreaterThanOrEqualMatch_ReturnsTrue()
        {
            // Arrange

            var blockItem = new ItemLevelBlockItem(FilterPredicateOperator.GreaterThanOrEqual, 10);
            var lootItem = new LootItem { ItemLevel = 50 };

            // Act
            var result = blockItem.MatchesLootItem(lootItem);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void MatchesBlockItem_LessThan_ReturnsTrue()
        {
            // Arrange

            var blockItem = new ItemLevelBlockItem(FilterPredicateOperator.LessThan, 10);
            var lootItem = new LootItem { ItemLevel = 1 };

            // Act
            var result = blockItem.MatchesLootItem(lootItem);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void MatchesBlockItem_LessThanOrEqual_ReturnsTrue()
        {
            // Arrange

            var blockItem = new ItemLevelBlockItem(FilterPredicateOperator.LessThanOrEqual, 10);
            var lootItem = new LootItem { ItemLevel = 1 };

            // Act
            var result = blockItem.MatchesLootItem(lootItem);

            // Assert
            Assert.IsTrue(result);
        }
    }
}
