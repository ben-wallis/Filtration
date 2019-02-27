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
        int AutosaveInterval { get; set; }

        void SetAutosaveTimer(System.Timers.Timer timer);
    }

    internal class SettingsPageViewModel : ViewModelBase, ISettingsPageViewModel
    {
        private readonly IItemFilterScriptDirectoryService _itemFilterScriptDirectoryService;
        private System.Timers.Timer _autosaveTimer;

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

        public int AutosaveInterval
        {
            get => Settings.Default.AutosaveInterval;
            set
            {
                Settings.Default.AutosaveInterval = value;
                if (_autosaveTimer != null)
                {
                    if (value < 0)
                    {
                        _autosaveTimer.Stop();
                    }
                    else if (_autosaveTimer.Interval != value)
                    {
                        _autosaveTimer.Stop();
                        _autosaveTimer.Interval = value;
                        _autosaveTimer.Start();
                    }
                }
            }
        }

        public void SetAutosaveTimer(System.Timers.Timer timer)
        {
            if (_autosaveTimer == null)
            {
                _autosaveTimer = timer;
            }
        }

        private void OnSetItemFilterScriptDirectoryCommand()
        {
            _itemFilterScriptDirectoryService.SetItemFilterScriptDirectory();
            RaisePropertyChanged(nameof(DefaultFilterDirectory));
        }
    }
}
