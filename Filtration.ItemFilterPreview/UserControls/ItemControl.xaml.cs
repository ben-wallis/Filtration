using System.Windows;
using Filtration.ObjectModel;

namespace Filtration.ItemFilterPreview.UserControls
{
    public partial class ItemControl
    {
        public ItemControl()
        {
            InitializeComponent();
            // ReSharper disable once PossibleNullReferenceException
            (Content as FrameworkElement).DataContext = this;
        }

        public static readonly DependencyProperty FilteredItemProperty = DependencyProperty.Register(
            "FilteredItem",
            typeof (IFilteredItem),
            typeof (ItemControl),
            new FrameworkPropertyMetadata()
            );
        
        public IFilteredItem FilteredItem
        {
            get { return (IFilteredItem)GetValue(FilteredItemProperty); }
            set { SetValue(FilteredItemProperty, value); }
        }
        
    }
}
