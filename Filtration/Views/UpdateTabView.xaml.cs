using System.Windows;
using Filtration.ViewModels;

namespace Filtration.Views
{
    public partial class UpdateTabView
    {
        public UpdateTabView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var updateViewModel = (UpdateViewModel)DataContext;
            ReleaseNotesWebBrowser.NavigateToString(updateViewModel.ReleaseNotes);
        }
    }
}
