using System.IO;
using System.Windows;
using Filtration.Common.Services;
using Filtration.Properties;
using Filtration.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Filtration.ViewModels
{
    internal interface ISettingsPageViewModel
    {
        RelayCommand SetItemFilterScriptDirectoryCommand { get; }

        string DefaultFilterDirectory { get; }
        bool ExtraLineBetweenBlocks { get; set; }
        bool DownloadPrereleaseUpdates { get; set; }
    }

    internal class SettingsPageViewModel : ViewModelBase, ISettingsPageViewModel
    {
        private readonly IItemFilterPersistenceService _itemFilterPersistenceService;
        private readonly IMessageBoxService _messageBoxService;

        public SettingsPageViewModel(IItemFilterPersistenceService itemFilterPersistenceService, IMessageBoxService messageBoxService)
        {
            _itemFilterPersistenceService = itemFilterPersistenceService;
            _messageBoxService = messageBoxService;
            SetItemFilterScriptDirectoryCommand = new RelayCommand(OnSetItemFilterScriptDirectoryCommand);
        }

        public RelayCommand SetItemFilterScriptDirectoryCommand { get; }

        public string DefaultFilterDirectory => Settings.Default.DefaultFilterDirectory;

        public bool ExtraLineBetweenBlocks
        {
            get => Settings.Default.ExtraLineBetweenBlocks;
            set => Settings.Default.ExtraLineBetweenBlocks = value;
        }

        public bool DownloadPrereleaseUpdates
        {
            get => Settings.Default.DownloadPrereleaseUpdates;
            set => Settings.Default.DownloadPrereleaseUpdates = value;
        }

        private void OnSetItemFilterScriptDirectoryCommand()
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                var result = dialog.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    try
                    {
                        _itemFilterPersistenceService.SetItemFilterScriptDirectory(dialog.FileName);
                        RaisePropertyChanged(nameof(DefaultFilterDirectory));
                    }
                    catch (DirectoryNotFoundException)
                    {
                        _messageBoxService.Show("Error", "The entered Default Filter Directory is invalid or does not exist.",
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
        }
    }
}
