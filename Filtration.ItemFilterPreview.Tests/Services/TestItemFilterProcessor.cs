using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Filtration.ItemFilterPreview.Services;
using Filtration.ItemFilterPreview.Tests.Properties;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Factories;
using Filtration.Parser.Services;
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
            var testInputItem = Mock.Of<IItem>();
            var testInputBlock = Mock.Of<IItemFilterBlock>();
            var testInputScript = Mock.Of<IItemFilterScript>(s => s.ItemFilterBlocks == new ObservableCollection<IItemFilterBlockBase> {testInputBlock});

            _testUtility.MockBlockItemMatcher
                .Setup(b => b.ItemBlockMatch(testInputBlock, testInputItem))
                .Returns(true)
                .Verifiable();

            //Act
            var result = _testUtility.ItemFilterProcessor.ProcessItemsAgainstItemFilterScript(testInputScript, new List<IItem> { testInputItem });

            //Assert
            _testUtility.MockBlockItemMatcher.Verify();
            Assert.AreEqual(testInputBlock, result.First(r => r.ItemFilterBlock == testInputBlock).ItemFilterBlock);
        }

        [Test]
        public void ProcessItemsAgainstItemFilterScript_DoesNotMatch_ResultHasNullItemFilterBlock()
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>();
            var testInputBlock = Mock.Of<IItemFilterBlock>();
            var testInputScript = Mock.Of<IItemFilterScript>(s => s.ItemFilterBlocks == new ObservableCollection<IItemFilterBlockBase> { testInputBlock });

            _testUtility.MockBlockItemMatcher
                .Setup(b => b.ItemBlockMatch(testInputBlock, testInputItem))
                .Returns(false)
                .Verifiable();

            //Act
            var result = _testUtility.ItemFilterProcessor.ProcessItemsAgainstItemFilterScript(testInputScript, new List<IItem> { testInputItem });

            //Assert
            _testUtility.MockBlockItemMatcher.Verify();
            Assert.AreEqual(null, result.First(r => r.Item == testInputItem).ItemFilterBlock);
        }

        [Test]
        [Ignore("Outdated item filter")]
        public void ProcessItemsAgainstItemFilterScript_IntegrationTest()
        {
            //Arrange
            var testInputScriptFile = Resources.MuldiniFilterScript;
            var blockGroupHierarchyBuilder = new BlockGroupHierarchyBuilder();
            var mockItemFilterScriptFactory = new Mock<IItemFilterScriptFactory>();
            mockItemFilterScriptFactory
                .Setup(i => i.Create())
                .Returns(new ItemFilterScript());

            var scriptTranslator = new ItemFilterScriptTranslator(blockGroupHierarchyBuilder, new ItemFilterBlockTranslator(blockGroupHierarchyBuilder), mockItemFilterScriptFactory.Object);
            var script = scriptTranslator.TranslateStringToItemFilterScript(testInputScriptFile);

            var testInputItem = new Item
            {
                BaseType = "BlahdeBlah",
                ItemClass = "Wands",
                ItemRarity = ItemRarity.Magic,
                ItemLevel = 9,
                DropLevel = 9,
                Height = 3,
                Width = 1,
                SocketGroups = new List<SocketGroup> {new SocketGroup(new List<Socket> {new Socket(SocketColor.Red)}, false)}
            };

            var itemFilterProcessor = new ItemFilterProcessor(new BlockItemMatcher());

            //Act
            var result = itemFilterProcessor.ProcessItemsAgainstItemFilterScript(script, new List<IItem> { testInputItem });

            //Assert
            Assert.AreEqual("Wands", result.First().ItemFilterBlock.BlockItems.OfType<ClassBlockItem>().First().Items.First());
        }

        [Test]
        [Ignore("Outdated item filter")]
        public void ProcessItemsAgainstItemFilterScript_IntegrationTest_10Items()
        {
            //Arrange
            var testInputScriptFile = Resources.MuldiniFilterScript;
            var blockGroupHierarchyBuilder = new BlockGroupHierarchyBuilder();
            var mockItemFilterScriptFactory = new Mock<IItemFilterScriptFactory>();
            mockItemFilterScriptFactory
                .Setup(i => i.Create())
                .Returns(new ItemFilterScript());
            var scriptTranslator = new ItemFilterScriptTranslator(blockGroupHierarchyBuilder, new ItemFilterBlockTranslator(blockGroupHierarchyBuilder), mockItemFilterScriptFactory.Object);
            var script = scriptTranslator.TranslateStringToItemFilterScript(testInputScriptFile);

            var testInputItems = new List<IItem>
            {
                new Item
                {
                    BaseType = "BlahdeBlah",
                    ItemClass = "Wands",
                    ItemRarity = ItemRarity.Magic,
                    ItemLevel = 9,
                    DropLevel = 9,
                    Height = 3,
                    Width = 1,
                    SocketGroups = new List<SocketGroup> {new SocketGroup(new List<Socket> {new Socket(SocketColor.Red)}, false)}
                },
                new Item
                {
                    BaseType = "BlahdeBlah",
                    ItemClass = "Wands",
                    ItemRarity = ItemRarity.Magic,
                    ItemLevel = 9,
                    DropLevel = 9,
                    Height = 3,
                    Width = 1,
                    SocketGroups = new List<SocketGroup> {new SocketGroup(new List<Socket> {new Socket(SocketColor.Red)}, false)}
                },
                new Item
                {
                    BaseType = "BlahdeBlah",
                    ItemClass = "Wands",
                    ItemRarity = ItemRarity.Magic,
                    ItemLevel = 9,
                    DropLevel = 9,
                    Height = 3,
                    Width = 1,
                    SocketGroups = new List<SocketGroup> {new SocketGroup(new List<Socket> {new Socket(SocketColor.Red)}, false)}
                },
                new Item
                {
                    BaseType = "BlahdeBlah",
                    ItemClass = "Wands",
                    ItemRarity = ItemRarity.Magic,
                    ItemLevel = 9,
                    DropLevel = 9,
                    Height = 3,
                    Width = 1,
                    SocketGroups = new List<SocketGroup> {new SocketGroup(new List<Socket> {new Socket(SocketColor.Red)}, false)}
                },
                new Item
                {
                    BaseType = "BlahdeBlah",
                    ItemClass = "Wands",
                    ItemRarity = ItemRarity.Magic,
                    ItemLevel = 9,
                    DropLevel = 9,
                    Height = 3,
                    Width = 1,
                    SocketGroups = new List<SocketGroup> {new SocketGroup(new List<Socket> {new Socket(SocketColor.Red)}, false)}
                },
                new Item
                {
                    BaseType = "BlahdeBlah",
                    ItemClass = "Wands",
                    ItemRarity = ItemRarity.Magic,
                    ItemLevel = 9,
                    DropLevel = 9,
                    Height = 3,
                    Width = 1,
                    SocketGroups = new List<SocketGroup> {new SocketGroup(new List<Socket> {new Socket(SocketColor.Red)}, false)}
                },
                new Item
                {
                    BaseType = "BlahdeBlah",
                    ItemClass = "Wands",
                    ItemRarity = ItemRarity.Magic,
                    ItemLevel = 9,
                    DropLevel = 9,
                    Height = 3,
                    Width = 1,
                    SocketGroups = new List<SocketGroup> {new SocketGroup(new List<Socket> {new Socket(SocketColor.Red)}, false)}
                },
                new Item
                {
                    BaseType = "BlahdeBlah",
                    ItemClass = "Wands",
                    ItemRarity = ItemRarity.Magic,
                    ItemLevel = 9,
                    DropLevel = 9,
                    Height = 3,
                    Width = 1,
                    SocketGroups = new List<SocketGroup> {new SocketGroup(new List<Socket> {new Socket(SocketColor.Red)}, false)}
                },
                new Item
                {
                    BaseType = "BlahdeBlah",
                    ItemClass = "Wands",
                    ItemRarity = ItemRarity.Magic,
                    ItemLevel = 9,
                    DropLevel = 9,
                    Height = 3,
                    Width = 1,
                    SocketGroups = new List<SocketGroup> {new SocketGroup(new List<Socket> {new Socket(SocketColor.Red)}, false)}
                },
                new Item
                {
                    BaseType = "BlahdeBlah",
                    ItemClass = "Wands",
                    ItemRarity = ItemRarity.Magic,
                    ItemLevel = 9,
                    DropLevel = 9,
                    Height = 3,
                    Width = 1,
                    SocketGroups = new List<SocketGroup> {new SocketGroup(new List<Socket> {new Socket(SocketColor.Red)}, false)}
                }
            };

            var itemFilterProcessor = new ItemFilterProcessor(new BlockItemMatcher());

            //Act
            var result = itemFilterProcessor.ProcessItemsAgainstItemFilterScript(script, testInputItems);

            //Assert
            Assert.AreEqual("Wands", result.First().ItemFilterBlock.BlockItems.OfType<ClassBlockItem>().First().Items.First());
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
