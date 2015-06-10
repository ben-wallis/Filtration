using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    internal interface IStartPageViewModel : IDocument
    {
        void Initialise(IMainWindowViewModel mainWindowViewModel);
    }

    internal class StartPageViewModel : PaneViewModel, IStartPageViewModel
    {
        private IMainWindowViewModel _mainWindowViewModel;

        public StartPageViewModel()
        {
            Title = "Start Page";
        }

        public void Initialise(IMainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }

        public RelayCommand OpenScriptCommand { get { return _mainWindowViewModel.OpenScriptCommand; } }
        public RelayCommand NewScriptCommand { get { return _mainWindowViewModel.NewScriptCommand; } }

        public bool IsScript { get { return false; } }
    }
}
