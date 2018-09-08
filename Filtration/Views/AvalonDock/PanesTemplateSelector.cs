using System.Windows;
using System.Windows.Controls;
using Filtration.ThemeEditor.ViewModels;
using Filtration.ViewModels;
using Filtration.ViewModels.ToolPanes;
using Xceed.Wpf.AvalonDock.Layout;

namespace Filtration.Views.AvalonDock
{
    class PanesTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemFilterScriptTemplate { get; set; }
        public DataTemplate BlockGroupBrowserTemplate { get; set; }
        public DataTemplate SectionBrowserTemplate { get; set; }
        public DataTemplate BlockOutputPreviewTemplate { get; set; }
        public DataTemplate StartPageTemplate { get; set; }
        public DataTemplate ThemeTemplate { get; set; }
        public DataTemplate UpdateTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ItemFilterScriptViewModel)
            {
                return ItemFilterScriptTemplate;
            }

            if (item is IThemeEditorViewModel)
            {
                return ThemeTemplate;
            }

            if (item is CommentBlockBrowserViewModel)
            {
                return SectionBrowserTemplate;
            }

            if (item is BlockGroupBrowserViewModel)
            {
                return BlockGroupBrowserTemplate;
            }

            if (item is BlockOutputPreviewViewModel)
            {
                return BlockOutputPreviewTemplate;
            }

            if (item is StartPageViewModel)
            {
                return StartPageTemplate;
            }

            if (item is IUpdateViewModel)
            {
                return UpdateTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
