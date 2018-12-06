using System.Threading.Tasks;
using Filtration.Services;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels.DesignTime
{
    public class DesignTimeUpdateViewModel : IUpdateViewModel
    {
        public RelayCommand HideUpdateWindowCommand { get; }

        public RelayCommand NextStepCommand { get; }

        public UpdateStatus UpdateStatus => UpdateStatus.Updating;
        
        public string NextStepButtonText
        {
            get => "Downloading... (100%)"; set {} }

        public bool Visible => true;

        public bool IsInErrorState { get; }

        public string Version => "1.2.3";

        public string ReleaseNotes => @"Added support for DisableDropSound filter block items
        Added support for GemLevel filter block items
        Added support for HasExplicitMod filter block items
        Added support for StackSize filter block items
        Added support for CustomAlertSound filter block items
        Added support for MinimapIcon filter block items
        Added support for MapTier filter block items
        Added support for PlayEffect filter block items
        Added theme support for several filter blocks
            Added collapse/expand support for sections
            Added support for performing actions on whole sections(copy/move/delete etc)
        Added search box to Section Browser
        Improved parsing to support different syntaxes
        Small bugfixes";

        public bool IsScript { get; }
        public bool IsTheme { get; }
        public Task<bool> Close()
        {
            throw new System.NotImplementedException();
        }

        public RelayCommand CloseCommand { get; }
    }
}
