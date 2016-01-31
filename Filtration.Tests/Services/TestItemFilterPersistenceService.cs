using System.IO;
using System.Threading.Tasks;
using Filtration.Common.Services;
using Filtration.ObjectModel;
using Filtration.Parser.Interface.Services;
using Filtration.Services;
using Moq;
using NUnit.Framework;

namespace Filtration.Tests.Services
{
    [TestFixture]
    public class TestItemFilterPersistenceService
    {
        [Test]
        public async Task LoadItemFilterScript_CallsTranslatorAndFileSystemService()
        {
            // Arrange
            const string testInputPath = "C:\\Test Path\\Script.Filter";
            const string testScriptString = "This is a test item filter script";
            var testItemFilterScript = new ItemFilterScript();

            var mockFileSystemService = new Mock<IFileSystemService>();
            mockFileSystemService.Setup(s => s.ReadFileAsString(testInputPath)).Returns(testScriptString).Verifiable();

            var mockItemFilterScriptTranslator = new Mock<IItemFilterScriptTranslator>();
            mockItemFilterScriptTranslator.Setup(t => t.TranslateStringToItemFilterScript(testScriptString)).Returns(testItemFilterScript).Verifiable();

            var service = new ItemFilterPersistenceService(mockFileSystemService.Object, mockItemFilterScriptTranslator.Object);

            // Act
            var script = await service.LoadItemFilterScriptAsync(testInputPath);

            // Assert
            mockFileSystemService.Verify();
            mockItemFilterScriptTranslator.Verify();
            Assert.AreEqual(testItemFilterScript, script);
        }

        [Test]
        public async Task SaveItemFilterScript_CallsTranslatorAndFileSystemService()
        {
            // Arrange
            var testFilePath = "C:\\Test\\File.txt";
            var testScript = new ItemFilterScript {FilePath = testFilePath};
            var testTranslatedScript = "Test translated script";

            var mockFileSystemService = new Mock<IFileSystemService>();
            mockFileSystemService.Setup(s => s.WriteFileFromString(testFilePath, testTranslatedScript)).Verifiable();

            var mockItemFilterScriptTranslator = new Mock<IItemFilterScriptTranslator>();
            mockItemFilterScriptTranslator.Setup(t => t.TranslateItemFilterScriptToString(testScript)).Returns(testTranslatedScript).Verifiable();

            var service = new ItemFilterPersistenceService(mockFileSystemService.Object, mockItemFilterScriptTranslator.Object);

            // Act
            await service.SaveItemFilterScriptAsync(testScript);

            // Assert
            mockFileSystemService.Verify();
            mockItemFilterScriptTranslator.Verify();
        }

        [Test]
        public void DefaultPathOfExileDirectoryExists_CallsFileSystemServiceWithCorrectString()
        {
            // Arrange
            const string testUserProfilePath = "C:\\Users\\TestUser";


            var mockFileSystemService = new Mock<IFileSystemService>();
            mockFileSystemService.Setup(f => f.GetUserProfilePath()).Returns(testUserProfilePath).Verifiable();
            mockFileSystemService.Setup(f => f.DirectoryExists(testUserProfilePath + "\\Documents\\My Games\\Path of Exile")).Returns(true).Verifiable();

            var mockItemFilterScriptTranslator = new Mock<IItemFilterScriptTranslator>();

            var service = new ItemFilterPersistenceService(mockFileSystemService.Object, mockItemFilterScriptTranslator.Object);

            // Act
            service.DefaultPathOfExileDirectory();

            // Assert
            mockFileSystemService.Verify();
        }

        [Test]
        public void SetItemFilterScriptDirectory_InvalidPath_ThrowsDirectoryNotFoundException()
        {
            // Arrange
            var testInputPath = "C:\\Test\\Path";

            var mockFileSystemService = new Mock<IFileSystemService>();
            mockFileSystemService.Setup(f => f.DirectoryExists(testInputPath)).Returns(false).Verifiable();

            var mockItemFilterScriptTranslator = new Mock<IItemFilterScriptTranslator>();

            var service = new ItemFilterPersistenceService(mockFileSystemService.Object, mockItemFilterScriptTranslator.Object);

            // Act
            
            // Assert
            Assert.Throws<DirectoryNotFoundException>(() => service.SetItemFilterScriptDirectory(testInputPath));
        }

        [Test]
        public void SetItemFilterScriptDirectory_ValidPath_SetsItemFilterScriptDirectory()
        {
            // Arrange
            var testInputPath = "C:\\Test\\Path";

            var mockFileSystemService = new Mock<IFileSystemService>();
            mockFileSystemService.Setup(f => f.DirectoryExists(testInputPath)).Returns(true).Verifiable();

            var mockItemFilterScriptTranslator = new Mock<IItemFilterScriptTranslator>();

            var service = new ItemFilterPersistenceService(mockFileSystemService.Object, mockItemFilterScriptTranslator.Object);

            // Act
            service.SetItemFilterScriptDirectory(testInputPath);

            // Assert
            Assert.AreEqual(testInputPath, service.ItemFilterScriptDirectory);
        }
    }
}
