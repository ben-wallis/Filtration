using Filtration.Models;
using Filtration.Services;
using Filtration.Translators;
using Moq;
using NUnit.Framework;

namespace Filtration.Tests.Services
{
    [TestFixture]
    public class TestLootFilterPersistenceService
    {
        [Test]
        public void LoadLootFilterScript_CallsTranslatorAndFileSystemService()
        {
            // Arrange
            const string TestInputPath = "C:\\Test Path\\Script.Filter";
            const string TestScriptString = "This is a test loot filter script";
            var testLootFilterScript = new LootFilterScript();

            var mockFileSystemService = new Mock<IFileSystemService>();
            mockFileSystemService.Setup(s => s.ReadFileAsString(TestInputPath)).Returns(TestScriptString).Verifiable();

            var mockLootFilterScriptTranslator = new Mock<ILootFilterScriptTranslator>();
            mockLootFilterScriptTranslator.Setup(t => t.TranslateStringToLootFilterScript(TestScriptString)).Returns(testLootFilterScript).Verifiable();

            var service = new LootFilterPersistenceService(mockFileSystemService.Object, mockLootFilterScriptTranslator.Object);

            // Act
            var script = service.LoadLootFilterScript(TestInputPath);

            // Assert
            mockFileSystemService.Verify();
            mockLootFilterScriptTranslator.Verify();
            Assert.AreEqual(testLootFilterScript, script);
        }

        [Test]
        public void SaveLootFilterScript_CallsTranslatorAndFileSystemService()
        {
            // Arrange
            var testFilePath = "C:\\Test\\File.txt";
            var testScript = new LootFilterScript {FilePath = testFilePath};
            var testTranslatedScript = "Test translated script";

            var mockFileSystemService = new Mock<IFileSystemService>();
            mockFileSystemService.Setup(s => s.WriteFileFromString(testFilePath, testTranslatedScript)).Verifiable();

            var mockLootFilterScriptTranslator = new Mock<ILootFilterScriptTranslator>();
            mockLootFilterScriptTranslator.Setup(t => t.TranslateLootFilterScriptToString(testScript)).Returns(testTranslatedScript).Verifiable();

            var service = new LootFilterPersistenceService(mockFileSystemService.Object, mockLootFilterScriptTranslator.Object);

            // Act
            service.SaveLootFilterScript(testScript);

            // Assert
            mockFileSystemService.Verify();
            mockLootFilterScriptTranslator.Verify();
        }

        [Test]
        public void DefaultPathOfExileDirectoryExists_CallsFileSystemServiceWithCorrectString()
        {
            // Arrange
            const string TestUserProfilePath = "C:\\Users\\TestUser";


            var mockFileSystemService = new Mock<IFileSystemService>();
            mockFileSystemService.Setup(f => f.GetUserProfilePath()).Returns(TestUserProfilePath).Verifiable();
            mockFileSystemService.Setup(f => f.DirectoryExists(TestUserProfilePath + "\\Documents\\My Games\\Path of Exile")).Returns(true).Verifiable();

            var mockLootFilterScriptTranslator = new Mock<ILootFilterScriptTranslator>();

            var service = new LootFilterPersistenceService(mockFileSystemService.Object, mockLootFilterScriptTranslator.Object);

            // Act
            var result = service.DefaultPathOfExileDirectory();

            // Assert
            mockFileSystemService.Verify();
        }
    }
}
