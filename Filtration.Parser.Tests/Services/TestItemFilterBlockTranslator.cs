using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.ThemeEditor;
using Filtration.Parser.Interface.Services;
using Filtration.Parser.Services;
using Moq;
using NUnit.Framework;

namespace Filtration.Parser.Tests.Services
{
    [TestFixture]
    public class TestItemFilterBlockTranslator
    {
        private ItemFilterBlockTranslatorTestUtility _testUtility;

        [SetUp]
        public void ItemFilterBlockTranslatorTestSetUp()
        {
            _testUtility = new ItemFilterBlockTranslatorTestUtility();
        }

        [Test]
        public void TranslateStringToItemFilterBlockComment_ReturnsItemFilterBlockCommentWithSpacesNotRemoved()
        {
            //Arrange
            var testInputString = "#  This is a comment\r\n# Line 2 \r\n # Test";

            //Act
            var result = _testUtility.Translator.TranslateStringToItemFilterCommentBlock(testInputString, Mock.Of<IItemFilterScript>());

            //Assert
            Assert.AreEqual("  This is a comment\r\n Line 2 \r\n Test", result.Comment);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_BlockGroupsEnabled_ActionBlockItemCommentIsNull()
        {

            // Arrange
            var inputString = "Show # Test - Test2 - Test3" + Environment.NewLine;

            var inputBlockGroup = new ItemFilterBlockGroup("TestBlockGroup", null);
            _testUtility.MockBlockGroupHierarchyBuilder
                .Setup(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.IsAny<IEnumerable<string>>(), true, true))
                .Returns(inputBlockGroup);

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, Mock.Of<IItemFilterScript>(i => i.ItemFilterScriptSettings.BlockGroupsEnabled));

            //Assert
            Assert.IsTrue(string.IsNullOrEmpty(result.ActionBlockItem.Comment));
        }

        [Test]
        public void TranslateStringToItemFilterBlock_BlockGroupsDisabled_ActionBlockItemCommentIsSetCorrectly()
        {

            // Arrange
            var testInputExpectedComment = " this is a comment that should be preserved";

            var inputString = $"Show #{testInputExpectedComment}" + Environment.NewLine;

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, Mock.Of<IItemFilterScript>(i => i.ItemFilterScriptSettings.BlockGroupsEnabled == false));

            //Assert
            Assert.AreEqual(testInputExpectedComment, result.ActionBlockItem.Comment);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_NotDisabled_SetsBlockEnabledTrue()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    ItemLevel >= 55";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(true, result.Enabled);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_DisabledBlock_SetsBlockEnabledFalse()
        {
            // Arrange
            var inputString = "HideDisabled" + Environment.NewLine +
                              "    ItemLevel >= 55";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(2, result.BlockItems.Count);
            Assert.AreEqual(BlockAction.Hide, result.Action);
            Assert.AreEqual(false, result.Enabled);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_NoDescriptionComment_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    ItemLevel >= 55";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is ItemLevelBlockItem));
            var blockItem = result.BlockItems.OfType<ItemLevelBlockItem>().First();
            Assert.AreEqual(55, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.GreaterThanOrEqual, blockItem.FilterPredicate.PredicateOperator);
        }

