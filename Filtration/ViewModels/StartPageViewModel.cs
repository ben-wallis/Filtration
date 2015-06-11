using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    internal interface IStartPageViewModel : IDocument
    {
    }

    internal class StartPageViewModel : PaneViewModel, IStartPageViewModel
    {

        public StartPageViewModel()
        {
            Title = "Start Page";
        }
        
        // TODO: Replace with MVVMLight ViewModel Messages
        public RelayCommand OpenScriptCommand { get { return null; } }
        public RelayCommand NewScriptCommand { get { return null; } }

        public bool IsScript { get { return false; } }
    }
}
