using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Filtration.ItemFilterPreview.Model;

namespace Filtration.ItemFilterPreview.UserControls
{
    public partial class LootExplosionSceneUserControl : UserControl
    {
        public LootExplosionSceneUserControl()
        {
            InitializeComponent();
        }
        
        public static readonly DependencyProperty FilteredItemsProperty = DependencyProperty.Register(
            "FilteredItems",
            typeof(IEnumerable<IFilteredItem>),
            typeof(LootExplosionSceneUserControl),
            new FrameworkPropertyMetadata()
            );

        public IEnumerable<IFilteredItem> FilteredItems
        {
            get { return (IEnumerable<IFilteredItem>)GetValue(FilteredItemsProperty); }
            set { SetValue(FilteredItemsProperty, value); }
        }

    }
}
