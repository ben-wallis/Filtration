using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Filtration.ObjectModel;

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
            set
            {
                SetValue(FilteredItemsProperty, value);
            }
        }
        
        private void LootCanvas_OnSourceUpdated(object sender, DataTransferEventArgs e)
        {
            var canvas = sender as Canvas;

        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var canvas = GetItemsPanel(FilteredItemsControl) as Canvas;
            if (canvas == null) return;
            var rand = new Random();

            foreach (var child in canvas.Children.OfType<ContentPresenter>())
            {
                Canvas.SetLeft(child, rand.Next((int)(canvas.ActualWidth - child.ActualWidth)));
                Canvas.SetTop(child, rand.Next((int)(canvas.ActualHeight - child.ActualHeight)));
            }
        }

        private static Panel GetItemsPanel(DependencyObject itemsControl)
        {
            var itemsPresenter = GetVisualChild<ItemsPresenter>(itemsControl);
            var itemsPanel = VisualTreeHelper.GetChild(itemsPresenter, 0) as Panel;
            return itemsPanel;
        }
        
        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            var child = default(T);

            var numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < numVisuals; i++)
            {
                var v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

    }
}
