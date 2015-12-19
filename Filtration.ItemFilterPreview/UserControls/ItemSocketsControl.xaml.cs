using System.Windows;

namespace Filtration.ItemFilterPreview.UserControls
{
    public partial class ItemSocketsControl
    {
        public ItemSocketsControl()
        {
            InitializeComponent();
        }

        private void ItemSocketsControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            const double ratio = 2d/3d;
            Width = Height * ratio;
        }
    }
}
