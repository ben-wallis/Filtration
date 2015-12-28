using System.Collections.Generic;
using System.Collections.ObjectModel;
using Filtration.ItemFilterPreview.Model;
using Filtration.ItemFilterPreview.Services;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;
using Moq;
using NUnit.Framework;

namespace Filtration.ItemFilterPreview.Tests.Services
{
    [TestFixture]
    public class TestItemFilterProcessor
    {
        private ItemFilterProcessorTestUtility _testUtility;

        [SetUp]
        public void ItemFilterProcessorTestSetUp()
        {
            _testUtility = new ItemFilterProcessorTestUtility();
        }

        [Test]
        public void ProcessItemsAgainstItemFilterScript_Matches_ReturnsTrue()
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.ItemClass == "Test Class");
            var testInputBlockItem = new ActionBlockItem(BlockAction.Show);
            var testInputBlock = Mock.Of<IItemFilterBlock>(b => b.Action == BlockAction.Show && 
                                                           b.BlockItems == new ObservableCollection<IItemFilterBlockItem> {testInputBlockItem});
            var testInputScript = Mock.Of<IItemFilterScript>(s => s.ItemFilterBlocks == new ObservableCollection<IItemFilterBlock> {testInputBlock});

            //Act
            var result = _testUtility.ItemFilterProcessor.ProcessItemsAgainstItemFilterScript(testInputScript, new List<IItem> { testInputItem });

            //Assert
            Assert.AreEqual(testInputBlock, result[testInputItem]);
        }

        private class ItemFilterProcessorTestUtility
        {
            public ItemFilterProcessorTestUtility()
            {
                // Mock setups
                MockBlockItemMatcher = new Mock<IBlockItemMatcher>();

                // Class under-test instantiation
                ItemFilterProcessor = new ItemFilterProcessor(MockBlockItemMatcher.Object);
            }

            public ItemFilterProcessor ItemFilterProcessor { get; private set; }

            public Mock<IBlockItemMatcher> MockBlockItemMatcher { get; }
        }
    }
}
