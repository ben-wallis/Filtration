using System;
using System.IO;
using System.Threading.Tasks;
using Filtration.ObjectModel;
using Filtration.ObjectModel.Factories;
using Filtration.Repositories;
using Filtration.Services;
using Filtration.ViewModels;
using Filtration.ViewModels.Factories;
using FluentAssertions;
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

            var repository = CreateItemFilterScriptRepository(itemFilterPersistenceService: mockPersistenceService.Object,
                                                              itemFilterScriptViewModelFactory: mockItemFilterScriptViewModelFactory.Object);

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

            var repository = CreateItemFilterScriptRepository(itemFilterPersistenceService: mockPersistenceService.Object,
                                                              itemFilterScriptViewModelFactory: mockItemFilterScriptViewModelFactory.Object);

            // Act
            Func<Task<IItemFilterScriptViewModel>> result = async () => await repository.LoadScriptFromFileAsync(testInputPath);

            // Assert
            result.Should().Throw<IOException>();
        }

        [Test]
        public void NewScript_ReturnsScriptFromViewModelFactory()
        {
            // Arrange
            var mockPersistenceService = new Mock<IItemFilterPersistenceService>();
            
            var mockItemFilterScriptViewModel = new Mock<IItemFilterScriptViewModel>();

            var mockItemFilterScriptViewModelFactory = new Mock<IItemFilterScriptViewModelFactory>();
            mockItemFilterScriptViewModelFactory.Setup(f => f.Create()).Returns(mockItemFilterScriptViewModel.Object);

            var repository = CreateItemFilterScriptRepository(itemFilterPersistenceService: mockPersistenceService.Object,
                                                              itemFilterScriptViewModelFactory: mockItemFilterScriptViewModelFactory.Object);

            // Act
            IItemFilterScriptViewModel result = repository.NewScript();

            // Assert
            Assert.AreEqual(mockItemFilterScriptViewModel.Object, result);
        }

        private ItemFilterScriptRepository CreateItemFilterScriptRepository(IItemFilterPersistenceService itemFilterPersistenceService = null,
                                                                            IItemFilterScriptFactory itemFilterScriptFactory = null,
                                                                            IItemFilterScriptViewModelFactory itemFilterScriptViewModelFactory = null)
        {
            return new ItemFilterScriptRepository(itemFilterPersistenceService ?? new Mock<IItemFilterPersistenceService>().Object,
                                                  itemFilterScriptFactory ?? new Mock<IItemFilterScriptFactory>().Object,
                                                  itemFilterScriptViewModelFactory ?? new Mock<IItemFilterScriptViewModelFactory>().Object);
        }
    }
}
