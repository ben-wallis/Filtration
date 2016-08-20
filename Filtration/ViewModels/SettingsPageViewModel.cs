using System.IO;
using System.Windows;
using Filtration.Common.Services;
using Filtration.Common.ViewModels;
using Filtration.Properties;
using Filtration.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    internal interface ISettingsPageViewModel
    {
    }

    internal class SettingsPageViewModel : ViewModelBase, ISettingsPageViewModel
    {
        private readonly IItemFilterPersistenceService _itemFilterPersistenceService;
        private readonly IMessageBoxService _messageBoxService;

        public SettingsPageViewModel(IItemFilterPersistenceService itemFilterPersistenceService, IMessageBoxService messageBoxService)
        {
            _itemFilterPersistenceService = itemFilterPersistenceService;
            _messageBoxService = messageBoxService;
            SaveCommand = new RelayCommand(OnSaveCommand);

            DefaultFilterDirectory = Settings.Default.DefaultFilterDirectory;
            ExtraLineBetweenBlocks = Settings.Default.ExtraLineBetweenBlocks;
            SuppressUpdateNotifications = Settings.Default.SuppressUpdates;
        }
        public RelayCommand SaveCommand { get; private set; }

        public string DefaultFilterDirectory { get; set; }
        public bool ExtraLineBetweenBlocks { get; set; }
        public bool SuppressUpdateNotifications { get; set; }

        private void OnSaveCommand()
        {
            try
            {
                _itemFilterPersistenceService.SetItemFilterScriptDirectory(DefaultFilterDirectory);

                Settings.Default.ExtraLineBetweenBlocks = ExtraLineBetweenBlocks;
                Settings.Default.SuppressUpdates = SuppressUpdateNotifications;
            }
            catch (DirectoryNotFoundException)
            {
                _messageBoxService.Show("Error", "The entered Default Filter Directory is invalid or does not exist.",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}
