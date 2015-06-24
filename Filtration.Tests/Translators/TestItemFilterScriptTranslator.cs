using System;
using System.IO;
using System.Linq;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.Properties;
using Filtration.Translators;
using Moq;
using NUnit.Framework;

namespace Filtration.Tests.Translators
{
    [TestFixture]
    public class TestItemFilterScriptTranslator
    {
        private ItemFilterScriptTranslatorTestUtility _testUtility;

        [SetUp]
        public void ItemFilterScriptTranslatorTestSetup()
        {
            _testUtility = new ItemFilterScriptTranslatorTestUtility();
            Settings.Default.Reset();
        }

        [Test]
        public void TranslateStringToItemFilterScript_ReturnsScriptWithCorrectNumberOfBlocks()
        {
            // Arrange
            var testInput = File.ReadAllText(@"Resources/testscript.txt");

            _testUtility.MockItemFilterBlockTranslator.Setup(t => t.TranslateStringToItemFilterBlock(It.IsAny<string>())).Verifiable();

            // Act
            var script = _testUtility.ScriptTranslator.TranslateStringToItemFilterScript(testInput);

            // Assert
            Assert.AreEqual(5, script.ItemFilterBlocks.Count);
            _testUtility.MockItemFilterBlockTranslator.Verify();
        }

        [Test]
        public void TranslateStringToItemFilterScript_ReturnsScriptWithDescriptionCorrectlySet()
        {
            // Arrange
            var testInput = File.ReadAllText(@"Resources/testscript.txt");
            var expectedDescription =   "Item Filter Script created by Filtration v0.1 - www.github.com/XVar/filtration" + Environment.NewLine +
                                        "Begin Script Description" + Environment.NewLine +
                                        "This is a test script" + Environment.NewLine +
                                        Environment.NewLine +
                                        "End Script Description";

            var mockItemFilterBlockTranslator = new Mock<IItemFilterBlockTranslator>();
            mockItemFilterBlockTranslator.Setup(t => t.TranslateStringToItemFilterBlock(It.IsAny<string>())).Verifiable();

            // Act
            var script = _testUtility.ScriptTranslator.TranslateStringToItemFilterScript(testInput);

            // Assert
            Assert.AreEqual(expectedDescription, script.Description);
        }

        [Ignore("Integration Test")]
        [Test]
        public void TranslateStringToItemFilterScript_ThioleItemFilterTest()
        {
            // Arrange
            var testInput = File.ReadAllText(@"Resources/ThioleItemFilter.txt");


            var mockBlockGroupHierarchyBuilder = new Mock<IBlockGroupHierarchyBuilder>();
            var blockTranslator = new ItemFilterBlockTranslator(mockBlockGroupHierarchyBuilder.Object);
            var translator = new ItemFilterScriptTranslator(blockTranslator, mockBlockGroupHierarchyBuilder.Object);

            // Act
            translator.TranslateStringToItemFilterScript(testInput);
            // Assert
            // Not crashing out when loading a huge script means this integration test has passed!
        }

        [Test]
        public void TranslateItemFilterScriptToString_OneBlock_CallsTranslator()
        {
            // Arrange
            var testScript = new ItemFilterScript();

            var testBlock = new ItemFilterBlock();
            testBlock.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.Equal, 5));

            const string BlockOutput = "Test Script Output";

            testScript.ItemFilterBlocks.Add(testBlock);

            _testUtility.MockItemFilterBlockTranslator.Setup(t => t.TranslateItemFilterBlockToString(testBlock)).Returns(BlockOutput).Verifiable();


            // Act
            _testUtility.ScriptTranslator.TranslateItemFilterScriptToString(testScript);

