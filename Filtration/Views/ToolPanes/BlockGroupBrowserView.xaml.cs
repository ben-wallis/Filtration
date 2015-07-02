using System.Windows;
using System.Windows.Controls;

namespace Filtration.Views.ToolPanes
{
    public partial class BlockGroupBrowserView
    {
        public BlockGroupBrowserView()
        {
            InitializeComponent();
        }

        private void BlockGroupCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            // Prevents the user from putting a ThreeState checkbox into the indeterminate state
            // allowing the indeterminate state to only be set by the object itself.
            var cb = e.Source as CheckBox;
            if (cb != null && !cb.IsChecked.HasValue)
            {
                cb.IsChecked = false;
            }
        }
    }
}
