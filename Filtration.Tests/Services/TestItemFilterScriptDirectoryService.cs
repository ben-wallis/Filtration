using Filtration.Common.Services;
using Filtration.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
using Moq;
using NUnit.Framework;

namespace Filtration.Tests.Services
{
    [TestFixture]
    public class TestItemFilterScriptDirectoryService
    {

        [Test]
        public void PromptForFilterScriptDirectoryIfRequired_ItemFilterScriptDirectoryNotNull_DoesNotSetItemFilterScriptDirectory()
        {
            //Arrange
            var mockItemFilterPersistenceService = new Mock<IItemFilterPersistenceService>();
            mockItemFilterPersistenceService.Setup(i => i.ItemFilterScriptDirectory)
                                            .Returns("testdir");

            var service = CreateItemFilterScriptDirectoryService(itemFilterPersistenceService: mockItemFilterPersistenceService.Object);

            //Act
            service.PromptForFilterScriptDirectoryIfRequired();

            //Assert
            mockItemFilterPersistenceService.Verify(i => i.SetItemFilterScriptDirectory(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void PromptForFilterScriptDirectoryIfRequired_NotSet_DefaultDirectoryExists_SetsDirectoryToDefaultDirectory()
        {
            //Arrange
            var mockItemFilterPersistenceService = new Mock<IItemFilterPersistenceService>();
            var testInputDefaultDirectory = "testdefaultdirectory";
            mockItemFilterPersistenceService.Setup(i => i.DefaultPathOfExileDirectory())
                                            .Returns(testInputDefaultDirectory);

            var mockFileSystemService = new Mock<IFileSystemService>();
            mockFileSystemService.Setup(f => f.DirectoryExists(testInputDefaultDirectory))
                                  .Returns(true);

            var service = CreateItemFilterScriptDirectoryService(fileSystemService: mockFileSystemService.Object,
                                                                 itemFilterPersistenceService: mockItemFilterPersistenceService.Object);

            //Act
            service.PromptForFilterScriptDirectoryIfRequired();

            //Assert
            mockItemFilterPersistenceService.Verify(i => i.SetItemFilterScriptDirectory(testInputDefaultDirectory));
        }

        [Test, Timeout(1000)]
        public void PromptForFilterScriptDirectoryIfRequired_NotSet_DefaultDirectoryDoesNotExist_SetsDirectoryToUserSelected()
        {
            //Arrange
            var testInputUserSelectedDirectory = "blah";

            var mockItemFilterPersistenceService = new Mock<IItemFilterPersistenceService>();
            var mockDialogService = new Mock<IDialogService>();

            mockDialogService.Setup(d => d.ShowFolderPickerDialog(It.IsAny<string>(), out testInputUserSelectedDirectory))
                             .Returns(CommonFileDialogResult.Ok);

            mockItemFilterPersistenceService.Setup(i => i.SetItemFilterScriptDirectory(testInputUserSelectedDirectory))
                                            .Callback(() => mockItemFilterPersistenceService.Setup(f => f.ItemFilterScriptDirectory)
                                                                                            .Returns(testInputUserSelectedDirectory));

            var service = CreateItemFilterScriptDirectoryService(dialogService: mockDialogService.Object,
                                                                 itemFilterPersistenceService: mockItemFilterPersistenceService.Object);

            //Act
            service.PromptForFilterScriptDirectoryIfRequired();

            //Assert
            mockItemFilterPersistenceService.Verify(i => i.SetItemFilterScriptDirectory(testInputUserSelectedDirectory));
        }

        private ItemFilterScriptDirectoryService CreateItemFilterScriptDirectoryService(IDialogService dialogService = null,
                                                                                        IFileSystemService fileSystemService = null,
                                                                                        IItemFilterPersistenceService itemFilterPersistenceService = null,
                                                                                        IMessageBoxService messageBoxService = null)
        {
            return new ItemFilterScriptDirectoryService(dialogService ?? new Mock<IDialogService>().Object,
                                                        fileSystemService ?? new Mock<IFileSystemService>().Object,
                                                        itemFilterPersistenceService ?? new Mock<IItemFilterPersistenceService>().Object,
                                                        messageBoxService ?? new Mock<IMessageBoxService>().Object);
        }
    }
}
