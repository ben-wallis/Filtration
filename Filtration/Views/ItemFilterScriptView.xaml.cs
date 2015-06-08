using System.Windows.Controls;

namespace Filtration.Views
{
    public partial class ItemFilterScriptView
    {
        public ItemFilterScriptView()
        {
            InitializeComponent();
        }

        private void SectionBrowserListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox) sender;
            BlocksListBox.ScrollIntoView(listBox.SelectedItem);
        }
    }
}
