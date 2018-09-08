using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Filtration.Views
{
    public partial class AboutWindow
    {
        public AboutWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void AboutWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var curApp = Application.Current;
            var mainWindow = curApp.MainWindow;
            Left = mainWindow.Left + (mainWindow.Width - ActualWidth) / 2;
            Top = mainWindow.Top + (mainWindow.Height - ActualHeight) / 2;
        }

        public string Version
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return "Version " + fvi.ProductVersion;
            }
        }
    }
}
