using Filtration.Models;
using Filtration.Models.BlockItemTypes;
using NUnit.Framework;

namespace Filtration.Tests.Models
{
    [TestFixture]
    public class TestLootFilterBlock
    {
        [Test]
        public void LootFilterBlock_BlockCount_ReturnsCorrectNumber()
        {
            // Arrange
            var block = new LootFilterBlock();
            block.BlockItems.Add(new ItemLevelBlockItem());
            block.BlockItems.Add(new ItemLevelBlockItem());
            block.BlockItems.Add(new ItemLevelBlockItem());
            block.BlockItems.Add(new ItemLevelBlockItem());

            // Act
            var count = block.BlockCount(typeof (ItemLevelBlockItem));

            // Assert
            Assert.AreEqual(4, count);
        }

        [Test]
        public void LootFilterBlock_AddBlockItemAllowed_LessThanMaximum_ReturnsTrue()
        {
            // Arrange
            var block = new LootFilterBlock();
            block.BlockItems.Add(new ItemLevelBlockItem());

            // Act
            bool result = block.AddBlockItemAllowed(typeof (ItemLevelBlockItem));

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void LootFilterBlock_AddBlockItemAllowed_MoreThanMaximum_ReturnsFalse()
        {
            // Arrange
            var block = new LootFilterBlock();
            block.BlockItems.Add(new SoundBlockItem());

            // Act
            bool result = block.AddBlockItemAllowed(typeof (SoundBlockItem));

            // Assert
            Assert.IsFalse(result);
        }
    }
}
