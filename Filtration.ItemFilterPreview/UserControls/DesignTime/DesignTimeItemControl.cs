using System.Windows.Media;
using Filtration.ObjectModel;
using Moq;

namespace Filtration.ItemFilterPreview.UserControls.DesignTime
{
    public class DesignTimeItemControl
    {
        public IFilteredItem FilteredItem
        {
            get
            {
                return Mock.Of<IFilteredItem>(f => f.BackgroundColor == Colors.Bisque && f.TextColor == Colors.Maroon && f.BorderColor == Colors.CornflowerBlue && f.FontSize == 15);
            }
        }
    }
}
