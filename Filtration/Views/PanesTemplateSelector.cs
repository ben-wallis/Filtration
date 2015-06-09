using System.Windows;
using System.Windows.Controls;
using Filtration.ViewModels;
using Xceed.Wpf.AvalonDock.Layout;

namespace Filtration.Views
{
    class PanesTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemFilterScriptTemplate { get; set; }

        public DataTemplate SectionBrowserTemplate { get; set; }



        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var itemAsLayoutContent = item as LayoutContent;

            if (item is ItemFilterScriptViewModel)
            {
                return ItemFilterScriptTemplate;
            }

            if (item is SectionBrowserViewModel)
            {
                return SectionBrowserTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
