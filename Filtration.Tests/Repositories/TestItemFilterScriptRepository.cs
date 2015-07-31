using System.IO;
using System.Threading.Tasks;
using Filtration.ObjectModel;
using Filtration.Repositories;
using Filtration.Services;
using Filtration.ViewModels;
using Moq;
using NUnit.Framework;

namespace Filtration.Tests.Repositories
{
    [TestFixture]
    public class TestItemFilterScriptRepository
    {
        [Test]
        public async Task LoadScriptFromFile_CallsPersistenceServiceUsingPathAndReturnsViewModel()
        {
            // Arrange
            var testInputPath = "C:\\TestPath.filter";
            

            var mockPersistenceService = new Mock<IItemFilterPersistenceService>();
            mockPersistenceService.Setup(p => p.LoadItemFilterScriptAsync(testInputPath)).ReturnsAsync(new ItemFilterScript()).Verifiable();
            
            var mockItemFilterScriptViewModel = new Mock<IItemFilterScriptViewModel>();
            
            var mockItemFilterScriptViewModelFactory = new Mock<IItemFilterScriptViewModelFactory>();
            mockItemFilterScriptViewModelFactory.Setup(f => f.Create()).Returns(mockItemFilterScriptViewModel.Object);

            var repository = new ItemFilterScriptRepository(mockPersistenceService.Object, mockItemFilterScriptViewModelFactory.Object);

            // Act
            var result = await repository.LoadScriptFromFileAsync(testInputPath);

            // Assert
            mockPersistenceService.Verify();
            Assert.AreEqual(mockItemFilterScriptViewModel.Object, result);
        }

        [Test]
        public void LoadScriptFromFile_PersistenceServiceThrows_ThrowsIOException()
        {
            // Arrange
            var testInputPath = "C:\\TestPath.filter";

            var mockPersistenceService = new Mock<IItemFilterPersistenceService>();
            mockPersistenceService.Setup(p => p.LoadItemFilterScriptAsync(testInputPath)).Throws<IOException>();

            var mockItemFilterScriptViewModelFactory = new Mock<IItemFilterScriptViewModelFactory>();

            var repository = new ItemFilterScriptRepository(mockPersistenceService.Object, mockItemFilterScriptViewModelFactory.Object);

            // Act
            
            // Assert
            Assert.Throws<IOException>(async () => await repository.LoadScriptFromFileAsync(testInputPath));
        }

        [Test]
        public void SetItemFilterScriptDirectory_CallsPersistenceServiceSetItemFilterScriptDirectory()
        {
            // Arrange
            var testInputPath = "C:\\Test\\Path";

            var mockPersistenceService = new Mock<IItemFilterPersistenceService>();
            mockPersistenceService.Setup(p => p.SetItemFilterScriptDirectory(testInputPath)).Verifiable();

            var mockItemFilterScriptViewModelFactory = new Mock<IItemFilterScriptViewModelFactory>();

            var repository = new ItemFilterScriptRepository(mockPersistenceService.Object, mockItemFilterScriptViewModelFactory.Object);

            // Act
            repository.SetItemFilterScriptDirectory(testInputPath);

            // Assert
            mockPersistenceService.Verify();
        }

        [Test]
        public void GetItemFilterScriptDirectory_ReturnsItemFilterScriptDirectoryFromPersistenceService()
        {
            // Arrange
            var testInputPath = "C:\\Test\\Path";

            var mockPersistenceService = new Mock<IItemFilterPersistenceService>();
            mockPersistenceService.SetupGet(p => p.ItemFilterScriptDirectory).Returns(testInputPath).Verifiable();

            var mockItemFilterScriptViewModelFactory = new Mock<IItemFilterScriptViewModelFactory>();

            var repository = new ItemFilterScriptRepository(mockPersistenceService.Object, mockItemFilterScriptViewModelFactory.Object);

            // Act
            string result =  repository.GetItemFilterScriptDirectory();

            // Assert
            mockPersistenceService.Verify();
            Assert.AreEqual(result, testInputPath);
        }

        [Test]
        public void NewScript_ReturnsScriptFromViewModelFactory()
        {
            // Arrange
            var mockPersistenceService = new Mock<IItemFilterPersistenceService>();
            
            var mockItemFilterScriptViewModel = new Mock<IItemFilterScriptViewModel>();

            var mockItemFilterScriptViewModelFactory = new Mock<IItemFilterScriptViewModelFactory>();
            mockItemFilterScriptViewModelFactory.Setup(f => f.Create()).Returns(mockItemFilterScriptViewModel.Object);

            var repository = new ItemFilterScriptRepository(mockPersistenceService.Object, mockItemFilterScriptViewModelFactory.Object);
            
            // Act
            IItemFilterScriptViewModel result = repository.NewScript();

            // Assert
            Assert.AreEqual(mockItemFilterScriptViewModel.Object, result);
        }
    }
}
