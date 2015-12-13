using System.Windows;
using System.Windows.Controls;
using Filtration.ViewModels;

namespace Filtration.Views.AttachedProperties
{
    internal class SelectingItemAttachedProperty
    {
        public static readonly DependencyProperty SelectingItemProperty = DependencyProperty.RegisterAttached(
            "SelectingItem",
            typeof(IItemFilterBlockViewModel),
            typeof(SelectingItemAttachedProperty),
            new PropertyMetadata(default(IItemFilterBlockViewModel), OnSelectingItemChanged));

        public static IItemFilterBlockViewModel GetSelectingItem(DependencyObject target)
        {
            return (IItemFilterBlockViewModel)target.GetValue(SelectingItemProperty);
        }

        public static void SetSelectingItem(DependencyObject target, IItemFilterBlockViewModel value)
        {
            target.SetValue(SelectingItemProperty, value);
        }

        static void OnSelectingItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox?.SelectedItem == null)
            {
                return;
            }

            listBox.Dispatcher.InvokeAsync(() =>
            {
                listBox.UpdateLayout();
                listBox.ScrollIntoView(listBox.SelectedItem);
            });
        }
    }
}
