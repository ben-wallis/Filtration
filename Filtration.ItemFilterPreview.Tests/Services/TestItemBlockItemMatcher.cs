using System.Collections.Generic;
using Filtration.ItemFilterPreview.Services;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Moq;
using NUnit.Framework;

namespace Filtration.ItemFilterPreview.Tests.Services
{
    [TestFixture]
    public class TestItemBlockItemMatcher
    {
        private ItemBlockItemMatcherTestUtility _testUtility;

        [SetUp]
        public void ItemBlockItemMatcherTestSetUp()
        {
            _testUtility = new ItemBlockItemMatcherTestUtility();
        }

        [Test]
        public void ItemBlockMatch_EmptyShowBlock_ReturnsTrue()
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>();
            var testInputBlock = new ItemFilterBlock();

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockMatch(testInputBlock, testInputItem);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ItemBlockMatch_SingleBlockItem_Matches_ReturnsTrue()
        {
            //Arrange
            var testBaseType = "Test Base Type";
            var testInputItem = Mock.Of<IItem>(i => i.BaseType == testBaseType);
            var testInputBlock = new ItemFilterBlock();
            var baseTypeBlockItem = new BaseTypeBlockItem();
            baseTypeBlockItem.Items.Add(testBaseType);
            testInputBlock.BlockItems.Add(baseTypeBlockItem);

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockMatch(testInputBlock, testInputItem);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ItemBlockMatch_SingleBlockItem_DoesNotMatche_ReturnsFalse()
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.BaseType == "Base Type 1");
            var testInputBlock = new ItemFilterBlock();
            var baseTypeBlockItem = new BaseTypeBlockItem();
            baseTypeBlockItem.Items.Add("Base Type 2");
            testInputBlock.BlockItems.Add(baseTypeBlockItem);

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockMatch(testInputBlock, testInputItem);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ItemBlockMatch_MultipleBlockItems_Matches_ReturnsTrue()
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.BaseType == "Base Type 1" && i.Height == 4 && i.Width == 2);
            var testInputBlock = new ItemFilterBlock();
            var baseTypeBlockItem = new BaseTypeBlockItem();
            baseTypeBlockItem.Items.Add("Base Type 1");
            var heightBlockItem = new HeightBlockItem(FilterPredicateOperator.Equal, 4);
            var widthBlockItem = new WidthBlockItem(FilterPredicateOperator.Equal, 2);

