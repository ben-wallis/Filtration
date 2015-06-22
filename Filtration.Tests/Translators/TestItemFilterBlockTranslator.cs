using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
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
    public class TestItemFilterBlockTranslator
    {
        private ItemFilterBlockTranslatorTestUtility _testUtility;

        [SetUp]
        public void ItemFilterBlockTranslatorTestSetUp()
        {
            _testUtility = new ItemFilterBlockTranslatorTestUtility();
        }

        [Test]
        public void TranslateStringToItemFilterBlock_NoDescriptionComment_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    ItemLevel >= 55";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is ItemLevelBlockItem));
            var blockItem = result.BlockItems.OfType<ItemLevelBlockItem>().First();
            Assert.AreEqual(55, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.GreaterThanOrEqual, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_BlockGroupComment_CallsBlockGroupHierarchyBuilder()
        {
            // Arrange
            var inputString = "Show # TestBlockGroup" + Environment.NewLine;
            var inputBlockGroup = new ItemFilterBlockGroup("TestBlockGroup", null);

            // Act
            _testUtility.MockBlockGroupHierarchyBuilder.Setup(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.IsAny<IEnumerable<string>>())).Returns(inputBlockGroup).Verifiable();
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert
            _testUtility.MockBlockGroupHierarchyBuilder.Verify();
        }

        [Test]
        public void TranslateStringToItemFilterBlock_ShowBlock_SetsBlockGroupIsCheckedCorrectly()
        {
            // Arrange
            var inputString = "Show # TestBlockGroup" + Environment.NewLine;
            var inputBlockGroup = new ItemFilterBlockGroup("TestBlockGroup", null);

            // Act
            _testUtility.MockBlockGroupHierarchyBuilder.Setup(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.IsAny<IEnumerable<string>>())).Returns(inputBlockGroup).Verifiable();
            _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert
            Assert.AreEqual(true, inputBlockGroup.IsChecked);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_HideBlock_SetsBlockGroupIsCheckedCorrectly()
        {
            // Arrange
            var inputString = "Hide # TestBlockGroup" + Environment.NewLine;
            var inputBlockGroup = new ItemFilterBlockGroup("TestBlockGroup", null);

            // Act
            _testUtility.MockBlockGroupHierarchyBuilder.Setup(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.IsAny<IEnumerable<string>>())).Returns(inputBlockGroup).Verifiable();
            _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert
            Assert.AreEqual(false, inputBlockGroup.IsChecked);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_NoBlockGroupComment_CallsBlockGroupHierarchyBuilder()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine;

            // Act
            _testUtility.MockBlockGroupHierarchyBuilder.Setup(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.IsAny<IEnumerable<string>>())).Verifiable();
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert
            _testUtility.MockBlockGroupHierarchyBuilder.Verify(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_BlockGroupCommentWithNoGroups_DoesNotThrow()
        {
            // Arrange
            var inputString = "Show    #" + Environment.NewLine;

            // Act
            _testUtility.MockBlockGroupHierarchyBuilder.Setup(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.IsAny<IEnumerable<string>>())).Verifiable();
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert
            _testUtility.MockBlockGroupHierarchyBuilder.Verify(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_BlockGroupComment_SetsBlockItemGroupCorrectly()
        {
            // Arrange
            var inputString = "Show # Test Block Group - Test Sub Block Group - Test Another Block Group" + Environment.NewLine;
            var testBlockGroup = new ItemFilterBlockGroup("zzzzz", null);

            // Act
            _testUtility.MockBlockGroupHierarchyBuilder.Setup(b => b.IntegrateStringListIntoBlockGroupHierarchy(It.IsAny<IEnumerable<string>>())).Returns(testBlockGroup).Verifiable();
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert
            Assert.AreEqual(testBlockGroup, result.BlockGroup);
            _testUtility.MockBlockGroupHierarchyBuilder.Verify();
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Hide_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Hide" + Environment.NewLine;

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is DropLevelBlockItem));
            var blockItem = result.BlockItems.OfType<DropLevelBlockItem>().First();
            Assert.AreEqual(40, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.Equal, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Quality_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    Quality < 18";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is BaseTypeBlockItem));
            var blockItem = result.BlockItems.OfType<BaseTypeBlockItem>().First();
            Assert.Contains("Test Base Type 1", blockItem.Items);
            Assert.Contains("TestOneWordBaseTypeInQuotes", blockItem.Items);
            Assert.Contains("TestOneWordBaseTypeNotInQuotes", blockItem.Items);
            Assert.Contains("Test BaseType 2", blockItem.Items);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Sockets_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    Sockets > 2";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
                              "    LinkedSockets != 1";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is LinkedSocketsBlockItem));
            var blockItem = result.BlockItems.OfType<LinkedSocketsBlockItem>().First();
            Assert.AreEqual(1, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.NotEqual, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Width_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    Width != 1";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert

            Assert.AreEqual(1, result.BlockItems.Count(b => b is WidthBlockItem));
            var blockItem = result.BlockItems.OfType<WidthBlockItem>().First();
            Assert.AreEqual(1, blockItem.FilterPredicate.PredicateOperand);
            Assert.AreEqual(FilterPredicateOperator.NotEqual, blockItem.FilterPredicate.PredicateOperator);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_Height_ReturnsCorrectObject()
        {
            // Arrange
            var inputString = "Show" + Environment.NewLine +
                              "    Height <= 3";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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

            Assert.DoesNotThrow(() => _testUtility.Translator.TranslateStringToItemFilterBlock(inputString));
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is BorderColorBlockItem));
            var blockItem = result.BlockItems.OfType<BorderColorBlockItem>().First();
            Assert.AreEqual(255, blockItem.Color.R);
            Assert.AreEqual(20, blockItem.Color.G);
            Assert.AreEqual(100, blockItem.Color.B);

        }

        [Test]
        public void TranslateStringToItemFilterBlock_SetFontSize_ReturnsCorrectObject()
        {
            // Arrange

            var inputString = "Show" + Environment.NewLine +
                              "    SetFontSize 15";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is SoundBlockItem));
            var blockItem = result.BlockItems.OfType<SoundBlockItem>().First();
            Assert.AreEqual(4, blockItem.Value);
            Assert.AreEqual(79, blockItem.SecondValue);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_PlayAlertSoundWithVolume_ReturnsCorrectObject()
        {
            // Arrange

            var inputString = "Show" + Environment.NewLine +
                              "    PlayAlertSound 2 95";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is SoundBlockItem));
            var blockItem = result.BlockItems.OfType<SoundBlockItem>().First();
            Assert.AreEqual(2, blockItem.Value);
            Assert.AreEqual(95, blockItem.SecondValue);
        }

        [Test]
        public void TranslateStringToItemFilterBlock_SectionComment_ReturnsItemFilterSectionObjectWithCorrectDescription()
        {
            // Arrange
            const string TestInputSectionDescription = "Wonderful items that you definitely won't want to miss!";
            var inputString = "# Section: " + TestInputSectionDescription;

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert
            Assert.IsInstanceOf<ItemFilterSection>(result);
            Assert.AreEqual(TestInputSectionDescription, result.Description);
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
                              "    Quality = 15" + Environment.NewLine +
                              "    Rarity <= Unique" + Environment.NewLine +
                              @"    Class ""My Item Class"" AnotherClass ""AndAnotherClass""" + Environment.NewLine +
                              @"    BaseType MyBaseType ""Another BaseType""" + Environment.NewLine +
                              "    JunkLine Let's ignore this one!" + Environment.NewLine +
                              "    #Quality Commented out quality line" + Environment.NewLine +
                              "    Sockets >= 3" + Environment.NewLine +
                              "    LinkedSockets != 2" + Environment.NewLine +
                              "    SocketGroup RGBB RGBWW" + Environment.NewLine +
                              "    SetTextColor 50 100 3 200" + Environment.NewLine +
                              "    SetBackgroundColor 255 100 5" + Environment.NewLine +
                              "    SetBorderColor 0 0 0" + Environment.NewLine +
                              "    SetFontSize 50" + Environment.NewLine +
                              "    PlayAlertSound 3";

            // Act
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert
            Assert.AreEqual("Test filter with everything", result.Description);
            var itemLevelblockItem = result.BlockItems.OfType<ItemLevelBlockItem>().First();
            Assert.AreEqual(FilterPredicateOperator.GreaterThanOrEqual, itemLevelblockItem.FilterPredicate.PredicateOperator);
            Assert.AreEqual(50, itemLevelblockItem.FilterPredicate.PredicateOperand);

            var dropLevelblockItem = result.BlockItems.OfType<DropLevelBlockItem>().First();
            Assert.AreEqual(FilterPredicateOperator.LessThan, dropLevelblockItem.FilterPredicate.PredicateOperator);
            Assert.AreEqual(70, dropLevelblockItem.FilterPredicate.PredicateOperand);

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

            var socketsblockItem = result.BlockItems.OfType<SocketsBlockItem>().First();
            Assert.AreEqual(FilterPredicateOperator.GreaterThanOrEqual, socketsblockItem.FilterPredicate.PredicateOperator);
            Assert.AreEqual(3, socketsblockItem.FilterPredicate.PredicateOperand);

            var linkedSocketsblockItem = result.BlockItems.OfType<LinkedSocketsBlockItem>().First();
            Assert.AreEqual(FilterPredicateOperator.NotEqual, linkedSocketsblockItem.FilterPredicate.PredicateOperator);
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

            var soundblockItem = result.BlockItems.OfType<SoundBlockItem>().First();
            Assert.AreEqual(3, soundblockItem.Value);
            Assert.AreEqual(79, soundblockItem.SecondValue);
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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is SoundBlockItem));
            var blockItem = result.BlockItems.OfType<SoundBlockItem>().First();
            Assert.AreEqual(2, blockItem.Value);
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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

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
            var result = _testUtility.Translator.TranslateStringToItemFilterBlock(inputString);

            // Assert
            Assert.AreEqual(1, result.BlockItems.Count(b => b is BorderColorBlockItem));
            var blockItem = result.BlockItems.OfType<BorderColorBlockItem>().First();
            Assert.AreEqual(255, blockItem.Color.R);
            Assert.AreEqual(20, blockItem.Color.G);
            Assert.AreEqual(100, blockItem.Color.B);
        }
        
        [Test]
        public void TranslateItemFilterBlockToString_NothingPopulated_ReturnsCorrectString()
        {
            // Arrange
            var expectedResult = "Show";

            // Act
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
                                 "    LinkedSockets != 3";

            _testUtility.TestBlock.BlockItems.Add(new LinkedSocketsBlockItem(FilterPredicateOperator.NotEqual, 3));

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

            _testUtility.TestBlock.BlockItems.Add(new TextColorBlockItem(new Color {A = 255, R = 54, G = 102, B = 255}));

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

            _testUtility.TestBlock.BlockItems.Add(new SoundBlockItem(2, 50));

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
        public void TranslateItemFilterBlockToString_Section_ReturnsCorrectString()
        {
            // Arrange
            var TestInputSectionText = "Ermagerd it's a section!";
            var expectedResult = "# Section: " + TestInputSectionText;

            _testUtility.TestBlock = new ItemFilterSection { Description = TestInputSectionText };

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
                                 "    ItemLevel > 70" + Environment.NewLine +
                                 "    ItemLevel <= 85" + Environment.NewLine +
                                 "    DropLevel != 56" + Environment.NewLine +
                                 "    Quality > 2" + Environment.NewLine +
                                 "    Rarity = Unique" + Environment.NewLine +
                                 "    Class \"Body Armour\" \"Gloves\" \"Belt\" \"Two Hand Axes\"" + Environment.NewLine +
                                 "    BaseType \"Greater Life Flask\" \"Simple Robe\" \"Full Wyrmscale\"" +
                                 Environment.NewLine +
                                 "    Sockets <= 6" + Environment.NewLine +
                                 "    LinkedSockets >= 4" + Environment.NewLine +
                                 "    Width = 3" + Environment.NewLine +
                                 "    Height <= 6" + Environment.NewLine +
                                 "    Height >= 2" + Environment.NewLine +
                                 "    SetTextColor 255 89 0 56" + Environment.NewLine +
                                 "    SetBackgroundColor 0 0 0" + Environment.NewLine +
                                 "    SetBorderColor 255 1 254" + Environment.NewLine +
                                 "    SetFontSize 50" + Environment.NewLine +
                                 "    PlayAlertSound 6 90";

            _testUtility.TestBlock.BlockItems.Add(new ActionBlockItem(BlockAction.Show));
            _testUtility.TestBlock.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.GreaterThan, 70));
            _testUtility.TestBlock.BlockItems.Add(new ItemLevelBlockItem(FilterPredicateOperator.LessThanOrEqual, 85));
            _testUtility.TestBlock.BlockItems.Add(new DropLevelBlockItem(FilterPredicateOperator.NotEqual, 56));
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
            _testUtility.TestBlock.BlockItems.Add(new SocketsBlockItem(FilterPredicateOperator.LessThanOrEqual, 6));
            _testUtility.TestBlock.BlockItems.Add(new LinkedSocketsBlockItem(FilterPredicateOperator.GreaterThanOrEqual, 4));
            _testUtility.TestBlock.BlockItems.Add(new WidthBlockItem(FilterPredicateOperator.Equal, 3));
            _testUtility.TestBlock.BlockItems.Add(new HeightBlockItem(FilterPredicateOperator.LessThanOrEqual, 6));
            _testUtility.TestBlock.BlockItems.Add(new HeightBlockItem(FilterPredicateOperator.GreaterThanOrEqual, 2));
            _testUtility.TestBlock.BlockItems.Add(new TextColorBlockItem(new Color {A = 56, R = 255, G = 89, B = 0}));
            _testUtility.TestBlock.BlockItems.Add(new BackgroundColorBlockItem(new Color { A = 255, R = 0, G = 0, B = 0 }));
            _testUtility.TestBlock.BlockItems.Add(new BorderColorBlockItem(new Color { A = 255, R = 255, G = 1, B = 254 }));
            _testUtility.TestBlock.BlockItems.Add(new FontSizeBlockItem(50));
            _testUtility.TestBlock.BlockItems.Add(new SoundBlockItem(6, 90));

            // Act
            var result = _testUtility.Translator.TranslateItemFilterBlockToString(_testUtility.TestBlock);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        private class ItemFilterBlockTranslatorTestUtility
        {
            public ItemFilterBlockTranslatorTestUtility()
            {
                // Test Data
                TestBlock = new ItemFilterBlock();

                // Mock setups
                MockBlockGroupHierarchyBuilder = new Mock<IBlockGroupHierarchyBuilder>();

                // Class under test instantiation
                Translator = new ItemFilterBlockTranslator(MockBlockGroupHierarchyBuilder.Object);
            }

            public ItemFilterBlock TestBlock { get; set; }
            public Mock<IBlockGroupHierarchyBuilder> MockBlockGroupHierarchyBuilder { get; private set; }
            public ItemFilterBlockTranslator Translator { get; private set; }
        }
    }
}
