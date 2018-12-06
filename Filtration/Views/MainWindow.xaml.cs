using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

        private async void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            var allDocumentsClosed = await _mainWindowViewModel.CloseAllDocumentsAsync();
            if (!allDocumentsClosed)
            {
                e.Cancel = true;
            }
        }

        private async void MainWindow_OnDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
            var droppedFilterFiles = new List<string>();

            foreach (var filename in filenames)
            {
                var extension = Path.GetExtension(filename);
                if (extension != null &&
                    (extension.ToUpperInvariant() == ".FILTER" || extension.ToUpperInvariant() == ".FILTERTHEME"))
                {
                    droppedFilterFiles.Add(filename);
                }
            }

            if (droppedFilterFiles.Count > 0)
            {
                await _mainWindowViewModel.OpenDroppedFilesAsync(droppedFilterFiles);
            }
        }
    }
}
