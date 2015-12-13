using System.Windows.Media;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.ThemeEditor;
using NUnit.Framework;

namespace Filtration.ThemeEditor.Tests.Models
{
    [TestFixture]
    public class TestTheme
    {
        [Test]
        public void ComponentExists_ComponentDoesExist_ReturnsTrue()
        {
            // Arrange
            var theme = new Theme();

            var testInputComponentTargetType = ThemeComponentType.TextColor;
            const string testInputComponentName = "test";

            theme.AddComponent(testInputComponentTargetType, testInputComponentName, new Color());
            
            // Act
            var result = theme.ComponentExists(testInputComponentTargetType, testInputComponentName);

            // Assert
            Assert.AreEqual(true, result);
        }
        
        [Test]
        public void ComponentExists_ComponentDoesNotExist_DifferentNameSameType_ReturnsFalse()
        {
            // Arrange
            var theme = new Theme();

            var testInputComponentTargetType = ThemeComponentType.TextColor;
            const string testInputComponentName = "test";
            theme.AddComponent(testInputComponentTargetType, testInputComponentName, new Color());

            // Act
            var result = theme.ComponentExists(testInputComponentTargetType, "blah");

            // Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        public void ComponentExists_ComponentDoesNotExist_DifferentTypeSameName_ReturnsFalse()
        {
            // Arrange
            var theme = new Theme();

            var testInputComponentTargetType = ThemeComponentType.TextColor;
            const string testInputComponentName = "test";

            theme.AddComponent(testInputComponentTargetType, testInputComponentName, new Color());

            // Act
            var result = theme.ComponentExists(ThemeComponentType.BorderColor, testInputComponentName);

            // Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        public void ComponentExists_ComponentDoesNotExist_NoComponents_ReturnsFalse()
        {
            // Arrange
            var theme = new Theme();
            
            // Act
            var result = theme.ComponentExists(ThemeComponentType.BorderColor, "test");

            // Assert
            Assert.AreEqual(false, result);
        }
    }
}