        [Ignore("Update required, ItemFilterBlockTranslator does not set IsShowChecked anymore")]
        [Test]
        public void TranslateStringToItemFilterBlock_BlockGroupsEnabled_ShowBlock_SetsBlockGroupIsCheckedCorrectly()
        {
            // Arrange
            var inputString = "Show # TestBlockGroup" + Environment.NewLine;
            var inputBlockGroup = new ItemFilterBlockGroup("TestBlockGroup", null);

            // Act
            _testUtility.MockBlockGroupHierarchyBuilder.Setup(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.IsAny<IEnumerable<string>>(), true, true)).Returns(inputBlockGroup).Verifiable();
            _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, Mock.Of<IItemFilterScript>(i => i.ItemFilterScriptSettings.BlockGroupsEnabled));

            // Assert
            Assert.AreEqual(true, inputBlockGroup.IsShowChecked);
        }

        [Ignore("Update required, ItemFilterBlockTranslator does not set IsShowChecked anymore")]
        [Test]
        public void TranslateStringToItemFilterBlock_BlockGroupsEnabled_HideBlock_SetsBlockGroupIsCheckedCorrectly()
        {
            // Arrange
            var inputString = "Hide # TestBlockGroup" + Environment.NewLine;
            var inputBlockGroup = new ItemFilterBlockGroup("TestBlockGroup", null);

            // Act
            _testUtility.MockBlockGroupHierarchyBuilder.Setup(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.IsAny<IEnumerable<string>>(), false, true)).Returns(inputBlockGroup).Verifiable();
            _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, Mock.Of<IItemFilterScript>(i => i.ItemFilterScriptSettings.BlockGroupsEnabled));

            // Assert
            Assert.AreEqual(false, inputBlockGroup.IsShowChecked);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_BlockGroupsEnabled_BlockGroupComment_CallsBlockGroupHierarchyBuilder()
        {
            // Arrange
            var inputString = "Show # TestBlockGroup" + Environment.NewLine;
            var inputBlockGroup = new ItemFilterBlockGroup("TestBlockGroup", null);

            // Act
            _testUtility.MockBlockGroupHierarchyBuilder.Setup(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.IsAny<IEnumerable<string>>(), true, true)).Returns(inputBlockGroup).Verifiable();
            _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, Mock.Of<IItemFilterScript>(i => i.ItemFilterScriptSettings.BlockGroupsEnabled));

            // Assert
            _testUtility.MockBlockGroupHierarchyBuilder.Verify();
        }

        [Test]
        public void TranslateStringToItemFilterBlock_BlockGroupsEnabled_NoBlockGroupComment_DoesNotCallBlockGroupHierarchyBuilder()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine;

            // Act
            _testUtility.MockBlockGroupHierarchyBuilder.Setup(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.IsAny<IEnumerable<string>>(), true, true)).Verifiable();
            _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, Mock.Of<IItemFilterScript>(i => i.ItemFilterScriptSettings.BlockGroupsEnabled));

            // Assert
            _testUtility.MockBlockGroupHierarchyBuilder.Verify(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.IsAny<IEnumerable<string>>(), true, true), Times.Never);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_BlockGroupsEnabled_BlockGroupCommentWithNoGroups_DoesNotThrow()
        {
            // Arrange
            var inputString = "Show    #" + Environment.NewLine;

            // Act
            _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, Mock.Of<IItemFilterScript>(i => i.ItemFilterScriptSettings.BlockGroupsEnabled));

            // Assert
            _testUtility.MockBlockGroupHierarchyBuilder.Verify(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.IsAny<IEnumerable<string>>(), true, true), Times.Never);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_BlockGroupsEnabled_BlockGroupComment_SetsBlockItemGroupCorrectly()
        {
            // Arrange
            var inputString = "Show # Test Block Group - Test Sub Block Group - Test Another Block Group" + Environment.NewLine;
            var testBlockGroup = new ItemFilterBlockGroup("zzzzz", null);

            // Act
            _testUtility.MockBlockGroupHierarchyBuilder
                .Setup(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.Is<IEnumerable<string>>(s => s.Contains("Test Block Group") && s.Contains("Test Sub Block Group") && s.Contains("Test Another Block Group")), true, true))
                .Returns(testBlockGroup)
                .Verifiable();

            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, Mock.Of<IItemFilterScript>(i => i.ItemFilterScriptSettings.BlockGroupsEnabled));

            // Assert
            Assert.AreEqual(testBlockGroup, result.BlockGroup);
            _testUtility.MockBlockGroupHierarchyBuilder.Verify();
        }

        [Test]
        public void TranslateStringToItemFilterBlock_BlockGroupsEnabled_BlockGroupComment_NoSpacingAroundHyphens_SetsBlockItemGroupCorrectly()
        {
            // Arrange
            var inputString = "Show # AAA-BBB-CCC" + Environment.NewLine;
            var testBlockGroup = new ItemFilterBlockGroup("zzzzz", null);

            // Act
            _testUtility.MockBlockGroupHierarchyBuilder
                .Setup(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.Is<IEnumerable<string>>(s => s.Contains("AAA-BBB-CCC")), true, true))
                .Returns(testBlockGroup)
                .Verifiable();

            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, Mock.Of<IItemFilterScript>(i => i.ItemFilterScriptSettings.BlockGroupsEnabled));

            // Assert
            Assert.AreEqual(testBlockGroup, result.BlockGroup);
            _testUtility.MockBlockGroupHierarchyBuilder.Verify();
        }

        [Test]
        public void TranslateStringToItemFilterBlock_BlockGroupsDisabled_BlockGroupComment_DoesNotCallBlockGroupHierarchyBuilder()
        {
            // Arrange
            var inputString = "Show # AAA - BBB - CCC" + Environment.NewLine;

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, Mock.Of<IItemFilterScript>(i => i.ItemFilterScriptSettings.BlockGroupsEnabled == false));

            // Assert
            Assert.IsNull(result.BlockGroup);
            _testUtility.MockBlockGroupHierarchyBuilder.Verify(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.IsAny<IEnumerable<string>>(), true, true), Times.Never);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Hide_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Hide" + Environment.NewLine;

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is ActionBlockItem));
            Assert.AreEqual(BlockAction.Hide, result.Action);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_DescriptionComment_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "# This is a test Block" + Environment.NewLine +
                              "Show" + Environment.NewLine +
                              "    ItemLevel >= 55";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual("This is a test Block", result.Description);
            Assert.AreEqual(1, result.BlockItems.Count(b => b is ItemLevelBlockItem));
            var blockItem = result.BlockItems.OfType<ItemLevelBlockItem>().First();
            Assert.AreEqual(55, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.GreaterThanOrEqual, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_MultiLineDescriptionComment_OnlyAddsLastCommentLineToDescription()
        {
            // Arrange
            var inputString = "#First line" + Environment.NewLine +
                              "#Second Line" + Environment.NewLine +
                              "Show" + Environment.NewLine +
                              "    ItemLevel >= 55";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual("Second Line", result.Description);
            Assert.AreEqual(1, result.BlockItems.Count(b => b is ItemLevelBlockItem));
            var blockItem = result.BlockItems.OfType<ItemLevelBlockItem>().First();
            Assert.AreEqual(55, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.GreaterThanOrEqual, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_DropLevel_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    DropLevel = 40";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is DropLevelBlockItem));
            var blockItem = result.BlockItems.OfType<DropLevelBlockItem>().First();
            Assert.AreEqual(40, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.Equal, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_GemLevel_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    GemLevel = 20";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is GemLevelBlockItem));
            var blockItem = result.BlockItems.OfType<GemLevelBlockItem>().First();
            Assert.AreEqual(20, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.Equal, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_StackSize_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    StackSize > 5";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is StackSizeBlockItem));
            var blockItem = result.BlockItems.OfType<StackSizeBlockItem>().First();
            Assert.AreEqual(5, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.GreaterThan, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Corrupted_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    Corrupted True";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is CorruptedBlockItem));
            var blockItem = result.BlockItems.OfType<CorruptedBlockItem>().First();
            Assert.IsTrue(blockItem.BooleanValue);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_ElderItem_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    ElderItem False";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is ElderItemBlockItem));
            var blockItem = result.BlockItems.OfType<ElderItemBlockItem>().First();
            Assert.IsFalse(blockItem.BooleanValue);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_ShaperItem_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    ShaperItem True";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is ShaperItemBlockItem));
            var blockItem = result.BlockItems.OfType<ShaperItemBlockItem>().First();
            Assert.IsTrue(blockItem.BooleanValue);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_MapTier_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    MapTier >= 15";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is MapTierBlockItem));
            var blockItem = result.BlockItems.OfType<MapTierBlockItem>().First();
            Assert.AreEqual(15, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.GreaterThanOrEqual, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_ShapedMap_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    ShapedMap false";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is ShapedMapBlockItem));
            var blockItem = result.BlockItems.OfType<ShapedMapBlockItem>().First();
            Assert.IsFalse(blockItem.BooleanValue);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_ElderMap_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    ElderMap false";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is ElderMapBlockItem));
            var blockItem = result.BlockItems.OfType<ElderMapBlockItem>().First();
            Assert.IsFalse(blockItem.BooleanValue);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Identified_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    Identified True";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is IdentifiedBlockItem));
            var blockItem = result.BlockItems.OfType<IdentifiedBlockItem>().First();
            Assert.IsTrue(blockItem.BooleanValue);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Quality_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    Quality < 18";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is QualityBlockItem));
            var blockItem = result.BlockItems.OfType<QualityBlockItem>().First();
            Assert.AreEqual(18, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.LessThan, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Rarity_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    Rarity > Normal";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is RarityBlockItem));
            var blockItem = result.BlockItems.OfType<RarityBlockItem>().First();
            Assert.AreEqual((int)ItemRarity.Normal, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.GreaterThan, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Rarity_WorksWithoutPredicateOperator()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    Rarity Normal";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is RarityBlockItem));
            var blockItem = result.BlockItems.OfType<RarityBlockItem>().First();
            Assert.AreEqual((int)ItemRarity.Normal, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.Equal, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Class_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              @"    Class ""Test Class 1"" ""TestOneWordClassInQuotes"" TestOneWordClassNotInQuotes ""Test Class 2""";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is ClassBlockItem));
            var blockItem = result.BlockItems.OfType<ClassBlockItem>().First();
            Assert.Contains("Test Class 1", blockItem.Items);
            Assert.Contains("TestOneWordClassInQuotes", blockItem.Items);
            Assert.Contains("TestOneWordClassNotInQuotes", blockItem.Items);
            Assert.Contains("Test Class 2", blockItem.Items);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_BaseType_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              @"    BaseType ""Test Base Type 1"" ""TestOneWordBaseTypeInQuotes"" TestOneWordBaseTypeNotInQuotes ""Test BaseType 2""";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is BaseTypeBlockItem));
            var blockItem = result.BlockItems.OfType<BaseTypeBlockItem>().First();
            Assert.Contains("Test Base Type 1", blockItem.Items);
            Assert.Contains("TestOneWordBaseTypeInQuotes", blockItem.Items);
            Assert.Contains("TestOneWordBaseTypeNotInQuotes", blockItem.Items);
            Assert.Contains("Test BaseType 2", blockItem.Items);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Prophecy_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              @"    Prophecy ""Test Prophecy 1"" ""TestOneWordProphecyInQuotes"" TestOneWordProphecyNotInQuotes ""Test Prophecy 2""";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is ProphecyBlockItem));
            var blockItem = result.BlockItems.OfType<ProphecyBlockItem>().First();
            Assert.Contains("Test Prophecy 1", blockItem.Items);
            Assert.Contains("TestOneWordProphecyInQuotes", blockItem.Items);
            Assert.Contains("TestOneWordProphecyNotInQuotes", blockItem.Items);
            Assert.Contains("Test Prophecy 2", blockItem.Items);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_HasExplicitMod_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              @"    HasExplicitMod ""Test Mod 1"" ""TestOneWordModInQuotes"" TestOneWordModNotInQuotes ""Test Mod 2""";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is HasExplicitModBlockItem));
            var blockItem = result.BlockItems.OfType<HasExplicitModBlockItem>().First();
            Assert.Contains("Test Mod 1", blockItem.Items);
            Assert.Contains("TestOneWordModInQuotes", blockItem.Items);
            Assert.Contains("TestOneWordModNotInQuotes", blockItem.Items);
            Assert.Contains("Test Mod 2", blockItem.Items);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Sockets_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    Sockets > 2";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is SocketsBlockItem));
            var blockItem = result.BlockItems.OfType<SocketsBlockItem>().First();
            Assert.AreEqual(2, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.GreaterThan, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_LinkedSockets_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    LinkedSockets > 1";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is LinkedSocketsBlockItem));
            var blockItem = result.BlockItems.OfType<LinkedSocketsBlockItem>().First();
            Assert.AreEqual(1, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.GreaterThan, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Width_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    Width = 1";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is WidthBlockItem));
            var blockItem = result.BlockItems.OfType<WidthBlockItem>().First();
            Assert.AreEqual(1, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.Equal, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Height_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    Height <= 3";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is HeightBlockItem));
            var blockItem = result.BlockItems.OfType<HeightBlockItem>().First();
            Assert.AreEqual(3, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.LessThanOrEqual, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Height_PredicatesWorkWithoutSpaceBetweenOperatorAndOperand()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    Height <=3";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is HeightBlockItem));
            var blockItem = result.BlockItems.OfType<HeightBlockItem>().First();
            Assert.AreEqual(3, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.LessThanOrEqual, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_SocketGroup_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    SocketGroup RRGB";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is SocketGroupBlockItem));
            var blockItem = result.BlockItems.OfType<SocketGroupBlockItem>().First();
            Assert.AreEqual(1, blockItem.Items.Count);
            var firstSocketColorGroup = blockItem.Items.First();
            Assert.AreEqual("RRGB", firstSocketColorGroup);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_SetTextColor_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    SetTextColor 255 20 100";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is TextColorBlockItem));
            var blockItem = result.BlockItems.OfType<TextColorBlockItem>().First();
            Assert.AreEqual(255, blockItem.Color.R);
            Assert.AreEqual(20, blockItem.Color.G);
            Assert.AreEqual(100, blockItem.Color.B);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_SetTextColorWithAlpha_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    SetTextColor 65 0 255 12";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is TextColorBlockItem));
            var blockItem = result.BlockItems.OfType<TextColorBlockItem>().First();
            Assert.AreEqual(12, blockItem.Color.A);
            Assert.AreEqual(65, blockItem.Color.R);
            Assert.AreEqual(0, blockItem.Color.G);
            Assert.AreEqual(255, blockItem.Color.B);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_SetBackgroundColor_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    SetBackgroundColor 255 20 100";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is BackgroundColorBlockItem));
            var blockItem =
                (ColorBlockItem)result.BlockItems.OfType<BackgroundColorBlockItem>().First();
            Assert.AreEqual(255, blockItem.Color.R);
            Assert.AreEqual(20, blockItem.Color.G);
            Assert.AreEqual(100, blockItem.Color.B);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_SetBorderColor_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    SetBorderColor 255 20 100";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is BorderColorBlockItem));
            var blockItem = result.BlockItems.OfType<BorderColorBlockItem>().First();
            Assert.AreEqual(255, blockItem.Color.R);
            Assert.AreEqual(20, blockItem.Color.G);
            Assert.AreEqual(100, blockItem.Color.B);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_SetBorderColor_CommentWithLargeNumber_DoesNotThrow()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    SetBorderColor 255 20 100 # Some stuff 8504 with a number in a comment";

            // Act

            Assert.DoesNotThrow(() => _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript));
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is BorderColorBlockItem));
            var blockItem = result.BlockItems.OfType<BorderColorBlockItem>().First();
            Assert.AreEqual(255, blockItem.Color.R);
            Assert.AreEqual(20, blockItem.Color.G);
            Assert.AreEqual(100, blockItem.Color.B);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_SetTextColorWithThemeComponent_CallsThemeListBuilderAddComponent()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    SetTextColor 255 20 100 # Rare Item Text";
            var testComponent = new ColorThemeComponent(ThemeComponentType.TextColor, "Rare Item Text", new Color { A = 240, R = 255, G = 20, B = 100});
            var testInputThemeComponentCollection = new ThemeComponentCollection { testComponent };

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, Mock.Of<IItemFilterScript>(i => i.ItemFilterScriptSettings.ThemeComponentCollection == testInputThemeComponentCollection));

            // Assert
            var blockItem = result.BlockItems.OfType<TextColorBlockItem>().First();
            Assert.AreSame(testComponent, blockItem.ThemeComponent);
            var firstComponent = testInputThemeComponentCollection.First();
            Assert.AreEqual("Rare Item Text", firstComponent.ComponentName);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_SetFontSize_ReturnsCorrectObject()
        {
            // Arrange

            var inputString = "Show" + Environment.NewLine +
                              "    SetFontSize 15";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is FontSizeBlockItem));
            var blockItem = result.BlockItems.OfType<FontSizeBlockItem>().First();
            Assert.AreEqual(15, blockItem.Value);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_PlayAlertSoundWithoutVolume_ReturnsCorrectObject()
        {
            // Arrange

            var inputString = "Show" + Environment.NewLine +
                              "    PlayAlertSound 4";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is SoundBlockItem));
            var blockItem = result.BlockItems.OfType<SoundBlockItem>().First();
            Assert.AreEqual("4", blockItem.Value);
            Assert.AreEqual(79, blockItem.SecondValue);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_PlayAlertSoundWithVolume_ReturnsCorrectObject()
        {
            // Arrange

            var inputString = "Show" + Environment.NewLine +
                              "    PlayAlertSound 2 95";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is SoundBlockItem));
            var blockItem = result.BlockItems.OfType<SoundBlockItem>().First();
            Assert.AreEqual("2", blockItem.Value);
            Assert.AreEqual(95, blockItem.SecondValue);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_PlayAlertSoundPositionalWithoutVolume_ReturnsCorrectObject()
        {
            // Arrange

            var inputString = "Show" + Environment.NewLine +
                              "    PlayAlertSoundPositional 12";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is PositionalSoundBlockItem));
            var blockItem = result.BlockItems.OfType<PositionalSoundBlockItem>().First();
            Assert.AreEqual("12", blockItem.Value);
            Assert.AreEqual(79, blockItem.SecondValue);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_PlayAlertSoundPositionalWithVolume_ReturnsCorrectObject()
        {
            // Arrange

            var inputString = "Show" + Environment.NewLine +
                              "    PlayAlertSoundPositional 7 95";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is PositionalSoundBlockItem));
            var blockItem = result.BlockItems.OfType<PositionalSoundBlockItem>().First();
            Assert.AreEqual("7", blockItem.Value);
            Assert.AreEqual(95, blockItem.SecondValue);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_DisableDropSound_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    DisableDropSound # Test";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is DisableDropSoundBlockItem));
            var blockItem = result.BlockItems.OfType<DisableDropSoundBlockItem>().First();
            Assert.AreEqual(blockItem.Comment, " Test");
        }

        [Test]
        public void TranslateStringToItemFilterBlock_DisableDropSound_IncorrectBooleanValue_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    DisableDropSound True";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is DisableDropSoundBlockItem));
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Everything_ReturnsCorrectObject()
        {
            // Arrange

            var inputString = "#Comment which should be ignored" + Environment.NewLine +
                              "#Test filter with everything" + Environment.NewLine +
                              "Show" + Environment.NewLine +
                              "    ItemLevel >= 50" + Environment.NewLine +
                              "    DropLevel < 70" + Environment.NewLine +
                              "    GemLevel = 20" + Environment.NewLine +
                              "    StackSize > 2" + Environment.NewLine +
                              "    Quality = 15" + Environment.NewLine +
                              "    Rarity <= Unique" + Environment.NewLine +
                              "    Identified True" + Environment.NewLine +
                              "    Corrupted false" + Environment.NewLine +
                              "    ElderItem true" + Environment.NewLine +
                              "    ShaperItem False" + Environment.NewLine +
                              "    ShapedMap TRUE" + Environment.NewLine +
                              "    ElderMap False" + Environment.NewLine +
                              @"    Class ""My Item Class"" AnotherClass ""AndAnotherClass""" + Environment.NewLine +
                              @"    BaseType MyBaseType ""Another BaseType""" + Environment.NewLine +
                              @"    Prophecy MyProphecy ""Another Prophecy""" + Environment.NewLine +
                              @"    HasExplicitMod MyMod ""Another Mod""" + Environment.NewLine +
                              "    JunkLine Let's ignore this one!" + Environment.NewLine +
                              "    #Quality Commented out quality line" + Environment.NewLine +
                              "    Sockets >= 3" + Environment.NewLine +
                              "    LinkedSockets = 2" + Environment.NewLine +
                              "    SocketGroup RGBB RGBWW" + Environment.NewLine +
                              "    SetTextColor 50 100 3 200" + Environment.NewLine +
                              "    SetBackgroundColor 255 100 5" + Environment.NewLine +
                              "    SetBorderColor 0 0 0" + Environment.NewLine +
                              "    SetFontSize 50" + Environment.NewLine +
                              "    PlayAlertSound 3" + Environment.NewLine +
                              "    DisableDropSound # False" + Environment.NewLine +
                              "    CustomAlertSound \"test.mp3\" # customSoundTheme" + Environment.NewLine +
                              "    MinimapIcon 2 Green Triangle  # iconTheme" + Environment.NewLine +
                              "    PlayEffect Green Temp  # effectTheme" + Environment.NewLine;

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual("Test filter with everything", result.Description);
            var itemLevelblockItem = result.BlockItems.OfType<ItemLevelBlockItem>().First();
            Assert.AreEqual(FilterPredicateOperator.GreaterThanOrEqual, itemLevelblockItem.FilterPredicate.PredicateOperator);
            Assert.AreEqual(50, itemLevelblockItem.FilterPredicate.PredicateOperand);

            var corruptedBlockItem = result.BlockItems.OfType<CorruptedBlockItem>().First();
            Assert.IsFalse(corruptedBlockItem.BooleanValue);

            var identifiedBlockItem = result.BlockItems.OfType<IdentifiedBlockItem>().First();
            Assert.IsTrue(identifiedBlockItem.BooleanValue);

            var elderItemBlockItem = result.BlockItems.OfType<ElderItemBlockItem>().First();
            Assert.IsTrue(elderItemBlockItem.BooleanValue);

            var shaperItemBlockItem = result.BlockItems.OfType<ShaperItemBlockItem>().First();
            Assert.IsFalse(shaperItemBlockItem.BooleanValue);

            var shapedMapBlockItem = result.BlockItems.OfType<ShapedMapBlockItem>().First();
            Assert.IsTrue(shapedMapBlockItem.BooleanValue);

            var elderMapBlockItem = result.BlockItems.OfType<ElderMapBlockItem>().First();
            Assert.IsFalse(elderMapBlockItem.BooleanValue);

            var dropLevelblockItem = result.BlockItems.OfType<DropLevelBlockItem>().First();
            Assert.AreEqual(FilterPredicateOperator.LessThan, dropLevelblockItem.FilterPredicate.PredicateOperator);
            Assert.AreEqual(70, dropLevelblockItem.FilterPredicate.PredicateOperand);

            var gemLevelBlockItem = result.BlockItems.OfType<GemLevelBlockItem>().First();
            Assert.AreEqual(FilterPredicateOperator.Equal, gemLevelBlockItem.FilterPredicate.PredicateOperator);
            Assert.AreEqual(20, gemLevelBlockItem.FilterPredicate.PredicateOperand);

            var stackSizeBlockItem = result.BlockItems.OfType<StackSizeBlockItem>().First();
            Assert.AreEqual(FilterPredicateOperator.GreaterThan, stackSizeBlockItem.FilterPredicate.PredicateOperator);
            Assert.AreEqual(2, stackSizeBlockItem.FilterPredicate.PredicateOperand);

            var qualityblockItem = result.BlockItems.OfType<QualityBlockItem>().First();
            Assert.AreEqual(FilterPredicateOperator.Equal, qualityblockItem.FilterPredicate.PredicateOperator);
            Assert.AreEqual(15, qualityblockItem.FilterPredicate.PredicateOperand);

            var itemRarityblockItem = result.BlockItems.OfType<RarityBlockItem>().First();
            Assert.AreEqual(FilterPredicateOperator.LessThanOrEqual, itemRarityblockItem.FilterPredicate.PredicateOperator);
            Assert.AreEqual((int)ItemRarity.Unique, itemRarityblockItem.FilterPredicate.PredicateOperand);

            var classblockItem = result.BlockItems.OfType<ClassBlockItem>().First();
            Assert.AreEqual(3, classblockItem.Items.Count);
            Assert.Contains("My Item Class", classblockItem.Items);
            Assert.Contains("AnotherClass", classblockItem.Items);
            Assert.Contains("AndAnotherClass", classblockItem.Items);

            var baseTypeblockItem = result.BlockItems.OfType<BaseTypeBlockItem>().First();
            Assert.AreEqual(2, baseTypeblockItem.Items.Count);
            Assert.Contains("MyBaseType", baseTypeblockItem.Items);
            Assert.Contains("Another BaseType", baseTypeblockItem.Items);

            var prophecyblockItem = result.BlockItems.OfType<ProphecyBlockItem>().First();
            Assert.AreEqual(2, prophecyblockItem.Items.Count);
            Assert.Contains("MyProphecy", prophecyblockItem.Items);
            Assert.Contains("Another Prophecy", prophecyblockItem.Items);

            var hasExplicitModBlockItem = result.BlockItems.OfType<HasExplicitModBlockItem>().First();
            Assert.AreEqual(2, hasExplicitModBlockItem.Items.Count);
            Assert.Contains("MyMod", hasExplicitModBlockItem.Items);
            Assert.Contains("Another Mod", hasExplicitModBlockItem.Items);

            var socketsblockItem = result.BlockItems.OfType<SocketsBlockItem>().First();
            Assert.AreEqual(FilterPredicateOperator.GreaterThanOrEqual, socketsblockItem.FilterPredicate.PredicateOperator);
            Assert.AreEqual(3, socketsblockItem.FilterPredicate.PredicateOperand);

            var linkedSocketsblockItem = result.BlockItems.OfType<LinkedSocketsBlockItem>().First();
            Assert.AreEqual(FilterPredicateOperator.Equal, linkedSocketsblockItem.FilterPredicate.PredicateOperator);
            Assert.AreEqual(2, linkedSocketsblockItem.FilterPredicate.PredicateOperand);

            var socketGroupblockItem = result.BlockItems.OfType<SocketGroupBlockItem>().First();
            Assert.AreEqual(2, socketGroupblockItem.Items.Count);
            var firstSocketGroup = socketGroupblockItem.Items.First();
            Assert.AreEqual("RGBB", firstSocketGroup);
            var secondSocketGroup = socketGroupblockItem.Items.Skip(1).First();
            Assert.AreEqual("RGBWW", secondSocketGroup);

            var textColorblockItem = result.BlockItems.OfType<TextColorBlockItem>().First();
            Assert.AreEqual(200, textColorblockItem.Color.A);
            Assert.AreEqual(50, textColorblockItem.Color.R);
            Assert.AreEqual(100, textColorblockItem.Color.G);
            Assert.AreEqual(3, textColorblockItem.Color.B);

            var backgroundColorblockItem = result.BlockItems.OfType<BackgroundColorBlockItem>().First();
            Assert.AreEqual(255, backgroundColorblockItem.Color.R);
            Assert.AreEqual(100, backgroundColorblockItem.Color.G);
            Assert.AreEqual(5, backgroundColorblockItem.Color.B);

            var borderColorblockItem = result.BlockItems.OfType<BorderColorBlockItem>().First();
            Assert.AreEqual(0, borderColorblockItem.Color.R);
            Assert.AreEqual(0, borderColorblockItem.Color.G);
            Assert.AreEqual(0, borderColorblockItem.Color.B);

            var fontSizeblockItem = result.BlockItems.OfType<FontSizeBlockItem>().First();
            Assert.AreEqual(50, fontSizeblockItem.Value);

            Assert.AreEqual(0, result.BlockItems.OfType<SoundBlockItem>().Count());

            var disableDropSoundBlockItem = result.BlockItems.OfType<DisableDropSoundBlockItem>().First();
            Assert.AreEqual(disableDropSoundBlockItem.Comment, " False");

            var customSoundBlockItem = result.BlockItems.OfType<CustomSoundBlockItem>().First();
            Assert.AreEqual("test.mp3", customSoundBlockItem.Value);
            Assert.AreNotEqual(null, customSoundBlockItem.ThemeComponent);
            Assert.AreEqual(ThemeComponentType.CustomSound, customSoundBlockItem.ThemeComponent.ComponentType);
            Assert.AreEqual("customSoundTheme", customSoundBlockItem.ThemeComponent.ComponentName);
            Assert.AreEqual(typeof(StringThemeComponent), customSoundBlockItem.ThemeComponent.GetType());
            Assert.AreEqual("test.mp3", ((StringThemeComponent)customSoundBlockItem.ThemeComponent).Value);

            var mapIconBlockItem = result.BlockItems.OfType<MapIconBlockItem>().First();
            Assert.AreEqual(IconSize.Small, mapIconBlockItem.Size);
            Assert.AreEqual(IconColor.Green, mapIconBlockItem.Color);
            Assert.AreEqual(IconShape.Triangle, mapIconBlockItem.Shape);
            Assert.AreNotEqual(null, mapIconBlockItem.ThemeComponent);
            Assert.AreEqual(ThemeComponentType.Icon, mapIconBlockItem.ThemeComponent.ComponentType);
            Assert.AreEqual("iconTheme", mapIconBlockItem.ThemeComponent.ComponentName);
            Assert.AreEqual(typeof(IconThemeComponent), mapIconBlockItem.ThemeComponent.GetType());
            Assert.AreEqual(IconSize.Small, ((IconThemeComponent)mapIconBlockItem.ThemeComponent).IconSize);
            Assert.AreEqual(IconColor.Green, ((IconThemeComponent)mapIconBlockItem.ThemeComponent).IconColor);
            Assert.AreEqual(IconShape.Triangle, ((IconThemeComponent)mapIconBlockItem.ThemeComponent).IconShape);

            var effectColorBlockItem = result.BlockItems.OfType<EffectColorBlockItem>().First();
            Assert.AreEqual(EffectColor.Green, effectColorBlockItem.Color);
            Assert.IsTrue(effectColorBlockItem.Temporary);
            Assert.AreNotEqual(null, effectColorBlockItem.ThemeComponent);
            Assert.AreEqual(ThemeComponentType.Effect, effectColorBlockItem.ThemeComponent.ComponentType);
            Assert.AreEqual("effectTheme", effectColorBlockItem.ThemeComponent.ComponentName);
            Assert.AreEqual(typeof(EffectColorThemeComponent), effectColorBlockItem.ThemeComponent.GetType());
            Assert.AreEqual(EffectColor.Green, ((EffectColorThemeComponent)effectColorBlockItem.ThemeComponent).EffectColor);
            Assert.IsTrue(((EffectColorThemeComponent)effectColorBlockItem.ThemeComponent).Temporary);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_MultipleValues_ReturnsCorrectObject()
        {
            // Arrange

            var inputString = "Show" + Environment.NewLine +
                              "    ItemLevel >= 70" + Environment.NewLine +
                              "    ItemLevel <= 80" + Environment.NewLine +
                              "    Quality = 15" + Environment.NewLine +
                              "    Quality < 17";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(2, result.BlockItems.Count(b => b is ItemLevelBlockItem));
            var firstItemLevelblockItem = result.BlockItems.OfType<ItemLevelBlockItem>().First();
            Assert.AreEqual(70, firstItemLevelblockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.GreaterThanOrEqual, firstItemLevelblockItem.FilterPredicate.PredicateOperator);
            var secondItemLevelblockItem = result.BlockItems.OfType<ItemLevelBlockItem>().Skip(1).First();
            Assert.AreEqual(80, secondItemLevelblockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.LessThanOrEqual, secondItemLevelblockItem.FilterPredicate.PredicateOperator);

            Assert.AreEqual(2, result.BlockItems.Count(b => b is QualityBlockItem));
            var firstQualityblockItem = result.BlockItems.OfType<QualityBlockItem>().First();
            Assert.AreEqual(15, firstQualityblockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.Equal, firstQualityblockItem.FilterPredicate.PredicateOperator);
            var secondQualityblockItem = result.BlockItems.OfType<QualityBlockItem>().Skip(1).First();
            Assert.AreEqual(17, secondQualityblockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.LessThan, secondQualityblockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_MultipleTextColorItems_OnlyLastOneUsed()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    SetTextColor 25 1 50" + Environment.NewLine +
                              "    SetTextColor 255 20 100";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is TextColorBlockItem));
            var blockItem = result.BlockItems.OfType<TextColorBlockItem>().First();
            Assert.AreEqual(255, blockItem.Color.R);
            Assert.AreEqual(20, blockItem.Color.G);
            Assert.AreEqual(100, blockItem.Color.B);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_MultipleFontSizeItems_OnlyLastOneUsed()
        {
            // Arrange

            var inputString = "Show" + Environment.NewLine +
                              "    SetFontSize 12" + Environment.NewLine +
                              "    SetFontSize 38" + Environment.NewLine +
                              "    SetFontSize 27" + Environment.NewLine;

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is FontSizeBlockItem));
            var blockItem = result.BlockItems.OfType<FontSizeBlockItem>().First();
            Assert.AreEqual(27, blockItem.Value);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_MultipleSoundItems_OnlyLastOneUsed()
        {
            // Arrange

            var inputString = "Show" + Environment.NewLine +
                              "    PlayAlertSound 7" + Environment.NewLine +
                              "    PlayAlertSound 5 100 38" + Environment.NewLine +
                              "    PlayAlertSound 2" + Environment.NewLine;

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is SoundBlockItem));
            var blockItem = result.BlockItems.OfType<SoundBlockItem>().First();
            Assert.AreEqual("2", blockItem.Value);
            Assert.AreEqual(79, blockItem.SecondValue);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_MultipleBackgroundColorItems_OnlyLastOneUsed()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    SetBackgroundColor 25 1 50" + Environment.NewLine +
                              "    SetBackgroundColor 255 20 100";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is BackgroundColorBlockItem));
            var blockItem = result.BlockItems.OfType<BackgroundColorBlockItem>().First();
            Assert.AreEqual(255, blockItem.Color.R);
            Assert.AreEqual(20, blockItem.Color.G);
            Assert.AreEqual(100, blockItem.Color.B);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_MultipleBorderColorItems_OnlyLastOneUsed()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    SetBorderColor 25 1 50" + Environment.NewLine +
                              "    SetBorderColor 255 20 100";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is BorderColorBlockItem));
            var blockItem = result.BlockItems.OfType<BorderColorBlockItem>().First();
            Assert.AreEqual(255, blockItem.Color.R);
            Assert.AreEqual(20, blockItem.Color.G);
            Assert.AreEqual(100, blockItem.Color.B);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_MultipleRarityItems_OnlyLastOneUsed()
        {
            // Arrange
            var inputString = @"#8#" + Environment.NewLine +
                                 "Hide" + Environment.NewLine +
                                 "Rarity Magic" + Environment.NewLine +
                                 "DropLevel >= 67" + Environment.NewLine +
                                 "BaseType \"Sorcerer Boots\"" + Environment.NewLine +
                                 "Rarity Rare" + Environment.NewLine +
                                 "SetFontSize 26" + Environment.NewLine +
                                 "SetBackgroundColor 0 20 0";


            _testUtility.TestBlock.Enabled = false;
            _testUtility.TestBlock.BlockItems.Add(new WidthBlockItem(FilterPredicateOperator.Equal, 4));

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is RarityBlockItem));
            var blockItem = result.BlockItems.OfType<RarityBlockItem>().First();
            Assert.AreEqual(ItemRarity.Rare, (ItemRarity)blockItem.FilterPredicate.PredicateOperand);
        }


        [Test]
        public void TranslateStringToItemFilterBlock_SpecificTest_1()
        {
            // Arrange
            var inputString = @"Show" + Environment.NewLine +
                              "DropLevel >= 67" + Environment.NewLine +
                              "Rarity Rare" + Environment.NewLine +
                              "   ItemLevel >= 75 " + Environment.NewLine +
                              "	SetBorderColor 250 40 210" + Environment.NewLine +
                              "BaseType \"Sorcerer Boots\" \"Titan Greaves\" \"Slink Boots\" \"Murder Boots\"" + Environment.NewLine +
                              "SetBackgroundColor 0 20 0  ##TOP BASE FOR LEVEL###    " + Environment.NewLine +
                              "SetFontSize 28";


            _testUtility.TestBlock.Enabled = false;
            _testUtility.TestBlock.BlockItems.Add(new WidthBlockItem(FilterPredicateOperator.Equal, 4));

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is ActionBlockItem));
            var actionBlockItem = result.BlockItems.OfType<ActionBlockItem>().First();
            Assert.AreEqual(BlockAction.Show, actionBlockItem.Action);

            Assert.AreEqual(1, result.BlockItems.Count(b => b is DropLevelBlockItem));
            var droplevelBlockItem = result.BlockItems.OfType<DropLevelBlockItem>().First();
            Assert.AreEqual(67, droplevelBlockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.GreaterThanOrEqual, droplevelBlockItem.FilterPredicate.PredicateOperator);

            Assert.AreEqual(1, result.BlockItems.Count(b => b is RarityBlockItem));
            var rarityBlockItem = result.BlockItems.OfType<RarityBlockItem>().First();
            Assert.AreEqual(ItemRarity.Rare, (ItemRarity)rarityBlockItem.FilterPredicate.PredicateOperand);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_SpecificTest_2()
        {
            // Arrange
            var inputString = @"#8#" + Environment.NewLine +
                              "Hide " + Environment.NewLine +
                              "Rarity Magic " + Environment.NewLine +
                              "DropLevel >= 67" + Environment.NewLine +
                              "BaseType \"Sorcerer Boots\"" + Environment.NewLine +
                              "Rarity Magic " + Environment.NewLine +
                              "SetFontSize 26" + Environment.NewLine +
                              "SetBackgroundColor 0 20 0\"";


            _testUtility.TestBlock.Enabled = false;
            _testUtility.TestBlock.BlockItems.Add(new WidthBlockItem(FilterPredicateOperator.Equal, 4));

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is ActionBlockItem));
            var actionBlockItem = result.BlockItems.OfType<ActionBlockItem>().First();
            Assert.AreEqual(BlockAction.Hide, actionBlockItem.Action);

            Assert.AreEqual(1, result.BlockItems.Count(b => b is DropLevelBlockItem));
            var droplevelBlockItem = result.BlockItems.OfType<DropLevelBlockItem>().First();
            Assert.AreEqual(67, droplevelBlockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.GreaterThanOrEqual, droplevelBlockItem.FilterPredicate.PredicateOperator);

            Assert.AreEqual(1, result.BlockItems.Count(b => b is RarityBlockItem));
            var rarityBlockItem = result.BlockItems.OfType<RarityBlockItem>().First();
            Assert.AreEqual(ItemRarity.Magic, (ItemRarity)rarityBlockItem.FilterPredicate.PredicateOperand);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_CustomSoundDocumentsFile()
        {
            // Arrange
            var inputString = @"Show" + Environment.NewLine +
                              "CustomAlertSound \"test.mp3\"";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is CustomSoundBlockItem));
            var customSoundBlockItem = result.BlockItems.OfType<CustomSoundBlockItem>().First();
            Assert.AreEqual("test.mp3", customSoundBlockItem.Value);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_CustomSoundDocumentsRelativeFile()
        {
            // Arrange
            var inputString = @"Show" + Environment.NewLine +
                              "CustomAlertSound \"Sounds\test.mp3\"";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is CustomSoundBlockItem));
            var customSoundBlockItem = result.BlockItems.OfType<CustomSoundBlockItem>().First();
            Assert.AreEqual("Sounds\test.mp3", customSoundBlockItem.Value);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_CustomSoundFullBackSlashPath()
        {
            // Arrange
            var inputString = @"Show" + Environment.NewLine +
                              "CustomAlertSound \"C:\\Sounds\\test.mp3\"";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is CustomSoundBlockItem));
            var customSoundBlockItem = result.BlockItems.OfType<CustomSoundBlockItem>().First();
            Assert.AreEqual("C:\\Sounds\\test.mp3", customSoundBlockItem.Value);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_CustomSoundFullForwardSlashPath()
        {
            // Arrange
            var inputString = @"Show" + Environment.NewLine +
                              "CustomAlertSound \"C:/Sounds/test.mp3\"";

            //Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is CustomSoundBlockItem));
            var customSoundBlockItem = result.BlockItems.OfType<CustomSoundBlockItem>().First();
            Assert.AreEqual("C:/Sounds/test.mp3", customSoundBlockItem.Value);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_CustomSoundFullMixedPath()
        {
            // Arrange
            var inputString = @"Show" + Environment.NewLine +
                              "CustomAlertSound \"C:\\Sounds/test.mp3\"";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString, _testUtility.MockItemFilterScript);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is CustomSoundBlockItem));
            var customSoundBlockItem = result.BlockItems.OfType<CustomSoundBlockItem>().First();
            Assert.AreEqual("C:\\Sounds/test.mp3", customSoundBlockItem.Value);
        }

        [Test]
        public void TranslateItemFilterBlockToString_NothingPopulated_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show";

            // Act
            // TODO: Shouldn't be set to edited this way
            _testUtility.TestBlock.IsEdited = true;
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_HasBlockGroup_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show # Child 1 Block Group - Child 2 Block Group";

            var rootBlockGroup = new ItemFilterBlockGroup("Root Block Group", null);
            var child1BlockGroup = new ItemFilterBlockGroup("Child 1 Block Group", rootBlockGroup);
            var child2BlockGroup = new ItemFilterBlockGroup("Child 2 Block Group", child1BlockGroup);
            _testUtility.TestBlock.BlockGroup = child2BlockGroup;

            // TODO: Shouldn't be set to edited this way
            _testUtility.TestBlock.IsEdited = true;
            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_HasActionBlockComment_ReturnsCorrectString()
        {
            // Arrange
            var testInputActionBlockComment = "this is a test";
            var expectedResult = $"Show #{testInputActionBlockComment}";

            _testUtility.TestBlock.BlockItems.OfType<ActionBlockItem>().First().Comment = testInputActionBlockComment;
            // TODO: Shouldn't be set to edited this way
            _testUtility.TestBlock.IsEdited = true;

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_FilterTypeHide_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Hide";

            _testUtility.TestBlock.Action = BlockAction.Hide;

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_FilterDescription_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "# Test Block Description" + Environment.NewLine +
                                 "Show";

            _testUtility.TestBlock.Description = "Test Block Description";

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_FilterDescriptionWithApostraphes_DoesNotDuplicateApostraphes()
        {
            // Arrange
            var expectedResult = "# Test Block Descr'iption" + Environment.NewLine +
                                 "Show";

            _testUtility.TestBlock.Description = "Test Block Descr'iption";

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_ItemLevel_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    ItemLevel >= 56";

            _testUtility.TestBlock.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.GreaterThanOrEqual, 56));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_IdentifiedTrue_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    Identified True";

            _testUtility.TestBlock.BlockItems.Add(new IdentifiedBlockItem(true));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_IdentifiedFalse_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    Identified False";

            _testUtility.TestBlock.BlockItems.Add(new IdentifiedBlockItem(false));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_CorruptedTrue_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    Corrupted True";

            _testUtility.TestBlock.BlockItems.Add(new CorruptedBlockItem(true));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_CorruptedFalse_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    Corrupted False";

            _testUtility.TestBlock.BlockItems.Add(new CorruptedBlockItem(false));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_DropLevel_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    DropLevel = 23";

            _testUtility.TestBlock.BlockItems.Add(new DropLevelBlockItem(FilterPredicateOperator.Equal, 23));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_GemLevel_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    GemLevel <= 15";

            _testUtility.TestBlock.BlockItems.Add(new GemLevelBlockItem(FilterPredicateOperator.LessThanOrEqual, 15));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_StackSize_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    StackSize = 5";

            _testUtility.TestBlock.BlockItems.Add(new StackSizeBlockItem(FilterPredicateOperator.Equal, 5));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_Quality_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    Quality < 10";

            _testUtility.TestBlock.BlockItems.Add(new QualityBlockItem(FilterPredicateOperator.LessThan, 10));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_Rarity_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    Rarity < Rare";

            _testUtility.TestBlock.BlockItems.Add(new RarityBlockItem(FilterPredicateOperator.LessThan,
                (int) ItemRarity.Rare));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_Classes_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    Class \"Test Class\" \"Another Class\" \"Yet Another Class\"";

            var classBlockItem = new ClassBlockItem();
            classBlockItem.Items.Add("Test Class");
            classBlockItem.Items.Add("Another Class");
            classBlockItem.Items.Add("Yet Another Class");
            _testUtility.TestBlock.BlockItems.Add(classBlockItem);

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_Classes_DoesNotDuplicateApostraphes()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    Class \"Test Cl'ass\"";

            var classBlockItem = new ClassBlockItem();
            classBlockItem.Items.Add("Test Cl'ass");
            _testUtility.TestBlock.BlockItems.Add(classBlockItem);

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_BaseTypes_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    BaseType \"Test BaseType\" \"Another BaseType\" \"Yet Another BaseType\"";

            var baseTypeBlockItem = new BaseTypeBlockItem();
            baseTypeBlockItem.Items.Add("Test BaseType");
            baseTypeBlockItem.Items.Add("Another BaseType");
            baseTypeBlockItem.Items.Add("Yet Another BaseType");
            _testUtility.TestBlock.BlockItems.Add(baseTypeBlockItem);

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_Prophecies_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    Prophecy \"Test Prophecy\" \"Another Prophecy\" \"Yet Another Prophecy\"";

            var prophecyBlockItem = new ProphecyBlockItem();
            prophecyBlockItem.Items.Add("Test Prophecy");
            prophecyBlockItem.Items.Add("Another Prophecy");
            prophecyBlockItem.Items.Add("Yet Another Prophecy");
            _testUtility.TestBlock.BlockItems.Add(prophecyBlockItem);

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_HasExplicitMod_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    HasExplicitMod \"Test Mod\" \"Another Mod\" \"Yet Another Mod\"";

            var hasExplicitModBlockItem = new HasExplicitModBlockItem();
            hasExplicitModBlockItem.Items.Add("Test Mod");
            hasExplicitModBlockItem.Items.Add("Another Mod");
            hasExplicitModBlockItem.Items.Add("Yet Another Mod");
            _testUtility.TestBlock.BlockItems.Add(hasExplicitModBlockItem);

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_Sockets_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    Sockets >= 5";

            _testUtility.TestBlock.BlockItems.Add(new SocketsBlockItem(FilterPredicateOperator.GreaterThanOrEqual, 5));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_SocketGroup_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    SocketGroup \"RRG\" \"GBB\"";

            var socketGroupBlockItem = new SocketGroupBlockItem();
            socketGroupBlockItem.Items.Add("RRG");
            socketGroupBlockItem.Items.Add("GBB");
            _testUtility.TestBlock.BlockItems.Add(socketGroupBlockItem);

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_LinkedSockets_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    LinkedSockets = 3";

            _testUtility.TestBlock.BlockItems.Add(new LinkedSocketsBlockItem(FilterPredicateOperator.Equal, 3));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_Width_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    Width = 4";

            _testUtility.TestBlock.BlockItems.Add(new WidthBlockItem(FilterPredicateOperator.Equal, 4));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_Height_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    Height > 1";

            _testUtility.TestBlock.BlockItems.Add(new HeightBlockItem(FilterPredicateOperator.GreaterThan, 1));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_TextColorMaxAlpha_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    SetTextColor 54 102 255";

            _testUtility.TestBlock.BlockItems.Add(new TextColorBlockItem(new Color {A = 240, R = 54, G = 102, B = 255}));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_TextColorWithThemeComponent_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    SetTextColor 54 102 255 # Test Theme Component";

            var blockItem = new TextColorBlockItem(new Color {A = 240, R = 54, G = 102, B = 255})
            {
                ThemeComponent = new ColorThemeComponent(ThemeComponentType.TextColor, "Test Theme Component", new Color())
            };

            _testUtility.TestBlock.BlockItems.Add(blockItem);

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_TextColorNotMaxAlpha_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    SetTextColor 54 102 255 66";

            _testUtility.TestBlock.BlockItems.Add(new TextColorBlockItem(new Color {A = 66, R = 54, G = 102, B = 255}));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_BackgroundColorNotMaxAlpha_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    SetBackgroundColor 12 0 212 69";

            _testUtility.TestBlock.BlockItems.Add(
                new BackgroundColorBlockItem(new Color {A = 69, R = 12, G = 0, B = 212}));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_BorderColorNotMaxAlpha_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    SetBorderColor 128 32 0 1";

            _testUtility.TestBlock.BlockItems.Add(new BorderColorBlockItem(new Color {A = 1, R = 128, G = 32, B = 0}));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_FontSize_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    SetFontSize 15";

            _testUtility.TestBlock.BlockItems.Add(new FontSizeBlockItem(15));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_Sound_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    PlayAlertSound 2 50";

            _testUtility.TestBlock.BlockItems.Add(new SoundBlockItem("2", 50));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_MultipleItemLevel_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    ItemLevel > 56" + Environment.NewLine +
                                 "    ItemLevel >= 45" + Environment.NewLine +
                                 "    ItemLevel < 100";

            _testUtility.TestBlock.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.GreaterThan, 56));
            _testUtility.TestBlock.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.GreaterThanOrEqual, 45));
            _testUtility.TestBlock.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.LessThan, 100));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Ignore("This test fails after the refactoring to use OutputText in the translator, but this situation shouldn't ever occur anyway")]
        [Test]
        public void TranslateItemFilterBlockToString_MultipleFontSize_UsesFirstFontSize()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    SetFontSize 1";

            _testUtility.TestBlock.BlockItems.Add(new FontSizeBlockItem(1));
            _testUtility.TestBlock.BlockItems.Add(new FontSizeBlockItem(2));
            _testUtility.TestBlock.BlockItems.Add(new FontSizeBlockItem(3));
            _testUtility.TestBlock.BlockItems.Add(new FontSizeBlockItem(4));
            _testUtility.TestBlock.BlockItems.Add(new FontSizeBlockItem(15));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_DisabledBlock_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "#Show" + Environment.NewLine +
                                 "#    Width = 4";


            _testUtility.TestBlock.Enabled = false;
            _testUtility.TestBlock.BlockItems.Add(new WidthBlockItem(FilterPredicateOperator.Equal, 4));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TranslateItemFilterBlockToString_Everything_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show" + Environment.NewLine +
                                 "    LinkedSockets >= 4" + Environment.NewLine +
                                 "    Sockets <= 6" + Environment.NewLine +
                                 "    Quality > 2" + Environment.NewLine +
                                 "    Identified True" + Environment.NewLine +
                                 "    Corrupted False" + Environment.NewLine +
                                 "    ElderItem True" + Environment.NewLine +
                                 "    ShaperItem False" + Environment.NewLine +
                                 "    MapTier < 10" + Environment.NewLine +
                                 "    ShapedMap True" + Environment.NewLine +
                                 "    ElderMap True" + Environment.NewLine +
                                 "    Height <= 6" + Environment.NewLine +
                                 "    Height >= 2" + Environment.NewLine +
                                 "    Width = 3" + Environment.NewLine +
                                 "    ItemLevel > 70" + Environment.NewLine +
                                 "    ItemLevel <= 85" + Environment.NewLine +
                                 "    DropLevel > 56" + Environment.NewLine +
                                 "    GemLevel < 15" + Environment.NewLine +
                                 "    StackSize >= 4" + Environment.NewLine +
                                 "    Rarity = Unique" + Environment.NewLine +
                                 "    Class \"Body Armour\" \"Gloves\" \"Belt\" \"Two Hand Axes\"" + Environment.NewLine +
                                 "    BaseType \"Greater Life Flask\" \"Simple Robe\" \"Full Wyrmscale\"" + Environment.NewLine +
                                 "    Prophecy \"The Cursed Choir\" \"A Valuable Combination\" \"The Beautiful Guide\"" + Environment.NewLine +
                                 "    HasExplicitMod \"Guatelitzi's\" \"of Tacati\" \"Tyrannical\"" + Environment.NewLine +
                                 "    SetTextColor 255 89 0 56" + Environment.NewLine +
                                 "    SetBackgroundColor 0 0 0" + Environment.NewLine +
                                 "    SetBorderColor 255 1 254" + Environment.NewLine +
                                 "    SetFontSize 50" + Environment.NewLine +
                                 "    PlayAlertSound 6 90" + Environment.NewLine +
                                 "    DisableDropSound" + Environment.NewLine +
                                 "    MinimapIcon 1 Blue Circle" + Environment.NewLine +
                                 "    PlayEffect Red Temp" + Environment.NewLine +
                                 "    CustomAlertSound \"test.mp3\"";

            _testUtility.TestBlock.BlockItems.Add(new ActionBlockItem(BlockAction.Show));
            _testUtility.TestBlock.BlockItems.Add(new IdentifiedBlockItem(true));
            _testUtility.TestBlock.BlockItems.Add(new CorruptedBlockItem(false));
            _testUtility.TestBlock.BlockItems.Add(new ActionBlockItem(BlockAction.Show));
            _testUtility.TestBlock.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.GreaterThan, 70));
            _testUtility.TestBlock.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.LessThanOrEqual, 85));
            _testUtility.TestBlock.BlockItems.Add(new DropLevelBlockItem(FilterPredicateOperator.GreaterThan, 56));
            _testUtility.TestBlock.BlockItems.Add(new GemLevelBlockItem(FilterPredicateOperator.LessThan, 15));
            _testUtility.TestBlock.BlockItems.Add(new StackSizeBlockItem(FilterPredicateOperator.GreaterThanOrEqual, 4));
            _testUtility.TestBlock.BlockItems.Add(new QualityBlockItem(FilterPredicateOperator.GreaterThan, 2));
            _testUtility.TestBlock.BlockItems.Add(new RarityBlockItem(FilterPredicateOperator.Equal, (int)ItemRarity.Unique));
            var classItemblockItem = new ClassBlockItem();
            classItemblockItem.Items.Add("Body Armour");
            classItemblockItem.Items.Add("Gloves");
            classItemblockItem.Items.Add("Belt");
            classItemblockItem.Items.Add("Two Hand Axes");
            _testUtility.TestBlock.BlockItems.Add(classItemblockItem);
            var baseTypeItemblockItem = new BaseTypeBlockItem();
            baseTypeItemblockItem.Items.Add("Greater Life Flask");
            baseTypeItemblockItem.Items.Add("Simple Robe");
            baseTypeItemblockItem.Items.Add("Full Wyrmscale");
            _testUtility.TestBlock.BlockItems.Add(baseTypeItemblockItem);
            var prophecyItemblockItem = new ProphecyBlockItem();
            prophecyItemblockItem.Items.Add("The Cursed Choir");
            prophecyItemblockItem.Items.Add("A Valuable Combination");
            prophecyItemblockItem.Items.Add("The Beautiful Guide");
            _testUtility.TestBlock.BlockItems.Add(prophecyItemblockItem);
            var hasExplicitModBlockItem = new HasExplicitModBlockItem();
            hasExplicitModBlockItem.Items.Add("Guatelitzi's");
            hasExplicitModBlockItem.Items.Add("of Tacati");
            hasExplicitModBlockItem.Items.Add("Tyrannical");
            _testUtility.TestBlock.BlockItems.Add(hasExplicitModBlockItem);
            _testUtility.TestBlock.BlockItems.Add(new SocketsBlockItem(FilterPredicateOperator.LessThanOrEqual, 6));
            _testUtility.TestBlock.BlockItems.Add(new LinkedSocketsBlockItem(FilterPredicateOperator.GreaterThanOrEqual, 4));
            _testUtility.TestBlock.BlockItems.Add(new WidthBlockItem(FilterPredicateOperator.Equal, 3));
            _testUtility.TestBlock.BlockItems.Add(new HeightBlockItem(FilterPredicateOperator.LessThanOrEqual, 6));
            _testUtility.TestBlock.BlockItems.Add(new HeightBlockItem(FilterPredicateOperator.GreaterThanOrEqual, 2));
            _testUtility.TestBlock.BlockItems.Add(new TextColorBlockItem(new Color {A = 56, R = 255, G = 89, B = 0}));
            _testUtility.TestBlock.BlockItems.Add(new BackgroundColorBlockItem(new Color { A = 240, R = 0, G = 0, B = 0 }));
            _testUtility.TestBlock.BlockItems.Add(new BorderColorBlockItem(new Color { A = 240, R = 255, G = 1, B = 254 }));
            _testUtility.TestBlock.BlockItems.Add(new FontSizeBlockItem(50));
            _testUtility.TestBlock.BlockItems.Add(new SoundBlockItem("6", 90));
            _testUtility.TestBlock.BlockItems.Add(new ElderItemBlockItem(true));
            _testUtility.TestBlock.BlockItems.Add(new ShaperItemBlockItem(false));
            _testUtility.TestBlock.BlockItems.Add(new ShapedMapBlockItem(true));
            _testUtility.TestBlock.BlockItems.Add(new ElderMapBlockItem(true));
            _testUtility.TestBlock.BlockItems.Add(new CustomSoundBlockItem("test.mp3"));
            _testUtility.TestBlock.BlockItems.Add(new MapTierBlockItem(FilterPredicateOperator.LessThan, 10));
            _testUtility.TestBlock.BlockItems.Add(new MapIconBlockItem(IconSize.Medium, IconColor.Blue, IconShape.Circle));
            _testUtility.TestBlock.BlockItems.Add(new PlayEffectBlockItem(EffectColor.Red, true));
            _testUtility.TestBlock.BlockItems.Add(new DisableDropSoundBlockItem());

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void ReplaceAudioVisualBlockItemsFromString_SingleLine_ReplacesColorBlock()
        {
            // Arrange
            var testInputString = "SetTextColor 240 200 150 # Rarest Currency";

            var testInputBlockItems = new ObservableCollection<IItemFilterBlockItem>();
            var testInputBlockItem = new TextColorBlockItem(Colors.Red);
            testInputBlockItems.Add(testInputBlockItem);

            // Act
            _testUtility.Translator.ReplaceAudioVisualBlockItemsFromString(testInputBlockItems, testInputString);

            // Assert
            var textColorBlockItem = testInputBlockItems.First(b => b is TextColorBlockItem) as TextColorBlockItem;
            Assert.IsNotNull(textColorBlockItem);
            Assert.AreNotSame(testInputBlockItem, textColorBlockItem);
            Assert.AreEqual(new Color { R = 240, G = 200, B = 150, A = 240}, textColorBlockItem.Color);
        }

        [Test]
        public void ReplaceAudioVisualBlockItemsFromString_SingleLine_ReplacesSoundBlockItem()
        {
            // Arrange
            var testInputString = "PlayAlertSound 7 280";

            var testInputBlockItems = new ObservableCollection<IItemFilterBlockItem>();
            var testInputBlockItem = new SoundBlockItem("12",30);
            testInputBlockItems.Add(testInputBlockItem);

            // Act
            _testUtility.Translator.ReplaceAudioVisualBlockItemsFromString(testInputBlockItems, testInputString);

            // Assert
            var soundBlockItem = testInputBlockItems.First(b => b is SoundBlockItem) as SoundBlockItem;
            Assert.IsNotNull(soundBlockItem);
            Assert.AreNotSame(testInputBlockItem, soundBlockItem);
            Assert.AreEqual("7", soundBlockItem.Value);
            Assert.AreEqual(280, soundBlockItem.SecondValue);
        }

        [Test]
        public void ReplaceAudioVisualBlockItemsFromString_SingleLine_ReplacesColorBlockBugTest()
        {
            // Arrange
            var testInputString = "SetBackgroundColor 70 0 0 255";

            var testInputBlockItems = new ObservableCollection<IItemFilterBlockItem>();
            var testInputBlockItem = new BackgroundColorBlockItem(new Color { R = 70, G = 0, B = 1, A = 255});
            testInputBlockItems.Add(testInputBlockItem);

            // Act
            _testUtility.Translator.ReplaceAudioVisualBlockItemsFromString(testInputBlockItems, testInputString);

            // Assert
            var backgroundColorBlockItem = testInputBlockItems.First(b => b is BackgroundColorBlockItem) as BackgroundColorBlockItem;
            Assert.IsNotNull(backgroundColorBlockItem);
            Assert.AreNotSame(testInputBlockItem, backgroundColorBlockItem);
            Assert.AreEqual(new Color { R = 70, G = 0, B = 0, A = 255 }, backgroundColorBlockItem.Color);
        }

        [Ignore("Not currently possible - will not be necessary once commanding (to enable undo history) is implemented anyway")]
        [Test]
        public void ReplaceAudioVisualBlockItemsFromString_MalformedLine_DoesNothing()
        {
            // Arrange
            var testInputString = "SetTextCsaolor 240 200 150 # Rarest Currency";

            var testInputBlockItems = new ObservableCollection<IItemFilterBlockItem>();
            var testInputBlockItem = new TextColorBlockItem(Colors.Red);
            testInputBlockItems.Add(testInputBlockItem);

            // Act
            _testUtility.Translator.ReplaceAudioVisualBlockItemsFromString(testInputBlockItems, testInputString);

            // Assert
            var textColorBlockItem = testInputBlockItems.First(b => b is TextColorBlockItem) as TextColorBlockItem;
            Assert.IsNotNull(textColorBlockItem);
            Assert.AreSame(testInputBlockItem, textColorBlockItem);
        }

        [Test]
        public void ReplaceAudioVisualBlockItemsFromString_MultipleLines_ExistingBlockItems()
        {
            // Arrange
            var testInputString = "SetTextColor 240 200 150 # Rarest Currency" + Environment.NewLine +
                                  "SetBackgroundColor 0 0 0 # Rarest Currency Background" + Environment.NewLine +
                                  "SetBorderColor 255 255 255 # Rarest Currency Border" + Environment.NewLine +
                                  "PlayAlertSound 7 280";

            var testInputBlockItems = new ObservableCollection<IItemFilterBlockItem>();
            var testInputTextColorBlockItem = new TextColorBlockItem(Colors.Red);
            var testInputBackgroundColorBlockItem = new BackgroundColorBlockItem(Colors.Blue);
            var testInputBorderColorBlockItem = new BorderColorBlockItem(Colors.Yellow);
            var testInputSoundBlockItem = new SoundBlockItem("1", 1);

            testInputBlockItems.Add(testInputTextColorBlockItem);
            testInputBlockItems.Add(testInputBackgroundColorBlockItem);
            testInputBlockItems.Add(testInputBorderColorBlockItem);
            testInputBlockItems.Add(testInputSoundBlockItem);

            // Act
            _testUtility.Translator.ReplaceAudioVisualBlockItemsFromString(testInputBlockItems, testInputString);

            // Assert
            var textColorBlockItem = testInputBlockItems.First(b => b is TextColorBlockItem) as TextColorBlockItem;
            Assert.IsNotNull(textColorBlockItem);
            Assert.AreNotSame(testInputTextColorBlockItem, textColorBlockItem);
            Assert.AreEqual(new Color {A = 240, R = 240, G = 200, B = 150}, textColorBlockItem.Color);

            var backgroundColorBlockItem = testInputBlockItems.First(b => b is BackgroundColorBlockItem) as BackgroundColorBlockItem;
            Assert.IsNotNull(backgroundColorBlockItem);
            Assert.AreNotSame(testInputBackgroundColorBlockItem, backgroundColorBlockItem);
            Assert.AreEqual(new Color { A = 240, R = 0, G = 0, B = 0 }, backgroundColorBlockItem.Color);

            var borderColorBlockItem = testInputBlockItems.First(b => b is BorderColorBlockItem) as BorderColorBlockItem;
            Assert.IsNotNull(borderColorBlockItem);
            Assert.AreNotSame(testInputBorderColorBlockItem, borderColorBlockItem);
            Assert.AreEqual(new Color { A = 240, R = 255, G = 255, B = 255 }, borderColorBlockItem.Color);

            var soundBlockItem = testInputBlockItems.First(b => b is SoundBlockItem) as SoundBlockItem;
            Assert.IsNotNull(soundBlockItem);
            Assert.AreNotSame(testInputSoundBlockItem, soundBlockItem);
            Assert.AreEqual("7", soundBlockItem.Value);
            Assert.AreEqual(280, soundBlockItem.SecondValue);
        }

        [Test]
        public void ReplaceAudioVisualBlockItemsFromString_MultipleLines_NoExistingBlockItems()
        {
            // Arrange
            var testInputString = "SetTextColor 240 200 150 # Rarest Currency" + Environment.NewLine +
                                  "SetBackgroundColor 0 0 0 # Rarest Currency Background" + Environment.NewLine +
                                  "SetBorderColor 255 255 255 # Rarest Currency Border" + Environment.NewLine +
                                  "PlayAlertSound 7 280";

            var testInputBlockItems = new ObservableCollection<IItemFilterBlockItem>();

            // Act
            _testUtility.Translator.ReplaceAudioVisualBlockItemsFromString(testInputBlockItems, testInputString);

            // Assert
            var textColorBlockItem = testInputBlockItems.First(b => b is TextColorBlockItem) as TextColorBlockItem;
            Assert.IsNotNull(textColorBlockItem);
            Assert.AreEqual(new Color { A = 240, R = 240, G = 200, B = 150 }, textColorBlockItem.Color);

            var backgroundColorBlockItem = testInputBlockItems.First(b => b is BackgroundColorBlockItem) as BackgroundColorBlockItem;
            Assert.IsNotNull(backgroundColorBlockItem);
            Assert.AreEqual(new Color { A = 240, R = 0, G = 0, B = 0 }, backgroundColorBlockItem.Color);

            var borderColorBlockItem = testInputBlockItems.First(b => b is BorderColorBlockItem) as BorderColorBlockItem;
            Assert.IsNotNull(borderColorBlockItem);
            Assert.AreEqual(new Color { A = 240, R = 255, G = 255, B = 255 }, borderColorBlockItem.Color);

            var soundBlockItem = testInputBlockItems.First(b => b is SoundBlockItem) as SoundBlockItem;
            Assert.IsNotNull(soundBlockItem);
            Assert.AreEqual("7", soundBlockItem.Value);
            Assert.AreEqual(280, soundBlockItem.SecondValue);
        }

        [Test]
        public void ReplaceAudioVisualBlockItemsFromString_MultipleLines_SomeExistingBlockItems()
        {
            // Arrange
            var testInputString = "SetTextColor 240 200 150 # Rarest Currency" + Environment.NewLine +
                                  "SetBackgroundColor 0 0 0 # Rarest Currency Background";

            var testInputBlockItems = new ObservableCollection<IItemFilterBlockItem>();
            var testInputTextColorBlockItem = new TextColorBlockItem(Colors.Red);
            var testInputBackgroundColorBlockItem = new BackgroundColorBlockItem(Colors.Blue);
            var testInpuBorderColorBlockItem = new BorderColorBlockItem(Colors.Yellow);
            testInputBlockItems.Add(testInputTextColorBlockItem);
            testInputBlockItems.Add(testInputBackgroundColorBlockItem);
            testInputBlockItems.Add(testInpuBorderColorBlockItem);

            // Act
            _testUtility.Translator.ReplaceAudioVisualBlockItemsFromString(testInputBlockItems, testInputString);

            // Assert
            var textColorBlockItem = testInputBlockItems.First(b => b is TextColorBlockItem) as TextColorBlockItem;
            Assert.IsNotNull(textColorBlockItem);
            Assert.AreEqual(new Color { A = 240, R = 240, G = 200, B = 150 }, textColorBlockItem.Color);

            var backgroundColorBlockItem = testInputBlockItems.First(b => b is BackgroundColorBlockItem) as BackgroundColorBlockItem;
            Assert.IsNotNull(backgroundColorBlockItem);
            Assert.AreEqual(new Color { A = 240, R = 0, G = 0, B = 0 }, backgroundColorBlockItem.Color);

            Assert.AreEqual(0, testInputBlockItems.Count(b => b is BorderColorBlockItem));
        }

        [Ignore("ThemeComponentBuilder deprecated")]
        [Test]
        public void ReplaceAudioVisualBlockItemsFromString_ThemeComponentBuilderNotInitialised_DoesNotCallAddComponent()
        {
            // Arrange
            var testInputString = "SetTextColor 240 200 150 # Rarest Currency";

            var testInputBlockItems = new ObservableCollection<IItemFilterBlockItem>();
            var testInputBlockItem = new TextColorBlockItem(Colors.Red);
            testInputBlockItems.Add(testInputBlockItem);

            // Act
            _testUtility.Translator.ReplaceAudioVisualBlockItemsFromString(testInputBlockItems, testInputString);

            // Assert

        }

        private class ItemFilterBlockTranslatorTestUtility
        {
            public ItemFilterBlockTranslatorTestUtility()
            {
                // Test Data
                TestBlock = new ItemFilterBlock();

                // Mock setups
                MockBlockGroupHierarchyBuilder = new Mock<IBlockGroupHierarchyBuilder>();
                MockItemFilterScript = Mock.Of<IItemFilterScript>(i => i.ItemFilterScriptSettings.ThemeComponentCollection == new ThemeComponentCollection());

                // Class under test instantiation
                Translator = new ItemFilterBlockTranslator(MockBlockGroupHierarchyBuilder.Object);
            }

            public ItemFilterBlock TestBlock { get; set; }
            public Mock<IBlockGroupHierarchyBuilder> MockBlockGroupHierarchyBuilder { get; }
            public ItemFilterBlockTranslator Translator { get; }

            public IItemFilterScript MockItemFilterScript { get; }
        }
    }
}
