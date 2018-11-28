using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels.DesignTime
{
    public class DesignTimeSettingsPageViewModel : ISettingsPageViewModel
    {
        public RelayCommand SetItemFilterScriptDirectoryCommand { get; }

        public string DefaultFilterDirectory
        {
            get => @"c:\users\longusernamehere\Documents\My Games\Path of Exile";
            set { }
        }

        public bool BlocksExpandedOnOpen { get; set; }

        public bool ExtraLineBetweenBlocks
        {
            get => true;
            set { }
        }

        public bool DownloadPrereleaseUpdates { get; set; }
    }
}
