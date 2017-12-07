using System.Collections.Specialized;
using System.Windows.Controls;

namespace Filtration.UserControls
{
    public class AutoScrollingListBox : ListBox
    {
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            ScrollIntoView(e.AddedItems[0]);
            base.OnSelectionChanged(e);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null) return;

            var newItemCount = e.NewItems.Count;

            if (newItemCount > 0)
                ScrollIntoView(e.NewItems[newItemCount - 1]);

            base.OnItemsChanged(e);
        }
    }
}
