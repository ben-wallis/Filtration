using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Factories;
using Filtration.ObjectModel.ThemeEditor;
using Filtration.Parser.Interface.Services;
using Filtration.Parser.Services;
using Filtration.Parser.Tests.Properties;
using Filtration.Properties;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Filtration.Parser.Tests.Services
{
    [TestFixture]
    public class TestItemFilterScriptTranslator
    {
        [SetUp]
        public void ItemFilterScriptTranslatorTestSetup()
        {
            Settings.Default.Reset();
        }

        [Test]
        public void TranslateStringToItemFilterScript_ReturnsScriptWithCorrectNumberOfBlocks()
        {
            // Arrange
            var testInput = Resources.testscript;
            
            var mockItemFilterBlockTranslator = new Mock<IItemFilterBlockTranslator>();
            var translator = CreateItemFilterScriptTranslator(itemFilterBlockTranslator: mockItemFilterBlockTranslator.Object);

            // Act
            var script = translator.TranslateStringToItemFilterScript(testInput);

            // Assert
            Assert.AreEqual(5, script.ItemFilterBlocks.Count);
            mockItemFilterBlockTranslator.Verify(t => t.TranslateStringToItemFilterBlock(It.IsAny<string>(), It.IsAny<IItemFilterScriptSettings>()));
        }

        [Test]
        public void TranslateStringToItemFilterScript_ReturnsScriptWithDescriptionCorrectlySet()
        {
            // Arrange
            var testInput = Resources.testscript;

            var expectedDescription =   "Item Filter Script created by Filtration v0.1 - www.github.com/XVar/filtration" + Environment.NewLine +
                                        "Begin Script Description" + Environment.NewLine +
                                        "This is a test script" + Environment.NewLine +
                                        Environment.NewLine +
                                        "End Script Description";

            var translator = CreateItemFilterScriptTranslator();

            // Act
            var script = translator.TranslateStringToItemFilterScript(testInput);

            // Assert
            Assert.AreEqual(expectedDescription, script.Description);
        }

        [Test]
        public void TranslateStringToItemFilterScript_ThioleItemFilterTest()
        {
            // Arrange
            var testInput = Resources.ThioleItemFilter;

            var blockTranslator = new ItemFilterBlockTranslator(Mock.Of<IBlockGroupHierarchyBuilder>());
            var translator = CreateItemFilterScriptTranslator(itemFilterBlockTranslator: blockTranslator);

            // Act
            translator.TranslateStringToItemFilterScript(testInput);

            // Assert
            // Not crashing out when loading a huge script means this integration test has passed!
        }

        [Test]
        public void TranslateStringToItemFilterScript_Blah()
        {
            // Arrange
            var testInput = Resources.testscript2;

            var blockTranslator = new ItemFilterBlockTranslator(Mock.Of<IBlockGroupHierarchyBuilder>());
            var translator = CreateItemFilterScriptTranslator(itemFilterBlockTranslator: blockTranslator);

            // Act
            var result = translator.TranslateStringToItemFilterScript(testInput);

            // Assert
            var expectedResult = Mock.Of<IItemFilterScript>(s => s.ItemFilterBlocks == new ObservableCollection<IItemFilterBlockBase>
            {
                Mock.Of<IItemFilterBlock>(c => c.Description == "Blockdescription"),
                Mock.Of<IItemFilterCommentBlock>(c => c.Comment == " commentymccommentface"),
                Mock.Of<IItemFilterBlock>(),
                Mock.Of<IItemFilterCommentBlock>(c => c.Comment == "commment\r\nmorecomment\r\nblah"),
                Mock.Of<IItemFilterCommentBlock>(c => c.Comment == "anothercomment"),
                Mock.Of<IItemFilterCommentBlock>(c => c.Comment == "notpartofblockdescription    "),
                Mock.Of<IItemFilterBlock>(c => c.Description == "blockdescription2")
            } && s.ItemFilterBlockGroups == new ObservableCollection<ItemFilterBlockGroup> { new ItemFilterBlockGroup("Root", null, false) }
            && s.ThemeComponents == new ThemeComponentCollection() 
            && s.ItemFilterScriptSettings == new ItemFilterScriptSettings(new ThemeComponentCollection())
            && s.Description == "Script description\r\nScript description\r\nScript description\r\nScript description");

            result.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void TranslateItemFilterScriptToString_OneBlock_CallsTranslator()
        {
            // Arrange
            var testScript = new ItemFilterScript();

            var testBlock = new ItemFilterBlock();
            testBlock.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.Equal, 5));

            const string blockOutput = "Test Script Output";

            testScript.ItemFilterBlocks.Add(testBlock);
            
            var mockItemFilterBlockTranslator = new Mock<IItemFilterBlockTranslator>();
            mockItemFilterBlockTranslator
                .Setup(t => t.TranslateItemFilterBlockToString(testBlock))
                .Returns(blockOutput)
                .Verifiable();

            var translator = CreateItemFilterScriptTranslator(itemFilterBlockTranslator: mockItemFilterBlockTranslator.Object);

            // Act
            translator.TranslateItemFilterScriptToString(testScript);

            // Assert
            mockItemFilterBlockTranslator.Verify();
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

            var blockTranslator = new ItemFilterBlockTranslator(Mock.Of<IBlockGroupHierarchyBuilder>());
            var translator = CreateItemFilterScriptTranslator(itemFilterBlockTranslator: blockTranslator);

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

            var blockTranslator = new ItemFilterBlockTranslator(Mock.Of<IBlockGroupHierarchyBuilder>());
            var translator = CreateItemFilterScriptTranslator(itemFilterBlockTranslator: blockTranslator);

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

            var translator = CreateItemFilterScriptTranslator();

            // Act
            var result = translator.TranslateItemFilterScriptToString(script);

            // Assert
            Assert.AreEqual(expectedOutput, result);
        }

        [Test]
        public void TranslateStringToItemFilterScript_OneLineDescriptionNoBlockDescriptionAddsDescriptionToScript()
        {
            // Arrange
            var testInputScript = "# Script edited with Filtration - https://github.com/ben-wallis/Filtration" +
                                  Environment.NewLine +
                                  "Show" + Environment.NewLine +
                                  "BaseType \"Maelström Staff\"" + Environment.NewLine + Environment.NewLine;
            var blockTranslator = new ItemFilterBlockTranslator(Mock.Of<IBlockGroupHierarchyBuilder>());
            var translator = CreateItemFilterScriptTranslator(itemFilterBlockTranslator: blockTranslator);

            // Act
            var result = translator.TranslateStringToItemFilterScript(testInputScript);

            // Assert
            Assert.AreEqual("Script edited with Filtration - https://github.com/ben-wallis/Filtration", result.Description);
            var firstBlock = result.ItemFilterBlocks.OfType<ItemFilterBlock>().First();
            Assert.IsNull(firstBlock.Description);
        }

        [Test]
        public void TranslateStringToItemFilterScript_DisabledBlock_ReturnsCorrectBlockCount()
        {
            // Arrange
            var testInputScript = "Show" + Environment.NewLine +
                                  "    ItemLevel > 2" + Environment.NewLine +
                                  "    SetTextColor 255 40 0" + Environment.NewLine +
                                  Environment.NewLine +
                                  "#Disabled Block Start" + Environment.NewLine +
                                  "#Show" + Environment.NewLine +
                                  "#    ItemLevel > 2" + Environment.NewLine +
                                  "#    SetTextColor 255 215 0" + Environment.NewLine +
                                  "#    SetBorderColor 255 105 180" + Environment.NewLine +
                                  "#    SetFontSize 32" + Environment.NewLine +
                                  "#Disabled Block End" + Environment.NewLine +
                                  Environment.NewLine +
                                  "Show" + Environment.NewLine +
                                  "   ItemLevel > 20" + Environment.NewLine +
                                  "    SetTextColor 255 255 0";


            var blockTranslator = new ItemFilterBlockTranslator(Mock.Of<IBlockGroupHierarchyBuilder>());
            var translator = CreateItemFilterScriptTranslator(itemFilterBlockTranslator: blockTranslator);

            // Act
            var result = translator.TranslateStringToItemFilterScript(testInputScript);

            // Assert
            Assert.AreEqual(3, result.ItemFilterBlocks.Count);
        }

        [Test]
        public void TranslateStringToItemFilterScript_DisabledBlock_ReturnsCorrectBlocks()
        {
            // Arrange
            var testInputScript = "Show" + Environment.NewLine +
                                  "    ItemLevel > 2" + Environment.NewLine +
                                  "    SetTextColor 255 40 0" + Environment.NewLine +
                                  Environment.NewLine +
                                  "#Disabled Block Start" + Environment.NewLine +
                                  "#Show" + Environment.NewLine +
                                  "#    ItemLevel > 2" + Environment.NewLine +
                                  "#    SetTextColor 255 215 0" + Environment.NewLine +
                                  "#    SetBorderColor 255 105 180" + Environment.NewLine +
                                  "#    SetFontSize 32" + Environment.NewLine +
                                  "#Disabled Block End" + Environment.NewLine +
                                  Environment.NewLine +
                                  "Show" + Environment.NewLine +
                                  "   ItemLevel > 20" + Environment.NewLine +
                                  "    SetTextColor 255 255 0";


            var blockTranslator = new ItemFilterBlockTranslator(Mock.Of<IBlockGroupHierarchyBuilder>());
            var translator = CreateItemFilterScriptTranslator(itemFilterBlockTranslator: blockTranslator);

            // Act
            var result = translator.TranslateStringToItemFilterScript(testInputScript);

            // Assert
            Assert.AreEqual(3, result.ItemFilterBlocks.Count);

            var firstBlock = result.ItemFilterBlocks.OfType<ItemFilterBlock>().First();
            var secondBlock = result.ItemFilterBlocks.OfType<ItemFilterBlock>().Skip(1).First();
            var thirdBlock = result.ItemFilterBlocks.OfType<ItemFilterBlock>().Skip(2).First();

            Assert.AreEqual(3, firstBlock.BlockItems.Count);
            Assert.AreEqual(5, secondBlock.BlockItems.Count);
            Assert.AreEqual(3, thirdBlock.BlockItems.Count);
        }

        [Test]
        public void TranslateStringToItemFilterScript_DisabledBlock_BlockDescriptionNotLost()
        {
            // Arrange
            var testInputScript = "Show" + Environment.NewLine +
                                  "    ItemLevel > 2" + Environment.NewLine +
                                  "    SetTextColor 255 40 0" + Environment.NewLine +
                                  Environment.NewLine +
                                  "#Disabled Block Start" + Environment.NewLine +
                                  "# This is a disabled block" + Environment.NewLine +
                                  "#Show" + Environment.NewLine +
                                  "#    ItemLevel > 2" + Environment.NewLine +
                                  "#Disabled Block End";


            var blockTranslator = new ItemFilterBlockTranslator(Mock.Of<IBlockGroupHierarchyBuilder>());
            var translator = CreateItemFilterScriptTranslator(itemFilterBlockTranslator: blockTranslator);

            // Act
            var result = translator.TranslateStringToItemFilterScript(testInputScript);

            // Assert
            Assert.AreEqual(2, result.ItemFilterBlocks.Count);
            var secondBlock = result.ItemFilterBlocks.OfType<ItemFilterBlock>().Skip(1).First();
            Assert.AreEqual("This is a disabled block", secondBlock.Description);
        }

        // TODO: Reinstate this test
        [Ignore("Ignored until toggling block group parsing can be controlled from the filter script input")]
        [Test]
        public void TranslateStringToItemFilterScript_DisabledBlockWithBlockGroup_ReturnsCorrectBlock()
        {
            // Arrange
            var testInputScript = "Show" + Environment.NewLine +
                                  "    ItemLevel > 2" + Environment.NewLine +
                                  "    SetTextColor 255 40 0" + Environment.NewLine +
                                  Environment.NewLine +
                                  "#Disabled Block Start" + Environment.NewLine +
                                  "# This is a disabled block" + Environment.NewLine +
                                  "#Show#My Block Group" + Environment.NewLine +
                                  "#    ItemLevel > 2" + Environment.NewLine +
                                  "#Disabled Block End";


            var mockBlockGroupHierarchyBuilder = new Mock<IBlockGroupHierarchyBuilder>();
            mockBlockGroupHierarchyBuilder.Setup(
                    b => b.IntegrateStringListIntoBlockGroupHierarchy(It.IsAny<IEnumerable<string>>()))
                .Returns(new ItemFilterBlockGroup("My Block Group", null));

            var blockTranslator = new ItemFilterBlockTranslator(mockBlockGroupHierarchyBuilder.Object);
            
            var translator = CreateItemFilterScriptTranslator(itemFilterBlockTranslator: blockTranslator);

            // Act
            var result = translator.TranslateStringToItemFilterScript(testInputScript);

            // Assert
            Assert.AreEqual(2, result.ItemFilterBlocks.Count);
            var secondBlock = result.ItemFilterBlocks.OfType<ItemFilterBlock>().Skip(1).First();
            Assert.AreEqual("This is a disabled block", secondBlock.Description);
            Assert.AreEqual("My Block Group", secondBlock.BlockGroup.GroupName);
        }

        private ItemFilterScriptTranslator CreateItemFilterScriptTranslator(IBlockGroupHierarchyBuilder blockGroupHierarchyBuilder = null,
                                                                            IItemFilterBlockTranslator itemFilterBlockTranslator = null,
                                                                            IItemFilterScriptFactory itemFilterScriptFactory = null)
        {
            var mockItemFilterScriptFactory = new Mock<IItemFilterScriptFactory>();
            mockItemFilterScriptFactory
                .Setup(i => i.Create())
                .Returns(new ItemFilterScript());

            return new ItemFilterScriptTranslator(blockGroupHierarchyBuilder ?? new Mock<IBlockGroupHierarchyBuilder>().Object,
                                                  itemFilterBlockTranslator ?? new Mock<IItemFilterBlockTranslator>().Object,
                                                  itemFilterScriptFactory ?? mockItemFilterScriptFactory.Object);
        }
    }
}
