using System.Windows;
using System.Windows.Controls;
using Filtration.Models;
using Filtration.ViewModels;

namespace Filtration.Views
{
    public class BlockTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var viewModel = item as LootFilterBlockViewModel;
            var element = container as FrameworkElement;

            if (viewModel == null || element == null)
                return null;

            if (viewModel.Block is LootFilterSection)
            {
                return element.FindResource("LootFilterSectionTemplate") as DataTemplate;
            }

            return element.FindResource("LootFilterBlockTemplate") as DataTemplate;
        }
    }
}
