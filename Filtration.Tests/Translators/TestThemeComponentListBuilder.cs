using System.Net.Mime;
using System.Windows.Media;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.Translators;
using NUnit.Framework;

namespace Filtration.Tests.Translators
{
    [TestFixture]
    public class TestThemeComponentListBuilder
    {
        [Test]
        public void AddComponent_ReturnsFirstAddedComponent_WhenComponentAddedTwice()
        {
            // Arrange

            var testInputTargetType = ThemeComponentType.TextColor;
            var testInputComponentName = "testComponent";
            var testInputColor = new Color();

            var builder = new ThemeComponentListBuilder();
            builder.Initialise();

            // Act
            var firstResult = builder.AddComponent(testInputTargetType, testInputComponentName, testInputColor);
            var secondResult = builder.AddComponent(testInputTargetType, testInputComponentName, testInputColor);

            // Assert
            Assert.AreSame(firstResult, secondResult);
        }
        
        [Test]
        public void IsInitialised_NotInitialised_ReturnsFalse()
        {
            // Arrange
            var builder = new ThemeComponentListBuilder();

            // Act
            var result = builder.IsInitialised;

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsInitialised_Initialised_ReturnsTrue()
        {
            // Arrange
            var builder = new ThemeComponentListBuilder();
            builder.Initialise();

            // Act
            var result = builder.IsInitialised;

            // Assert
            Assert.IsTrue(result);
        }
    }
}
