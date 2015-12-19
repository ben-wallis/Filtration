using System.Collections.Generic;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.UserControls.DesignTime
{
    internal class DesignTimeEditableListBoxControl : IEditableListBoxControl
    {
        public RelayCommand AddItemCommand { get; }
        public RelayCommand<string> DeleteItemCommand { get; }
        public IEnumerable<string> AutoCompleteItemsSource { get; set; }
        ICollection<string> IEditableListBoxControl.ItemsSource { get; set; }

        public string Label
        {
            get { return "Base Types"; }
            set {  }
        }

        public string AddItemText { get; set; }

        public ICollection<string> ItemsSource
        {
            get { return new List<string> {"Test Item 1", "Blah", "Another Item"}; }
        }

        public string SelectedItem { get { return "Blah"; } }
    }
}