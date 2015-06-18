using System.Linq;
using Filtration.ViewModels;

namespace Filtration.Utilities
{
    internal class AutoMapperHelpers
    {
        public static void ItemFilterBlockGroupViewModelPostMap(ItemFilterBlockGroupViewModel viewModel)
        {
            foreach (var childViewModel in viewModel.ChildGroups)
            {
                ItemFilterBlockGroupViewModelPostMap(childViewModel);
            }

            if (viewModel.ChildGroups.Count > 0)
            {
                if (viewModel.ChildGroups.All(g => g.IsChecked == true))
                {
                    viewModel.IsChecked = true;
                } else if (viewModel.ChildGroups.Any(g => g.IsChecked == true))
                {
                    viewModel.IsChecked = null;
                }
                else
                {
                    viewModel.IsChecked = false;
                }
            }
        }
    }
}
