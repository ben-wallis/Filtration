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
        bool ExtraLineBetweenBlocks { get; set; }
        bool DownloadPrereleaseUpdates { get; set; }
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
            _itemFilterScriptDirectoryService.SetItemFilterScriptDirectory();
            RaisePropertyChanged(nameof(DefaultFilterDirectory));
        }
    }
}
