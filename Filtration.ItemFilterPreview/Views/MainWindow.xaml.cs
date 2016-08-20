using Filtration.ItemFilterPreview.ViewModels;

namespace Filtration.ItemFilterPreview.Views
{
    public interface IMainWindow
    {
        void Show();
    }

    internal partial class MainWindow : IMainWindow
    {
        public MainWindow(IMainWindowViewModel mainWindowViewModel)
        {
            DataContext = mainWindowViewModel;
            InitializeComponent();
        }
    }
}