            // Assert
            _testUtility.MockItemFilterBlockTranslator.Verify();
        }

        [Test]
        public void TranslateItemFilterScriptToString_ExtraLineBetweenBlocksSettingFalse_ReturnsCorrectOutput()
        {
            Settings.Default.ExtraLineBetweenBlocks = false;

            var script = new ItemFilterScript();
            var block1 = new ItemFilterBlock { Description = "Test Filter 1" };
            block1.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.GreaterThan, 5));

            var block2 = new ItemFilterBlock();
            block2.BlockItems.Add(new QualityBlockItem(FilterPredicateOperator.LessThan, 15));
            block2.BlockItems.Add(new FontSizeBlockItem(7));
            block2.BlockItems.Add(new WidthBlockItem(FilterPredicateOperator.Equal, 3));

            script.ItemFilterBlocks.Add(block1);
            script.ItemFilterBlocks.Add(block2);

            var expectedOutput = "# Script edited with Filtration - https://github.com/ben-wallis/Filtration" + Environment.NewLine +
                                 "# Test Filter 1" + Environment.NewLine +
                                 "Show" + Environment.NewLine +
                                 "    ItemLevel > 5" + Environment.NewLine +
                                 "Show" + Environment.NewLine +
                                 "    Quality < 15" + Environment.NewLine +
                                 "    Width = 3" + Environment.NewLine +
                                 "    SetFontSize 7" + Environment.NewLine;

            var mockBlockGroupHierarchyBuilder = new Mock<IBlockGroupHierarchyBuilder>();
            var blockTranslator = new ItemFilterBlockTranslator(mockBlockGroupHierarchyBuilder.Object);
            var translator = new ItemFilterScriptTranslator(blockTranslator, mockBlockGroupHierarchyBuilder.Object);

            // Act
            var result = translator.TranslateItemFilterScriptToString(script);

            // Assert
            Assert.AreEqual(expectedOutput, result);
        }

        [Test]
        public void TranslateItemFilterScriptToString_FullScript_ReturnsCorrectOutput()
        {
            var script = new ItemFilterScript
            {
                Description = "Test script description" + Environment.NewLine + 
                              "This is a really great script!" + Environment.NewLine + 
                              "Multiple line script descriptions are fun!"
            };
            var block1 = new ItemFilterBlock {Description = "Test Filter 1"};
            block1.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.GreaterThan, 5));

            var block2 = new ItemFilterBlock();
            block2.BlockItems.Add(new QualityBlockItem(FilterPredicateOperator.LessThan, 15));
            block2.BlockItems.Add(new FontSizeBlockItem(7));
            block2.BlockItems.Add(new WidthBlockItem(FilterPredicateOperator.Equal, 3));

            script.ItemFilterBlocks.Add(block1);
            script.ItemFilterBlocks.Add(block2);

            var expectedOutput = "# Script edited with Filtration - https://github.com/ben-wallis/Filtration" + Environment.NewLine +
                                 "# Test script description" + Environment.NewLine +
                                 "# This is a really great script!" + Environment.NewLine + 
                                 "# Multiple line script descriptions are fun!" + Environment.NewLine +
                                 Environment.NewLine +
                                 "# Test Filter 1" + Environment.NewLine +
                                 "Show" + Environment.NewLine +
                                 "    ItemLevel > 5" + Environment.NewLine +
                                 Environment.NewLine +
                                 "Show" + Environment.NewLine +
                                 "    Quality < 15" + Environment.NewLine +
                                 "    Width = 3" + Environment.NewLine +
                                 "    SetFontSize 7" + Environment.NewLine + Environment.NewLine;

            var mockBlockGroupHierarchyBuilder = new Mock<IBlockGroupHierarchyBuilder>();
            var blockTranslator = new ItemFilterBlockTranslator(mockBlockGroupHierarchyBuilder.Object);
            var translator = new ItemFilterScriptTranslator(blockTranslator, mockBlockGroupHierarchyBuilder.Object);

            // Act
            var result = translator.TranslateItemFilterScriptToString(script);

            // Assert
            Assert.AreEqual(expectedOutput, result);
        }
        
        [Test]
        public void TranslateItemFilterScriptToString_FullScriptWithExistingFiltrationTagline_ReturnsCorrectOutput()
        {
            var script = new ItemFilterScript
            {
                Description = "Script edited with Filtration - https://github.com/ben-wallis/Filtration" + Environment.NewLine +
                              "Test script description" + Environment.NewLine
            };

            var expectedOutput = "# Script edited with Filtration - https://github.com/ben-wallis/Filtration" +
                                 Environment.NewLine +
                                 "# Test script description" + Environment.NewLine + Environment.NewLine;

            // Act
            var result = _testUtility.ScriptTranslator.TranslateItemFilterScriptToString(script);

            // Assert
            Assert.AreEqual(expectedOutput, result);
        }

        [Test]
        public void TranslateStringToItemFilterScript_SectionDirectlyBeforeBlockWithoutDescription_ReturnsCorrectObject()
        {
            // Arrange
            var testInputScript = "# My Script" + Environment.NewLine +
                                  Environment.NewLine +
                                  "# Section: Chance Bases" + Environment.NewLine +
                                  "Show" + Environment.NewLine +
                                  "    BaseType \"Lapis Amulet\" \"Amber Amulet\"" + Environment.NewLine +
                                  "    SetBorderColor 255 0 255" + Environment.NewLine +
                                  "    SetFontSize 25";

            var blockTranslator = new ItemFilterBlockTranslator(_testUtility.MockBlockGroupHierarchyBuilder.Object);
            var translator = new ItemFilterScriptTranslator(blockTranslator, _testUtility.MockBlockGroupHierarchyBuilder.Object);

            // Act
            var result = translator.TranslateStringToItemFilterScript(testInputScript);

            // Assert
            Assert.AreEqual(2, result.ItemFilterBlocks.Count);
            var block = result.ItemFilterBlocks.First(l => l.GetType() != typeof(ItemFilterSection));
            Assert.AreEqual(4, block.BlockItems.Count);
            var baseTypeItem = block.BlockItems.OfType<BaseTypeBlockItem>().First();
            Assert.AreEqual(2, baseTypeItem.Items.Count);

        }

        private class ItemFilterScriptTranslatorTestUtility
        {
            public ItemFilterScriptTranslatorTestUtility()
            {
                // Mock setups
                MockItemFilterBlockTranslator = new Mock<IItemFilterBlockTranslator>();
                MockBlockGroupHierarchyBuilder = new Mock<IBlockGroupHierarchyBuilder>();

                // Class under test instantiation
                ScriptTranslator = new ItemFilterScriptTranslator(MockItemFilterBlockTranslator.Object, MockBlockGroupHierarchyBuilder.Object);
            }

            public ItemFilterScriptTranslator ScriptTranslator { get; private set; }
            public Mock<IItemFilterBlockTranslator> MockItemFilterBlockTranslator { get; private set; }
            public Mock<IBlockGroupHierarchyBuilder> MockBlockGroupHierarchyBuilder { get; private set; }
        }
    }
}
