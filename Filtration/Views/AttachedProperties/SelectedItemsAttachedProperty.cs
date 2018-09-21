using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Filtration.Views.AttachedProperties
{
    public static class SelectedItemsAttachedProperty
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached("SelectedItems", typeof(IList), typeof(SelectedItemsAttachedProperty),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedItemsChanged)));

        public static IList GetSelectedItems(DependencyObject obj)
        {
            return (IList)obj.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(DependencyObject obj, IList value)
        {
            obj.SetValue(SelectedItemsProperty, value);
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listbox = d as ListBox;
            if (listbox != null)
            {
                listbox.SelectedItems.Clear();
                var selectedItems = e.NewValue as IList;
                if (selectedItems != null)
                {
                    foreach (var item in selectedItems)
                    {
                        listbox.SelectedItems.Add(item);
                    }

                    listbox.SelectionChanged += (s, ev) =>
                    {
                        // TODO: Temporary solution. If a block item uses AutoScrollingListBox, it may create problems.
                        if (!(ev.OriginalSource is UserControls.AutoScrollingListBox))
                            return;

                        if (null != ev.RemovedItems)
                        {
                            foreach (var item in ev.RemovedItems)
                            {
                                selectedItems.Remove(item);
                            }
                        }
                        if (null != ev.AddedItems)
                        {
                            foreach (var item in ev.AddedItems)
                            {
                                if (!selectedItems.Contains(item))
                                {
                                    selectedItems.Add(item);
                                }
                            }
                        }
                    };

                    if (selectedItems is INotifyCollectionChanged)
                    {
                        (selectedItems as INotifyCollectionChanged).CollectionChanged += (s, ev) =>
                        {
                            // If this is the case, list requires re-adding all
                            if (ev.Action == NotifyCollectionChangedAction.Reset)
                            {
                                listbox.SelectedItems.Clear();
                            }

                            if (null != ev.OldItems)
                            {
                                foreach (var item in ev.OldItems)
                                {
                                    listbox.SelectedItems.Remove(item);
                                }
                            }

                            if (null != ev.NewItems)
                            {
                                foreach (var item in ev.NewItems)
                                {
                                    if (!listbox.SelectedItems.Contains(item))
                                    {
                                        listbox.SelectedItems.Add(item);
                                    }
                                }
                            }
                        };
                    }
                }
            }
        }
    }
}
