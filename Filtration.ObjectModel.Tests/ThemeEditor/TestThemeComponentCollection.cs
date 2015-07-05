using System.Windows.Media;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.ThemeEditor;
using NUnit.Framework;

namespace Filtration.ObjectModel.Tests.ThemeEditor
{
    [TestFixture]
    public class TestThemeComponentCollection
    {
        [Test]
        public void AddComponent_ReturnsFirstAddedComponent_WhenComponentAddedTwice()
        {
            // Arrange

            var testInputTargetType = ThemeComponentType.TextColor;
            var testInputComponentName = "testComponent";
            var testInputColor = new Color();

            var componentCollection = new ThemeComponentCollection();

            // Act
            var firstResult = componentCollection.AddComponent(testInputTargetType, testInputComponentName, testInputColor);
            var secondResult = componentCollection.AddComponent(testInputTargetType, testInputComponentName, testInputColor);

            // Assert
            Assert.AreSame(firstResult, secondResult);
        }
    }
}
