using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mime;
using AutoMapper;
using Filtration.ObjectModel;
using Filtration.ViewModels;

namespace Filtration.Utilities
{
    internal interface IBlockGroupMapper
    {
        ObservableCollection<ItemFilterBlockGroupViewModel> MapBlockGroupsToViewModels(
            ObservableCollection<ItemFilterBlockGroup> blockGroups, bool showAdvanced);
    }

    internal class BlockGroupMapper : IBlockGroupMapper
    {
        public ObservableCollection<ItemFilterBlockGroupViewModel> MapBlockGroupsToViewModels(
            ObservableCollection<ItemFilterBlockGroup> blockGroups, bool showAdvanced)
        {
            
            var mappedViewModels = Mapper.Map<ObservableCollection<ItemFilterBlockGroupViewModel>>(blockGroups, opts => opts.Items["showAdvanced"] = showAdvanced);
            AutoMapperHelpers.ItemFilterBlockGroupViewModelPostMap(mappedViewModels.First());
            return mappedViewModels.First().ChildGroups;
        }
    }
}
