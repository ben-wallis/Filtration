using System.Windows;

namespace Filtration.Views
{
    public partial class ReplaceColorsWindow
    {
        public ReplaceColorsWindow()
        {
            InitializeComponent();
        }

        private void ReplaceColorsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var curApp = Application.Current;
            var mainWindow = curApp.MainWindow;
            Left = mainWindow.Left + (mainWindow.Width - ActualWidth) / 2;
            Top = mainWindow.Top + (mainWindow.Height - ActualHeight) / 2;
        }
    }
}
