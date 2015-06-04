using Filtration.ViewModels;

namespace Filtration.Views
{
    internal partial class MainWindow : IMainWindow
    {
        public MainWindow(IMainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            DataContext = mainWindowViewModel;
        }
    }
}
