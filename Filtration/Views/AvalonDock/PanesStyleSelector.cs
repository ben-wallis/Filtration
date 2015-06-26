using System.Windows;
using System.Windows.Controls;
using Filtration.Interface;
using Filtration.ViewModels.ToolPanes;

namespace Filtration.Views.AvalonDock
{
    class PanesStyleSelector : StyleSelector
    {
        public Style ToolStyle { get; set; }
        public Style DocumentStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is ToolViewModel)
            {
                return ToolStyle;
            }

            if (item is IDocument)
            {
                return DocumentStyle;
            }
            
            return base.SelectStyle(item, container);
        }
    }
}
