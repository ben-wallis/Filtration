using System.Linq;
using Filtration.ViewModels.ToolPanes;
using Xceed.Wpf.AvalonDock.Layout;

namespace Filtration.Views.AvalonDock
{
    class LayoutInitializer : ILayoutUpdateStrategy
    {
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            //AD wants to add the anchorable into destinationContainer
            //just for test provide a new anchorablepane 
            //if the pane is floating let the manager go ahead
            LayoutAnchorablePane destPane = destinationContainer as LayoutAnchorablePane;
            if (destinationContainer?.FindParent<LayoutFloatingWindow>() != null)
                return false;

            if (anchorableToShow.Content is CommentBlockBrowserViewModel)
            {
                var toolsPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == "SectionBrowserPane");
                if (toolsPane != null)
                {
                   // anchorableToShow.CanHide = false;
                    toolsPane.Children.Add(anchorableToShow);
                    return true;
                }
            }

            if (anchorableToShow.Content is BlockGroupBrowserViewModel)
            {
                var toolsPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == "BlockGroupBrowserPane");
                if (toolsPane != null)
                {
                   // anchorableToShow.CanHide = false;
                    toolsPane.Children.Add(anchorableToShow);
                    return true;
                }
            }

            if (anchorableToShow.Content is BlockOutputPreviewViewModel)
            {
                var toolsPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == "BlockOutputPreviewPane");
                if (toolsPane != null)
                {
                    toolsPane.Children.Add(anchorableToShow);
                    return true;
                }
            }

            return false;
        }


        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {
        }


        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {

        }
    }
}
