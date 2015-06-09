using System.Windows;
using System.Windows.Controls;
using Filtration.ViewModels;

namespace Filtration.Views
{
    class PanesStyleSelector : StyleSelector
    {
        public Style ToolStyle { get; set; }
        public Style ScriptStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is ToolViewModel)
            {
                return ToolStyle;
            }

            if (item is IItemFilterScriptViewModel)
            {
                return ScriptStyle;
            }

            return base.SelectStyle(item, container);
        }
    }
}
