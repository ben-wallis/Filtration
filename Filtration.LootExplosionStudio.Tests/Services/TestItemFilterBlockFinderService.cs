using Filtration.LootExplosionStudio.Services;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.LootExplosionStudio;
using NUnit.Framework;

namespace Filtration.LootExplosionStudio.Tests.Services
{
    [TestFixture]
    public class TestItemFilterBlockFinderService
    {
        private ItemFilterBlockFinderServiceTestUtility _testUtility;

        [SetUp]
        public void ItemFilterProcessingServiceTestSetup()
        {
            _testUtility = new ItemFilterBlockFinderServiceTestUtility();
        }

        [Test]
        public void FindBlockForLootItem_SingleBlock_BaseType_Matches()
        {
            // Arrange
            var testInputBaseType = "TestBaseType";
            var testInputBlockItem = new BaseTypeBlockItem();
            testInputBlockItem.Items.Add(testInputBaseType);

            _testUtility.TestLootItem.BaseType = testInputBaseType;
            _testUtility.TestBlock.BlockItems.Add(testInputBlockItem);


            // Act
            var result = _testUtility.Service.FindBlockForLootItem(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(_testUtility.TestBlock, result);
        }

        [Test]
        public void FindBlockForLootItem_SingleHideBlock_Matches()
        {
            // Arrange

            _testUtility.TestBlock.Action = BlockAction.Hide;

            // Act
            var result = _testUtility.Service.FindBlockForLootItem(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(_testUtility.TestBlock, result);
        }

        [Test]
        public void FindBlockForLootItem_SingleBlock_MultipleBlockItems_Matches()
        {
            // Arrange
            var testInputBaseType = "TestBaseType";
            var testInputBaseTypeBlockItem = new BaseTypeBlockItem();
            testInputBaseTypeBlockItem.Items.Add(testInputBaseType);

            var testInputClass = "Test Class";
            var testInputClassBlockItem = new ClassBlockItem();
            testInputClassBlockItem.Items.Add(testInputClass);

            var testInputItemLevel = 57;
            var testInputItemLevelBlockItem = new ItemLevelBlockItem(FilterPredicateOperator.GreaterThan, 50);

            _testUtility.TestLootItem.BaseType = testInputBaseType;
            _testUtility.TestLootItem.Class = testInputClass;
            _testUtility.TestLootItem.ItemLevel = testInputItemLevel;

            _testUtility.TestBlock.BlockItems.Add(testInputBaseTypeBlockItem);
            _testUtility.TestBlock.BlockItems.Add(testInputClassBlockItem);
            _testUtility.TestBlock.BlockItems.Add(testInputItemLevelBlockItem);


            // Act
            var result = _testUtility.Service.FindBlockForLootItem(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(_testUtility.TestBlock, result);
        }



        [Test]
        public void FindBlockForLootItem_SingleBlock_MultipleBlockItemsOneWithoutMatch_Matches()
        {
            // Arrange
            var testInputBaseType = "TestBaseType";
            var testInputBaseTypeBlockItem = new BaseTypeBlockItem();
            testInputBaseTypeBlockItem.Items.Add(testInputBaseType);

            var testInputClass = "Test Class";
            var testInputClassBlockItem = new ClassBlockItem();
            testInputClassBlockItem.Items.Add(testInputClass);

            var testInputItemLevel = 57;
            var testInputItemLevelBlockItem = new ItemLevelBlockItem(FilterPredicateOperator.GreaterThan, 50);

            var testInputDropLevel = 35;
            var testInputDropLevelBlockItem = new DropLevelBlockItem(FilterPredicateOperator.GreaterThanOrEqual, 89);

            _testUtility.TestLootItem.BaseType = testInputBaseType;
            _testUtility.TestLootItem.Class = testInputClass;
            _testUtility.TestLootItem.ItemLevel = testInputItemLevel;
            _testUtility.TestLootItem.DropLevel = testInputDropLevel;

            _testUtility.TestBlock.BlockItems.Add(testInputBaseTypeBlockItem);
            _testUtility.TestBlock.BlockItems.Add(testInputClassBlockItem);
            _testUtility.TestBlock.BlockItems.Add(testInputItemLevelBlockItem);
            _testUtility.TestBlock.BlockItems.Add(testInputDropLevelBlockItem);

            // Act
            var result = _testUtility.Service.FindBlockForLootItem(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void FindBlockForLootItem_MultipleBlocksBlock_Matches()
        {
            // Arrange
            var testInputBlock1 = new ItemFilterBlock();
            var testInputClass1 = "Test Class";
            var testInputClassBlockItem1 = new ClassBlockItem();
            testInputClassBlockItem1.Items.Add(testInputClass1);
            testInputBlock1.BlockItems.Add(testInputClassBlockItem1);

            _testUtility.TestScript.ItemFilterBlocks.Add(testInputBlock1);

            var testInputBaseType = "TestBaseType";
            var testInputBaseTypeBlockItem = new BaseTypeBlockItem();
            testInputBaseTypeBlockItem.Items.Add(testInputBaseType);

            var testInputClass = "Test Class";
            var testInputClassBlockItem = new ClassBlockItem();
            testInputClassBlockItem.Items.Add(testInputClass);

            var testInputItemLevel = 57;
            var testInputItemLevelBlockItem = new ItemLevelBlockItem(FilterPredicateOperator.GreaterThan, 50);
            
            _testUtility.TestLootItem.BaseType = testInputBaseType;
            _testUtility.TestLootItem.Class = testInputClass;
            _testUtility.TestLootItem.ItemLevel = testInputItemLevel;

            _testUtility.TestBlock.BlockItems.Add(testInputBaseTypeBlockItem);
            _testUtility.TestBlock.BlockItems.Add(testInputClassBlockItem);
            _testUtility.TestBlock.BlockItems.Add(testInputItemLevelBlockItem);

            // Act
            var result = _testUtility.Service.FindBlockForLootItem(_testUtility.TestLootItem, _testUtility.TestScript);

            // Assert
            Assert.AreEqual(_testUtility.TestBlock, result);
        }

        private class ItemFilterBlockFinderServiceTestUtility
        {
            public ItemFilterBlockFinderServiceTestUtility()
            {
                TestBlock = new ItemFilterBlock();
                TestScript = new ItemFilterScript();
                TestScript.ItemFilterBlocks.Add(TestBlock);
                TestLootItem = new LootItem();

                Service = new ItemFilterBlockFinderService();
            }

            public ItemFilterScript TestScript { get; private set; }
            public ItemFilterBlock TestBlock { get; private set; }
            public LootItem TestLootItem { get; private set; }
            public ItemFilterBlockFinderService Service { get; private set; }
        }
    }
}
