using System.Windows.Input;
using System.Windows.Media;

namespace Filtration.Views
{
    public partial class ItemFilterBlockView
    {
        public ItemFilterBlockView()
        {
            InitializeComponent();
        }

        private void BlockExpander_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var originalSource = e.OriginalSource as Visual;
            if (originalSource != null && originalSource.IsDescendantOf(BlockItemsGrid))
            {
                return;
            }
            
            BlockExpander.IsExpanded = !BlockExpander.IsExpanded;
        }
    }
}
