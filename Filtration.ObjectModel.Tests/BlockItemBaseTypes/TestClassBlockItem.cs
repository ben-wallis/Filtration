using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.LootExplosionStudio;
using NUnit.Framework;

namespace Filtration.ObjectModel.Tests.BlockItemBaseTypes
{
    [TestFixture]
    public class TestClassBlockItem
    {
        [Test]
        public void MatchesBlockItem_BlankLootItem_ReturnsFalse()
        {
            // Arrange

            var blockItem = new ClassBlockItem();
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
            var testInputClass = "Test Class";

            var blockItem = new ClassBlockItem();
            blockItem.Items.Add("Testblah");
            blockItem.Items.Add(testInputClass);
            blockItem.Items.Add("Another Base Type");

            var testInputLootItem = new LootItem { Class = testInputClass };

            // Act
            var result = blockItem.MatchesLootItem(testInputLootItem);

            // Assert
            Assert.IsTrue(result);
        }
    }
}
