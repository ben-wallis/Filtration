using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Filtration.ItemFilterPreview.Model;
using Filtration.ItemFilterPreview.Services;
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

        [TestCase("Test Base Type", true)]
        [TestCase("Test Bas", true)]
        [TestCase("T", true)]
        [TestCase("Base Type", false)]
        public void BaseTypeBlockItemMatch_SingleBlockItemValue_ReturnsTrue(string testInputBaseType, bool expectedResult)
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.BaseType == "Test Base Type");
            var testInputBaseTypeBlockItem = new BaseTypeBlockItem();
            testInputBaseTypeBlockItem.Items.Add(testInputBaseType);

            //Act
            var result = _testUtility.ItemBlockItemMatcher.BaseTypeBlockItemMatch(testInputBaseTypeBlockItem, testInputItem);

            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("Test Base Type", true)]
        [TestCase("Test Bas", true)]
        [TestCase("T", true)]
        [TestCase("Base Type", false)]
        public void BaseTypeBlockItemMatch_MultipleBlockItemValues_ReturnsCorrectResult(string testInputBaseType, bool expectedResult)
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.BaseType == "Test Base Type");
            var testInputBlockItem = new BaseTypeBlockItem();
            testInputBlockItem.Items.Add("Something else");
            testInputBlockItem.Items.Add(testInputBaseType);
            testInputBlockItem.Items.Add("Blah");

            //Act
            var result = _testUtility.ItemBlockItemMatcher.BaseTypeBlockItemMatch(testInputBlockItem, testInputItem);

            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("Test Item Class", true)]
        [TestCase("Test It", true)]
        [TestCase("T", true)]
        [TestCase("Item Class", false)]
        public void ItemClassBlockItemMatch_SingleBlockItemValue_ReturnsCorrectResult(string testInputBlockItemItemClass, bool expectedResult)
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.ItemClass == "Test Item Class");
            var testInputBlockItem = new ClassBlockItem();
            testInputBlockItem.Items.Add(testInputBlockItemItemClass);

            //Act
            var result = _testUtility.ItemBlockItemMatcher.ClassBlockItemMatch(testInputBlockItem, testInputItem);

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
        public void DropLevelBlockItemMatch_ReturnsCorrectResult(FilterPredicateOperator testInputFilterPredicateOperator, int testInputBlockItemDropLevel, bool expectedResult)
        {
            //Arrange
            var testInputItem = Mock.Of<IItem>(i => i.DropLevel == 50);
            var testInputBlockItem = new DropLevelBlockItem(testInputFilterPredicateOperator, testInputBlockItemDropLevel);

            //Act
            var result = _testUtility.ItemBlockItemMatcher.DropLevelBlockItemMatch(testInputBlockItem, testInputItem);
            
            //Assert
            Assert.AreEqual(expectedResult, result);
        }


        private class ItemBlockItemMatcherTestUtility
        {
            public ItemBlockItemMatcherTestUtility()
            {
                // Mock setups

                // Class under-test instantiation
                ItemBlockItemMatcher = new ItemBlockItemMatcher();
            }

            public ItemBlockItemMatcher ItemBlockItemMatcher { get; private set; }
        }
    }
}
