using System.Windows.Media;
using Filtration.Common.Services;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.ThemeEditor;
using Filtration.ThemeEditor.Services;
using Moq;
using NUnit.Framework;

namespace Filtration.ThemeEditor.Tests.Services
{
    [TestFixture]
    public class TestThemeService
    {
        [Test]
        public void ApplyThemeToScript_SingleBlock_ReplacesColor()
        {
            // Arrange

            var testInputBlockItem = new TextColorBlockItem();
            var testInputBlock = new ItemFilterBlock();
            testInputBlock.BlockItems.Add(testInputBlockItem);
            var testInputScript = new ItemFilterScript();
            testInputScript.ItemFilterBlocks.Add(testInputBlock);
            
            var testInputTheme = new Theme();
            var testInputThemeComponentColor = new Color{ R = 255, G = 0, B = 1 };
            var testInputThemeComponent = new ColorThemeComponent(ThemeComponentType.TextColor, "Test Component 1", testInputThemeComponentColor);
            testInputTheme.Components.Add(testInputThemeComponent);
            testInputBlockItem.ThemeComponent = testInputThemeComponent;
            var mockMessageBoxService = new Mock<IMessageBoxService>();

            var service = new ThemeService(mockMessageBoxService.Object);

            // Act
            service.ApplyThemeToScript(testInputTheme, testInputScript);

            // Assert
            Assert.AreEqual(testInputThemeComponentColor, testInputBlockItem.Color);
        }
        
        [Test]
        public void ApplyThemeToScript_SingleBlockDifferentComponentName_DoesNotReplaceColour()
        {
            // Arrange

            var testInputBlockItem = new TextColorBlockItem();
            var testInputBlock = new ItemFilterBlock();
            testInputBlock.BlockItems.Add(testInputBlockItem);
            var testInputScript = new ItemFilterScript();
            testInputScript.ItemFilterBlocks.Add(testInputBlock);

            var testInputTheme = new Theme();
            var testInputThemeComponentColor = new Color { R = 255, G = 0, B = 1 };
            var testInputThemeComponent = new ColorThemeComponent(ThemeComponentType.TextColor, "Test Component 1", testInputThemeComponentColor);
            var testInputBlockItemThemeComponent = new ColorThemeComponent(ThemeComponentType.TextColor, "Different Component", testInputThemeComponentColor);
            testInputTheme.Components.Add(testInputThemeComponent);
            testInputBlockItem.ThemeComponent = testInputBlockItemThemeComponent;
            
            var mockMessageBoxService = new Mock<IMessageBoxService>();

            var service = new ThemeService(mockMessageBoxService.Object);

            // Act
            service.ApplyThemeToScript(testInputTheme, testInputScript);

            // Assert
            Assert.AreNotEqual(testInputThemeComponentColor, testInputBlockItem.Color);
        }
    }
}
