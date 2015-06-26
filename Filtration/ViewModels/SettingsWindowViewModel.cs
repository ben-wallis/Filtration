using System;
using System.IO;
using System.Windows;
using Filtration.Common.ViewModels;
using Filtration.Properties;
using Filtration.Services;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    internal interface ISettingsWindowViewModel
    {
        event EventHandler OnRequestClose;
    }

    internal class SettingsWindowViewModel : FiltrationViewModelBase, ISettingsWindowViewModel
    {
        private readonly IItemFilterPersistenceService _itemFilterPersistenceService;

        public SettingsWindowViewModel(IItemFilterPersistenceService itemFilterPersistenceService)
        {
            _itemFilterPersistenceService = itemFilterPersistenceService;
            CancelCommand = new RelayCommand(OnCancelCommand);
            SaveCommand = new RelayCommand(OnSaveCommand);

            DefaultFilterDirectory = Settings.Default.DefaultFilterDirectory;
            ExtraLineBetweenBlocks = Settings.Default.ExtraLineBetweenBlocks;
        }

        public event EventHandler OnRequestClose;

        public RelayCommand CancelCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }

        public string DefaultFilterDirectory { get; set; }
        public bool ExtraLineBetweenBlocks { get; set; }

        private void OnCancelCommand()
        {
            if (OnRequestClose != null)
            {
                OnRequestClose(this, new EventArgs());
            }
        }

        private void OnSaveCommand()
        {
            try
            {
                _itemFilterPersistenceService.SetItemFilterScriptDirectory(DefaultFilterDirectory);

                Settings.Default.ExtraLineBetweenBlocks = ExtraLineBetweenBlocks;
                if (OnRequestClose != null)
                {
                    OnRequestClose(this, new EventArgs());
                }
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show("The entered Default Filter Directory is invalid or does not exist.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}
