using Filtration.ViewModels;

namespace Filtration.Views
{
    public interface IMainWindow
    {
        void Show();
    }

    internal partial class MainWindow : IMainWindow
    {
        public MainWindow(IMainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            DataContext = mainWindowViewModel;
        }
    }
}
