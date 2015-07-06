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
        public MainWindow(IMainWindowViewModel mainWindowViewModel)
        {
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
    }
}
