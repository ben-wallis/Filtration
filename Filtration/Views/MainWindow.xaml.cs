using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using Filtration.ViewModels;

namespace Filtration.Views
{
    public interface IMainWindow
    {
        void Show();
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

        private void ScriptToolsGroup_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ScriptToolsGroup.IsVisible)
            {
                RibbonRoot.SelectedTabItem = ScriptToolsTabItem;
            }
        }

        private void ThemeToolsGroup_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ThemeToolsGroup.IsVisible)
            {
                RibbonRoot.SelectedTabItem = ThemeToolsTabItem;
            }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            var allDocumentsClosed = _mainWindowViewModel.CloseAllDocuments().Result;
            if (!allDocumentsClosed)
            {
                e.Cancel = true;

            }
        }
    }
}