            testInputBlock.BlockItems.Add(baseTypeBlockItem);
            testInputBlock.BlockItems.Add(heightBlockItem);
            testInputBlock.BlockItems.Add(widthBlockItem);

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockMatch(testInputBlock, testInputItem);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ItemBlockMatch_MultipleBlockItems_DoesNotMatch_ReturnsFalse()
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.BaseType == "Base Type 1" && i.Height == 4 && i.Width == 2);
            var testInputBlock = new ItemFilterBlock();
            var baseTypeBlockItem = new BaseTypeBlockItem();
            baseTypeBlockItem.Items.Add("Base Type d");
            var heightBlockItem = new HeightBlockItem(FilterPredicateOperator.Equal, 3);
            var widthBlockItem = new WidthBlockItem(FilterPredicateOperator.Equal, 2);

            testInputBlock.BlockItems.Add(baseTypeBlockItem);
            testInputBlock.BlockItems.Add(heightBlockItem);
            testInputBlock.BlockItems.Add(widthBlockItem);

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockMatch(testInputBlock, testInputItem);

            //Assert
            Assert.IsFalse(result);
        }

        [TestCase("Test Base Type", true)]
        [TestCase("Test Bas", true)]
        [TestCase("T", true)]
        [TestCase("Base Type", false)]
        public void ItemBlockItemMatch_BaseTypeBlockItem_SingleBlockItemValue_ReturnsTrue(string testInputBaseType, bool expectedResult)
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.BaseType == "Test Base Type");
            var testInputBaseTypeBlockItem = new BaseTypeBlockItem();
            testInputBaseTypeBlockItem.Items.Add(testInputBaseType);

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockItemMatch(testInputBaseTypeBlockItem, testInputItem);

            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("Test Base Type", true)]
        [TestCase("Test Bas", true)]
        [TestCase("T", true)]
        [TestCase("Base Type", false)]
        public void ItemBlockItemMatch_BaseTypeBlockItem_MultipleBlockItemValues_ReturnsCorrectResult(string testInputBaseType, bool expectedResult)
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.BaseType == "Test Base Type");
            var testInputBlockItem = new BaseTypeBlockItem();
            testInputBlockItem.Items.Add("Something else");
            testInputBlockItem.Items.Add(testInputBaseType);
            testInputBlockItem.Items.Add("Blah");

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockItemMatch(testInputBlockItem, testInputItem);

            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("Test Item Class", true)]
        [TestCase("Test It", true)]
        [TestCase("T", true)]
        [TestCase("Carrots", false)]
        [TestCase("Item Class", true)]
        public void ItemBlockItemMatch_ClassBlockItem_SingleBlockItemValue_ReturnsCorrectResult(string testInputBlockItemItemClass, bool expectedResult)
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.ItemClass == "Test Item Class");
            var testInputBlockItem = new ClassBlockItem();
            testInputBlockItem.Items.Add(testInputBlockItemItemClass);

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockItemMatch(testInputBlockItem, testInputItem);

            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase(FilterPredicateOperator.Equal, 49, false)]
        [TestCase(FilterPredicateOperator.Equal, 50, true)]
        [TestCase(FilterPredicateOperator.Equal, 51, false)]
        [TestCase(FilterPredicateOperator.GreaterThan, 49, true)]
        [TestCase(FilterPredicateOperator.GreaterThan, 50, false)]
        [TestCase(FilterPredicateOperator.GreaterThan, 51, false)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 49, true)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 50, true)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 51, false)]
        [TestCase(FilterPredicateOperator.LessThan, 49, false)]
        [TestCase(FilterPredicateOperator.LessThan, 50, false)]
        [TestCase(FilterPredicateOperator.LessThan, 51, true)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 49, false)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 50, true)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 51, true)]
        [TestCase(-1, 51, false)]
        public void ItemBlockItemMatch_DropLevelBlockItem_ReturnsCorrectResult(FilterPredicateOperator testInputFilterPredicateOperator, int testInputBlockItemDropLevel, bool expectedResult)
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.DropLevel == 50);
            var testInputBlockItem = new DropLevelBlockItem(testInputFilterPredicateOperator, testInputBlockItemDropLevel);

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockItemMatch(testInputBlockItem, testInputItem);
            
            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase(FilterPredicateOperator.Equal, 1, false)]
        [TestCase(FilterPredicateOperator.Equal, 2, true)]
        [TestCase(FilterPredicateOperator.Equal, 3, false)]
        [TestCase(FilterPredicateOperator.GreaterThan, 1, true)]
        [TestCase(FilterPredicateOperator.GreaterThan, 2, false)]
        [TestCase(FilterPredicateOperator.GreaterThan, 3, false)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 1, true)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 2, true)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 3, false)]
        [TestCase(FilterPredicateOperator.LessThan, 1, false)]
        [TestCase(FilterPredicateOperator.LessThan, 2, false)]
        [TestCase(FilterPredicateOperator.LessThan, 3, true)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 1, false)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 2, true)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 3, true)]
        [TestCase(-1, 3, false)]
        public void ItemBlockItemMatch_HeightBlockItem_ReturnsCorrectResult(FilterPredicateOperator testInputFilterPredicateOperator, int testInputBlockItemHeight, bool expectedResult)
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.Height == 2);
            var testInputBlockItem = new HeightBlockItem(testInputFilterPredicateOperator, testInputBlockItemHeight);

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockItemMatch(testInputBlockItem, testInputItem);

            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase(FilterPredicateOperator.Equal, 49, false)]
        [TestCase(FilterPredicateOperator.Equal, 50, true)]
        [TestCase(FilterPredicateOperator.Equal, 51, false)]
        [TestCase(FilterPredicateOperator.GreaterThan, 49, true)]
        [TestCase(FilterPredicateOperator.GreaterThan, 50, false)]
        [TestCase(FilterPredicateOperator.GreaterThan, 51, false)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 49, true)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 50, true)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 51, false)]
        [TestCase(FilterPredicateOperator.LessThan, 49, false)]
        [TestCase(FilterPredicateOperator.LessThan, 50, false)]
        [TestCase(FilterPredicateOperator.LessThan, 51, true)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 49, false)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 50, true)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 51, true)]
        [TestCase(-1, 51, false)]
        public void ItemBlockItemMatch_ItemLevelBlockItem_ReturnsCorrectResult(FilterPredicateOperator testInputFilterPredicateOperator, int testInputBlockItemItemLevel, bool expectedResult)
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.ItemLevel == 50);
            var testInputBlockItem = new ItemLevelBlockItem(testInputFilterPredicateOperator, testInputBlockItemItemLevel);

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockItemMatch(testInputBlockItem, testInputItem);

            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase(FilterPredicateOperator.Equal, 2, false)]
        [TestCase(FilterPredicateOperator.Equal, 3, true)]
        [TestCase(FilterPredicateOperator.Equal, 4, false)]
        [TestCase(FilterPredicateOperator.GreaterThan, 2, true)]
        [TestCase(FilterPredicateOperator.GreaterThan, 3, false)]
        [TestCase(FilterPredicateOperator.GreaterThan, 4, false)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 2, true)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 3, true)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 4, false)]
        [TestCase(FilterPredicateOperator.LessThan, 2, false)]
        [TestCase(FilterPredicateOperator.LessThan, 3, false)]
        [TestCase(FilterPredicateOperator.LessThan, 4, true)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 2, false)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 3, true)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 4, true)]
        [TestCase(-1, 3, false)]
        public void ItemBlockItemMatch_LinkedSocketsBlockItem_ReturnsCorrectResult(FilterPredicateOperator testInputFilterPredicateOperator, int testInputBlockItemLinkedSockets, bool expectedResult)
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.LinkedSockets == 3);
            var testInputBlockItem = new LinkedSocketsBlockItem(testInputFilterPredicateOperator, testInputBlockItemLinkedSockets);

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockItemMatch(testInputBlockItem, testInputItem);

            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase(FilterPredicateOperator.Equal, 11, false)]
        [TestCase(FilterPredicateOperator.Equal, 12, true)]
        [TestCase(FilterPredicateOperator.Equal, 13, false)]
        [TestCase(FilterPredicateOperator.GreaterThan, 11, true)]
        [TestCase(FilterPredicateOperator.GreaterThan, 12, false)]
        [TestCase(FilterPredicateOperator.GreaterThan, 13, false)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 11, true)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 12, true)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 13, false)]
        [TestCase(FilterPredicateOperator.LessThan, 11, false)]
        [TestCase(FilterPredicateOperator.LessThan, 12, false)]
        [TestCase(FilterPredicateOperator.LessThan, 13, true)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 11, false)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 12, true)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 13, true)]
        [TestCase(-1, 13, false)]
        public void ItemBlockItemMatch_QualityBlockItem_ReturnsCorrectResult(FilterPredicateOperator testInputFilterPredicateOperator, int testInputBlockItemQuality, bool expectedResult)
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.Quality == 12);
            var testInputBlockItem = new QualityBlockItem(testInputFilterPredicateOperator, testInputBlockItemQuality);

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockItemMatch(testInputBlockItem, testInputItem);

            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase(FilterPredicateOperator.Equal, ItemRarity.Normal, false)]
        [TestCase(FilterPredicateOperator.Equal, ItemRarity.Magic , true)]
        [TestCase(FilterPredicateOperator.Equal, ItemRarity.Rare, false)]
        [TestCase(FilterPredicateOperator.GreaterThan, ItemRarity.Normal, true)]
        [TestCase(FilterPredicateOperator.GreaterThan, ItemRarity.Magic, false)]
        [TestCase(FilterPredicateOperator.GreaterThan, ItemRarity.Rare, false)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, ItemRarity.Normal, true)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, ItemRarity.Magic, true)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, ItemRarity.Rare, false)]
        [TestCase(FilterPredicateOperator.LessThan, ItemRarity.Normal, false)]
        [TestCase(FilterPredicateOperator.LessThan, ItemRarity.Magic, false)]
        [TestCase(FilterPredicateOperator.LessThan, ItemRarity.Rare, true)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, ItemRarity.Normal, false)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, ItemRarity.Magic, true)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, ItemRarity.Rare, true)]
        [TestCase(-1, 13, false)]
        public void ItemBlockItemMatch_RarityBlockItem_ReturnsCorrectResult(FilterPredicateOperator testInputFilterPredicateOperator, int testInputBlockItemRarity, bool expectedResult)
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.ItemRarity == ItemRarity.Magic);
            var testInputBlockItem = new RarityBlockItem(testInputFilterPredicateOperator, testInputBlockItemRarity);

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockItemMatch(testInputBlockItem, testInputItem);

            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase(FilterPredicateOperator.Equal, 2, false)]
        [TestCase(FilterPredicateOperator.Equal, 3, true)]
        [TestCase(FilterPredicateOperator.Equal, 4, false)]
        [TestCase(FilterPredicateOperator.GreaterThan, 2, true)]
        [TestCase(FilterPredicateOperator.GreaterThan, 3, false)]
        [TestCase(FilterPredicateOperator.GreaterThan, 4, false)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 2, true)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 3, true)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 4, false)]
        [TestCase(FilterPredicateOperator.LessThan, 2, false)]
        [TestCase(FilterPredicateOperator.LessThan, 3, false)]
        [TestCase(FilterPredicateOperator.LessThan, 4, true)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 2, false)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 3, true)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 4, true)]
        [TestCase(-1, 3, false)]
        public void ItemBlockItemMatch_SocketsBlockItem_ReturnsCorrectResult(FilterPredicateOperator testInputFilterPredicateOperator, int testInputBlockItemSockets, bool expectedResult)
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.SocketCount == 3);
            var testInputBlockItem = new SocketsBlockItem(testInputFilterPredicateOperator, testInputBlockItemSockets);

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockItemMatch(testInputBlockItem, testInputItem);

            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase(FilterPredicateOperator.Equal, 1, false)]
        [TestCase(FilterPredicateOperator.Equal, 2, true)]
        [TestCase(FilterPredicateOperator.Equal, 3, false)]
        [TestCase(FilterPredicateOperator.GreaterThan, 1, true)]
        [TestCase(FilterPredicateOperator.GreaterThan, 2, false)]
        [TestCase(FilterPredicateOperator.GreaterThan, 3, false)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 1, true)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 2, true)]
        [TestCase(FilterPredicateOperator.GreaterThanOrEqual, 3, false)]
        [TestCase(FilterPredicateOperator.LessThan, 1, false)]
        [TestCase(FilterPredicateOperator.LessThan, 2, false)]
        [TestCase(FilterPredicateOperator.LessThan, 3, true)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 1, false)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 2, true)]
        [TestCase(FilterPredicateOperator.LessThanOrEqual, 3, true)]
        [TestCase(-1, 3, false)]
        public void ItemBlockItemMatch_ReturnsCorrectResult(FilterPredicateOperator testInputFilterPredicateOperator, int testInputBlockItemWidth, bool expectedResult)
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.Width == 2);
            var testInputBlockItem = new WidthBlockItem(testInputFilterPredicateOperator, testInputBlockItemWidth);

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockItemMatch(testInputBlockItem, testInputItem);

            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void ItemBlockItemMatch_SocketGroupBlockItem_SingleItemSocketGroup_SingleBlockItemSocketGroup_Match_ReturnsCorrectResult()
        {
            //Arrange
            var testInputBlockItem = new SocketGroupBlockItem();
            testInputBlockItem.Items.Add("RGB");

            var testInputItem = Mock.Of<IItem>(i => i.LinkedSocketGroups == new List<SocketGroup>
            {
                new SocketGroup(new List<Socket>
                {
                    new Socket(SocketColor.Red),
                    new Socket(SocketColor.Green),
                    new Socket(SocketColor.Blue),
                }, true)
            });

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockItemMatch(testInputBlockItem, testInputItem);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ItemBlockItemMatch_SocketGroupBlockItem_SingleItemSocketGroup_SingleBlockItemSocketGroup_NoMatch_ReturnsCorrectResult()
        {
            //Arrange
            var testInputBlockItem = new SocketGroupBlockItem();
            testInputBlockItem.Items.Add("RGB");

            var testInputItem = Mock.Of<IItem>(i => i.LinkedSocketGroups == new List<SocketGroup>
            {
                new SocketGroup(new List<Socket>
                {
                    new Socket(SocketColor.Red),
                    new Socket(SocketColor.Green)
                }, true)
            });

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockItemMatch(testInputBlockItem, testInputItem);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ItemBlockItemMatch_SocketGroupBlockItem_MultipleItemSocketGroup_SingleBlockItemSocketGroup_NoMatch_ReturnsCorrectResult()
        {
            //Arrange
            var testInputBlockItem = new SocketGroupBlockItem();
            testInputBlockItem.Items.Add("RGB");
            testInputBlockItem.Items.Add("RGWW");
            testInputBlockItem.Items.Add("RRGG");
            testInputBlockItem.Items.Add("WWWW");

            var testInputItem = Mock.Of<IItem>(i => i.LinkedSocketGroups == new List<SocketGroup>
            {
                new SocketGroup(new List<Socket>
                {
                    new Socket(SocketColor.Red),
                    new Socket(SocketColor.Green),
                    new Socket(SocketColor.White),
                    new Socket(SocketColor.Green)

                }, true)
            });

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockItemMatch(testInputBlockItem, testInputItem);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ItemBlockItemMatch_SocketGroupBlockItem_MultipleItemSocketGroup_SingleBlockItemSocketGroup_Match_ReturnsCorrectResult()
        {
            //Arrange
            var testInputBlockItem = new SocketGroupBlockItem();
            testInputBlockItem.Items.Add("RGB");
            testInputBlockItem.Items.Add("RGWW");
            testInputBlockItem.Items.Add("RGWG");
            testInputBlockItem.Items.Add("WWWW");

            var testInputItem = Mock.Of<IItem>(i => i.LinkedSocketGroups == new List<SocketGroup>
            {
                new SocketGroup(new List<Socket>
                {
                    new Socket(SocketColor.Red),
                    new Socket(SocketColor.Green),
                    new Socket(SocketColor.White),
                    new Socket(SocketColor.Green)

                }, true)
            });

            //Act
            var result = _testUtility.BlockItemMatcher.ItemBlockItemMatch(testInputBlockItem, testInputItem);

            //Assert
            Assert.IsTrue(result);
        }

        private class ItemBlockItemMatcherTestUtility
        {
            public ItemBlockItemMatcherTestUtility()
            {
                // Mock setups

                // Class under-test instantiation
                BlockItemMatcher = new BlockItemMatcher();
            }

            public BlockItemMatcher BlockItemMatcher { get; }
        }
    }
}
