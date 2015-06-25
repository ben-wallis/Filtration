using System.Windows.Media;
using Filtration.ObjectModel.BlockItemTypes;
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

            var testInputTargetType = typeof (TextColorBlockItem);
            var testInputComponentName = "testComponent";
            var testInputColor = new Color();

            var builder = new ThemeComponentListBuilder();

            // Act
            var firstResult = builder.AddComponent(testInputTargetType, testInputComponentName, testInputColor);
            var secondResult = builder.AddComponent(testInputTargetType, testInputComponentName, testInputColor);

            // Assert
            Assert.AreSame(firstResult, secondResult);
        }
    }
}
