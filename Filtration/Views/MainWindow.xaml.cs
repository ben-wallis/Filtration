using Filtration.ViewModels;

namespace Filtration.Views
{
    public interface IMainWindow
    {
        void Show();
        void OpenScriptFromCommandLineArgument(string scriptPath);
    }

    internal partial class MainWindow : IMainWindow
    {
        private readonly IMainWindowViewModel _mainWindowViewModel;

        public MainWindow(IMainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
            InitializeComponent();
            DataContext = mainWindowViewModel;
        }

        public void OpenScriptFromCommandLineArgument(string scriptPath)
        {
            _mainWindowViewModel.LoadScriptFromFile(scriptPath);
        }
    }
}
