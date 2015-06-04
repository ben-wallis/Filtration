using System.Windows.Controls;

namespace Filtration.Views
{
    public partial class LootFilterScriptView
    {
        public LootFilterScriptView()
        {
            InitializeComponent();
        }

        private void SectionBrowserListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BlocksListBox.ScrollIntoView(((ListBox)sender).SelectedItem);
        }
    }
}
