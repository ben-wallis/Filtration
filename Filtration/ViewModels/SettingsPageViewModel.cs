using Filtration.Properties;
using Filtration.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    internal interface ISettingsPageViewModel
    {
        RelayCommand SetItemFilterScriptDirectoryCommand { get; }

        string DefaultFilterDirectory { get; }
        bool BlocksExpandedOnOpen { get; set; }
        bool DownloadPrereleaseUpdates { get; set; }
        bool ExtraLineBetweenBlocks { get; set; }
    }

    internal class SettingsPageViewModel : ViewModelBase, ISettingsPageViewModel
    {
        private readonly IItemFilterScriptDirectoryService _itemFilterScriptDirectoryService;

        public SettingsPageViewModel(IItemFilterScriptDirectoryService itemFilterScriptDirectoryService)
        {
            _itemFilterScriptDirectoryService = itemFilterScriptDirectoryService;
            SetItemFilterScriptDirectoryCommand = new RelayCommand(OnSetItemFilterScriptDirectoryCommand);
        }

        public RelayCommand SetItemFilterScriptDirectoryCommand { get; }

        public string DefaultFilterDirectory => Settings.Default.DefaultFilterDirectory;

        public bool BlocksExpandedOnOpen
        {
            get => Settings.Default.BlocksExpandedOnOpen;
            set => Settings.Default.BlocksExpandedOnOpen = value;
        }

        public bool DownloadPrereleaseUpdates
        {
            get => Settings.Default.DownloadPrereleaseUpdates;
            set => Settings.Default.DownloadPrereleaseUpdates = value;
        }

        public bool ExtraLineBetweenBlocks
        {
            get => Settings.Default.ExtraLineBetweenBlocks;
            set => Settings.Default.ExtraLineBetweenBlocks = value;
        }

        private void OnSetItemFilterScriptDirectoryCommand()
        {
            _itemFilterScriptDirectoryService.SetItemFilterScriptDirectory();
            RaisePropertyChanged(nameof(DefaultFilterDirectory));
        }
    }
}
