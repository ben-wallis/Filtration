using System.Collections.Generic;
using Filtration.ObjectModel;
using Filtration.Parser.Services;
using NUnit.Framework;

namespace Filtration.Parser.Tests.Services
{
    [TestFixture]
    public class TestBlockGroupHierarchyBuilder
    {
        [Test]
        public void IntegrateStringListIntoBlockGroupHierarchy_ReturnsBlockGroupWithCorrectName()
        {
            // Arrange
            var inputStrings = new List<string> {"Block Group", "Sub Block Group"};

            var rootBlock = new ItemFilterBlockGroup("Root", null);
            var builder = new BlockGroupHierarchyBuilder();

            // Act
            var result = builder.IntegrateStringListIntoBlockGroupHierarchy(inputStrings, rootBlock, true, true);
            
            // Assert
            Assert.AreEqual(1, rootBlock.ChildGroups.Count);
            Assert.AreEqual("Sub Block Group", result.ParentGroup.GroupName);
        }

        [Test]
        public void IntegrateStringListIntoBlockGroupHierarchy_SingleBlockGroup()
        {
            // Arrange
            var inputStrings = new List<string> { "Block Group" };

            var rootBlock = new ItemFilterBlockGroup("Root", null);
            var builder = new BlockGroupHierarchyBuilder();

            // Act
            var result = builder.IntegrateStringListIntoBlockGroupHierarchy(inputStrings, rootBlock, true, true);

            // Assert
            Assert.AreEqual(1, rootBlock.ChildGroups.Count);
            Assert.AreEqual("Block Group", result.ParentGroup.GroupName);
        }

        [Test]
        public void IntegrateStringListIntoBlockGroupHierarchy_AdvancedBlockGroup_MarksBlockGroupAsAdvanced()
        {
            // Arrange
            var inputStrings = new List<string> { "~Block Group" };

            var rootBlock = new ItemFilterBlockGroup("Root", null);
            var builder = new BlockGroupHierarchyBuilder();

            // Act
            var result = builder.IntegrateStringListIntoBlockGroupHierarchy(inputStrings, rootBlock, true, true);

            // Assert
            Assert.AreEqual(1, rootBlock.ChildGroups.Count);
            Assert.AreEqual("Block Group", result.ParentGroup.GroupName);
            Assert.AreEqual(true, result.ParentGroup.Advanced);
        }

        [Test]
        public void IntegrateStringListIntoBlockGroupHierarchy_AdvancedBlockGroup_ChildrenAreCreatedAsAdvanced()
        {
            // Arrange
            var inputStrings = new List<string> { "~Advanced Block Group", "This should be advanced too" };

            var rootBlock = new ItemFilterBlockGroup("Root", null);
            var builder = new BlockGroupHierarchyBuilder();

            // Act
            var result = builder.IntegrateStringListIntoBlockGroupHierarchy(inputStrings, rootBlock, true, true);

            // Assert
            Assert.AreEqual(1, rootBlock.ChildGroups.Count);
            Assert.AreEqual(true, result.ParentGroup.Advanced);
        }

        [Test]
        public void IntegrateStringListIntoBlockGroupHierarchy_ExistingAdvancedBlockGroup_SetsParentCorrectly()
        {
            // Arrange
            var inputStrings = new List<string> { "~Block Group" };

            var rootBlock = new ItemFilterBlockGroup("Root", null);
            var subBlock = new ItemFilterBlockGroup("Block Group", rootBlock, true);
            rootBlock.ChildGroups.Add(subBlock);

            var builder = new BlockGroupHierarchyBuilder();

            // Act
            var result = builder.IntegrateStringListIntoBlockGroupHierarchy(inputStrings, rootBlock, true, true);

            // Assert
            Assert.AreEqual(1, rootBlock.ChildGroups.Count);
            Assert.AreEqual("Block Group", result.ParentGroup.GroupName);
            Assert.AreEqual(true, result.ParentGroup.Advanced);
        }

        [Test]
        public void IntegrateStringListIntoBlockGroupHierarchy_MultipleBlockGroups()
        {
            // Arrange
            var rootBlock = new ItemFilterBlockGroup("Root", null);
            var builder = new BlockGroupHierarchyBuilder();

            // Act
            var inputStrings = new List<string> { "Block Group" };
            builder.IntegrateStringListIntoBlockGroupHierarchy(inputStrings, rootBlock, true, true);
            inputStrings = new List<string> { "Block Group 2" };
            builder.IntegrateStringListIntoBlockGroupHierarchy(inputStrings, rootBlock, true, true);
            inputStrings = new List<string> { "Block Group", "Sub Block Group" };
            var result = builder.IntegrateStringListIntoBlockGroupHierarchy(inputStrings, rootBlock, true, true);

            // Assert
            Assert.AreEqual(2, rootBlock.ChildGroups.Count);
            Assert.AreEqual("Sub Block Group", result.ParentGroup.GroupName);
        }
    }
}
