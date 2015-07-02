using System.IO;
using System.Windows;
using Filtration.Common.ViewModels;
using Filtration.Properties;
using Filtration.Services;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    internal interface ISettingsPageViewModel
    {
    }

    internal class SettingsPageViewModel : FiltrationViewModelBase, ISettingsPageViewModel
    {
        private readonly IItemFilterPersistenceService _itemFilterPersistenceService;

        public SettingsPageViewModel(IItemFilterPersistenceService itemFilterPersistenceService)
        {
            _itemFilterPersistenceService = itemFilterPersistenceService;
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
                MessageBox.Show("The entered Default Filter Directory is invalid or does not exist.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}
