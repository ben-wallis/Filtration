using System.Collections.ObjectModel;
using System.Linq;
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

            Mapper.Reset();
            if (showAdvanced)
            {
                Mapper.CreateMap<ItemFilterBlockGroup, ItemFilterBlockGroupViewModel>()
                    .ForMember(dest => dest.IsChecked,
                        opts => opts.MapFrom(from => from.IsChecked))
                    .ForMember(dest => dest.SourceBlockGroup,
                        opts => opts.MapFrom(from => from));
            }
            else
            {
                Mapper.CreateMap<ItemFilterBlockGroup, ItemFilterBlockGroupViewModel>()
                    .ForMember(dest => dest.IsChecked,
                        opts => opts.MapFrom(from => from.IsChecked))
                    .ForMember(dest => dest.ChildGroups,
                        opts => opts.MapFrom(from => from.ChildGroups.Where(c => c.Advanced == false)))
                    .ForMember(dest => dest.SourceBlockGroup,
                        opts => opts.MapFrom(from => from));
            }

            var mappedViewModels = Mapper.Map<ObservableCollection<ItemFilterBlockGroupViewModel>>(blockGroups);
            AutoMapperHelpers.ItemFilterBlockGroupViewModelPostMap(mappedViewModels.First());
            return mappedViewModels.First().ChildGroups;
        }
    }
}
