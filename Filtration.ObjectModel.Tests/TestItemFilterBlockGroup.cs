using NUnit.Framework;

namespace Filtration.ObjectModel.Tests
{
    [TestFixture]
    public class TestItemFilterBlockGroup
    {
        [Test]
        public void ToString_ReturnsFullBlockHierarchy()
        {
            // Arrange
            const string expectedResult = "Child 1 Block Group - Child 2 Block Group";

            var rootBlockGroup = new ItemFilterBlockGroup("Root Block Group", null);
            var child1BlockGroup = new ItemFilterBlockGroup("Child 1 Block Group", rootBlockGroup);
            var child2BlockGroup = new ItemFilterBlockGroup("Child 2 Block Group", child1BlockGroup);
            
            // Act
            var result = child2BlockGroup.ToString();

            // Assert
            Assert.AreEqual(expectedResult, result);
        }
        
        [Test]
        public void ToString_AddsTildeForAdvancedBlock()
        {
            // Arrange
            const string expectedResult = "~Child 1 Block Group - Child 2 Block Group";

            var rootBlockGroup = new ItemFilterBlockGroup("Root Block Group", null);
            var child1BlockGroup = new ItemFilterBlockGroup("Child 1 Block Group", rootBlockGroup, true);
            var child2BlockGroup = new ItemFilterBlockGroup("Child 2 Block Group", child1BlockGroup);

            // Act
            var result = child2BlockGroup.ToString();

            // Assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}
