using System;
using System.IO;
using System.Linq;
using Filtration.Enums;
using Filtration.Models;
using Filtration.Models.BlockItemBaseTypes;
using Filtration.Models.BlockItemTypes;
using Filtration.Translators;
using Moq;
using NUnit.Framework;

namespace Filtration.Tests.Translators
{
    [TestFixture]
    public class TestLootFilterScriptTranslator
    {
        [Test]
        public void TranslateStringToLootFilterScript_ReturnsScriptWithCorrectNumberOfBlocks()
        {
            // Arrange
            var testInput = File.ReadAllText(@"Resources/testscript.txt");

            var mockLootFilterBlockTranslator = new Mock<ILootFilterBlockTranslator>();
            mockLootFilterBlockTranslator.Setup(t => t.TranslateStringToLootFilterBlock(It.IsAny<string>())).Verifiable();

            var translator = new LootFilterScriptTranslator(mockLootFilterBlockTranslator.Object);

            // Act
            var script = translator.TranslateStringToLootFilterScript(testInput);

            // Assert
            Assert.AreEqual(5, script.LootFilterBlocks.Count);
            mockLootFilterBlockTranslator.Verify();
        }

        [Test]
        public void TranslateStringToLootFilterScript_ReturnsScriptWithDescriptionCorrectlySet()
        {
            // Arrange
            var testInput = File.ReadAllText(@"Resources/testscript.txt");
            var expectedDescription =   "Loot Filter Script created by Filtration v0.1 - www.github.com/XVar/filtration" + Environment.NewLine +
                                        "Begin Script Description" + Environment.NewLine +
                                        "This is a test script" + Environment.NewLine +
                                        Environment.NewLine +
                                        "End Script Description";

            var mockLootFilterBlockTranslator = new Mock<ILootFilterBlockTranslator>();
            mockLootFilterBlockTranslator.Setup(t => t.TranslateStringToLootFilterBlock(It.IsAny<string>())).Verifiable();

            var translator = new LootFilterScriptTranslator(mockLootFilterBlockTranslator.Object);

            // Act
            var script = translator.TranslateStringToLootFilterScript(testInput);

            // Assert
            Assert.AreEqual(expectedDescription, script.Description);
        }

        [Ignore("Integration Test")]
        [Test]
        public void TranslateStringToLootFilterScript_ThioleLootFilterTest()
        {
            // Arrange
            var testInput = File.ReadAllText(@"Resources/ThioleLootFilter.txt");

            
            var BlockTranslator = new LootFilterBlockTranslator();
            var translator = new LootFilterScriptTranslator(BlockTranslator);

            // Act
            var script = translator.TranslateStringToLootFilterScript(testInput);
            // Assert
            // Not crashing out when loading a huge script means this integration test has passed!
        }

        [Test]
        public void TranslateLootFilterScriptToString_OneBlock_CallsTranslator()
        {
            // Arrange
            var testScript = new LootFilterScript();

            var testBlock = new LootFilterBlock();
            testBlock.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.Equal, 5));

            var BlockOutput = "Test Script Output";
            var expectedOutput = "Test Script Output" + Environment.NewLine + Environment.NewLine;

            testScript.LootFilterBlocks.Add(testBlock);

            var mockLootFilterBlockTranslator = new Mock<ILootFilterBlockTranslator>();
            mockLootFilterBlockTranslator.Setup(t => t.TranslateLootFilterBlockToString(testBlock)).Returns(BlockOutput).Verifiable();

            var translator = new LootFilterScriptTranslator(mockLootFilterBlockTranslator.Object);

            // Act
            var result = translator.TranslateLootFilterScriptToString(testScript);

            // Assert
            Assert.AreEqual(expectedOutput, result);
            mockLootFilterBlockTranslator.Verify();
        }

        [Test]
        public void TranslateLootFilterScriptToString_FullScript_ReturnsCorrectOutput()
        {
            var script = new LootFilterScript
            {
                Description = "Test script description" + Environment.NewLine + 
                              "This is a really great script!" + Environment.NewLine + 
                              "Multiple line script descriptions are fun!"
            };
            var block1 = new LootFilterBlock {Description = "Test Filter 1"};
            block1.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.GreaterThan, 5));

            var block2 = new LootFilterBlock();
            block2.BlockItems.Add(new QualityBlockItem(FilterPredicateOperator.LessThan, 15));
            block2.BlockItems.Add(new FontSizeBlockItem(7));
            block2.BlockItems.Add(new WidthBlockItem(FilterPredicateOperator.Equal, 3));

            script.LootFilterBlocks.Add(block1);
            script.LootFilterBlocks.Add(block2);

            var expectedOutput = "# Test script description" + Environment.NewLine +
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

            var blockTranslator = new LootFilterBlockTranslator();
            var translator = new LootFilterScriptTranslator(blockTranslator);

            // Act
            var result = translator.TranslateLootFilterScriptToString(script);

            // Assert
            Assert.AreEqual(expectedOutput, result);
        }

        [Test]
        public void TranslateStringToLootFilterScript_SectionDirectlyBeforeBlockWithoutDescription_ReturnsCorrectObject()
        {
            // Arrange
            var testInputScript = "# My Script" + Environment.NewLine +
                                  Environment.NewLine +
                                  "# Section: Chance Bases" + Environment.NewLine +
                                  "Show" + Environment.NewLine +
                                  "    BaseType \"Lapis Amulet\" \"Amber Amulet\"" + Environment.NewLine +
                                  "    SetBorderColor 255 0 255" + Environment.NewLine +
                                  "    SetFontSize 25";

            var blockTranslator = new LootFilterBlockTranslator();
            var translator = new LootFilterScriptTranslator(blockTranslator);

            // Act
            var result = translator.TranslateStringToLootFilterScript(testInputScript);

            // Assert
            Assert.AreEqual(2, result.LootFilterBlocks.Count);
            var block = result.LootFilterBlocks.First(l => l.GetType() != typeof(LootFilterSection));
            Assert.AreEqual(4, block.BlockItems.Count);
            var baseTypeItem = block.BlockItems.OfType<BaseTypeBlockItem>().First();
            Assert.AreEqual(2, baseTypeItem.Items.Count);

        }
    }
}
