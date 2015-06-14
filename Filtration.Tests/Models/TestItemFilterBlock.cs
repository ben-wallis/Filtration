using Filtration.Models;
using Filtration.Models.BlockItemTypes;
using NUnit.Framework;

namespace Filtration.Tests.Models
{
    [TestFixture]
    public class TestItemFilterBlock
    {
        [Test]
        public void ItemFilterBlock_BlockCount_ReturnsCorrectNumber()
        {
            // Arrange
            var block = new ItemFilterBlock();
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
        public void ItemFilterBlock_AddBlockItemAllowed_LessThanMaximum_ReturnsTrue()
        {
            // Arrange
            var block = new ItemFilterBlock();
            block.BlockItems.Add(new ItemLevelBlockItem());

            // Act
            bool result = block.AddBlockItemAllowed(typeof (ItemLevelBlockItem));

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ItemFilterBlock_AddBlockItemAllowed_MoreThanMaximum_ReturnsFalse()
        {
            // Arrange
            var block = new ItemFilterBlock();
            block.BlockItems.Add(new SoundBlockItem());

            // Act
            bool result = block.AddBlockItemAllowed(typeof (SoundBlockItem));

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void HasParentInBlockGroupHierarchy_ReturnsCorrectResult()
        {
            // Arrange
            var testInputRootBlockGroup = new ItemFilterBlockGroup("Root Block Group", null);
            var testInputSubBlockGroup = new ItemFilterBlockGroup("Sub Block Group", testInputRootBlockGroup);
            var testInputSubSubBlockGroup = new ItemFilterBlockGroup("Sub Sub Block Group", testInputSubBlockGroup);

            var block = new ItemFilterBlock {BlockGroup = testInputSubSubBlockGroup};

            // Act

            // Assert
            Assert.AreEqual(true, block.HasBlockGroupInParentHierarchy(testInputRootBlockGroup, block.BlockGroup));
            Assert.AreEqual(true, block.HasBlockGroupInParentHierarchy(testInputSubBlockGroup, block.BlockGroup));
        }
    }
}
