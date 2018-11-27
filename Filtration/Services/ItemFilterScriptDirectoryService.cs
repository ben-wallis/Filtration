using System.IO;
using System.Windows;
using Filtration.Common.Services;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Filtration.Services
{
    internal interface IItemFilterScriptDirectoryService
    {
        string ItemFilterScriptDirectory { get; }
        void PromptForFilterScriptDirectoryIfRequired();
        void SetItemFilterScriptDirectory();
    }

    internal sealed class ItemFilterScriptDirectoryService : IItemFilterScriptDirectoryService
    {
        private readonly IDialogService _dialogService;
        private readonly IFileSystemService _fileSystemService;
        private readonly IItemFilterPersistenceService _itemFilterPersistenceService;
        private readonly IMessageBoxService _messageBoxService;

        public ItemFilterScriptDirectoryService(IDialogService dialogService,
                                                IFileSystemService fileSystemService,
                                                IItemFilterPersistenceService itemFilterPersistenceService,
                                                IMessageBoxService messageBoxService)
        {
            _dialogService = dialogService;
            _fileSystemService = fileSystemService;
            _itemFilterPersistenceService = itemFilterPersistenceService;
            _messageBoxService = messageBoxService;
        }


        public string ItemFilterScriptDirectory => _itemFilterPersistenceService.ItemFilterScriptDirectory;

        public void PromptForFilterScriptDirectoryIfRequired()
        {
            // If the directory is already set, do nothing
            if (!string.IsNullOrEmpty(_itemFilterPersistenceService.ItemFilterScriptDirectory))
            {
                return;
            }

            // If the directory is not set but the default directory exists, set the directory to the default directory
            if (_fileSystemService.DirectoryExists(_itemFilterPersistenceService.DefaultPathOfExileDirectory()))
            {
                _itemFilterPersistenceService.SetItemFilterScriptDirectory(_itemFilterPersistenceService.DefaultPathOfExileDirectory());
                return;
            }

            // Otherwise, prompt the user to select the directory
            _messageBoxService.Show("Data directory required", @"The Path of Exile user data directory was not found in the default location (Documents\My Games\Path of Exile), please select it manually.", MessageBoxButton.OK, MessageBoxImage.Information);
            while (string.IsNullOrEmpty(_itemFilterPersistenceService.ItemFilterScriptDirectory))
            {
                SetItemFilterScriptDirectory();
            }
        }

        public void SetItemFilterScriptDirectory()
        {
            var result = _dialogService.ShowFolderPickerDialog(@"Select Path of Exile user data directory", out var filepath);
            if (result != CommonFileDialogResult.Ok)
            {
                return;
            }

            try
            {
                _itemFilterPersistenceService.SetItemFilterScriptDirectory(filepath);
            }
            catch (DirectoryNotFoundException)
            {
                _messageBoxService.Show("Error", "The entered Default Filter Directory is invalid or does not exist.",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}
