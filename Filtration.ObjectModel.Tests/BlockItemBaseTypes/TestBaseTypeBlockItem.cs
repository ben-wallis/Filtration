using System;
using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.LootExplosionStudio;
using NUnit.Framework;

namespace Filtration.ObjectModel.Tests.BlockItemBaseTypes
{
    [TestFixture]
    public class TestBaseTypeBlockItem
    {
        [Test]
        public void MatchesBlockItem_BlankLootItem_ReturnsFalse()
        {
            // Arrange
            
            var blockItem = new BaseTypeBlockItem();
            var testInputLootItem = new LootItem();

            // Act
            var result = blockItem.MatchesLootItem(testInputLootItem);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void MatchesBlockItem_StringMatch_ReturnsTrue()
        {
            // Arrange
            var testInputBaseType = "Test Base Type";

            var blockItem = new BaseTypeBlockItem();
            blockItem.Items.Add("Testblah");
            blockItem.Items.Add(testInputBaseType);
            blockItem.Items.Add("Another Base Type");

            var testInputLootItem = new LootItem { BaseType = testInputBaseType};

            // Act
            var result = blockItem.MatchesLootItem(testInputLootItem);

            // Assert
            Assert.IsTrue(result);
        }
    }
}
