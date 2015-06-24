﻿using System.IO;
using Filtration.ObjectModel;
using Filtration.Services;
using Filtration.Translators;
using Moq;
using NUnit.Framework;

namespace Filtration.Tests.Services
{
    [TestFixture]
    public class TestItemFilterPersistenceService
    {
        [Test]
        public void LoadItemFilterScript_CallsTranslatorAndFileSystemService()
        {
            // Arrange
            const string TestInputPath = "C:\\Test Path\\Script.Filter";
            const string TestScriptString = "This is a test item filter script";
            var testItemFilterScript = new ItemFilterScript();

            var mockFileSystemService = new Mock<IFileSystemService>();
            mockFileSystemService.Setup(s => s.ReadFileAsString(TestInputPath)).Returns(TestScriptString).Verifiable();

            var mockItemFilterScriptTranslator = new Mock<IItemFilterScriptTranslator>();
            mockItemFilterScriptTranslator.Setup(t => t.TranslateStringToItemFilterScript(TestScriptString)).Returns(testItemFilterScript).Verifiable();

            var service = new ItemFilterPersistenceService(mockFileSystemService.Object, mockItemFilterScriptTranslator.Object);

            // Act
            var script = service.LoadItemFilterScript(TestInputPath);

            // Assert
            mockFileSystemService.Verify();
            mockItemFilterScriptTranslator.Verify();
            Assert.AreEqual(testItemFilterScript, script);
        }

        [Test]
        public void SaveItemFilterScript_CallsTranslatorAndFileSystemService()
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
            service.SaveItemFilterScript(testScript);

            // Assert
            mockFileSystemService.Verify();
            mockItemFilterScriptTranslator.Verify();
        }

        [Test]
        public void DefaultPathOfExileDirectoryExists_CallsFileSystemServiceWithCorrectString()
        {
            // Arrange
            const string TestUserProfilePath = "C:\\Users\\TestUser";


            var mockFileSystemService = new Mock<IFileSystemService>();
            mockFileSystemService.Setup(f => f.GetUserProfilePath()).Returns(TestUserProfilePath).Verifiable();
            mockFileSystemService.Setup(f => f.DirectoryExists(TestUserProfilePath + "\\Documents\\My Games\\Path of Exile")).Returns(true).Verifiable();

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
