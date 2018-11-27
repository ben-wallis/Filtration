using System.Threading.Tasks;
using Filtration.ObjectModel.Factories;
using Filtration.Services;
using Filtration.ViewModels;
using Filtration.ViewModels.Factories;

namespace Filtration.Repositories
{
    internal interface IItemFilterScriptRepository
    {
        Task<IItemFilterScriptViewModel> LoadScriptFromFileAsync(string path);
        IItemFilterScriptViewModel NewScript();
    }

    internal class ItemFilterScriptRepository : IItemFilterScriptRepository
    {
        private readonly IItemFilterPersistenceService _itemFilterPersistenceService;
        private readonly IItemFilterScriptFactory _itemFilterScriptFactory;
        private readonly IItemFilterScriptViewModelFactory _itemFilterScriptViewModelFactory;

        public ItemFilterScriptRepository(IItemFilterPersistenceService itemFilterPersistenceService,
                                          IItemFilterScriptFactory itemFilterScriptFactory,
                                          IItemFilterScriptViewModelFactory itemFilterScriptViewModelFactory)
        {
            _itemFilterPersistenceService = itemFilterPersistenceService;
            _itemFilterScriptFactory = itemFilterScriptFactory;
            _itemFilterScriptViewModelFactory = itemFilterScriptViewModelFactory;
        }

        public async Task<IItemFilterScriptViewModel> LoadScriptFromFileAsync(string path)
        {
            var loadedScript = await _itemFilterPersistenceService.LoadItemFilterScriptAsync(path);

            var newViewModel = _itemFilterScriptViewModelFactory.Create();
            newViewModel.Initialise(loadedScript, false);

            return newViewModel;
        }

        public IItemFilterScriptViewModel NewScript()
        {
            var newScript = _itemFilterScriptFactory.Create();
            var newViewModel = _itemFilterScriptViewModelFactory.Create();
            newViewModel.Initialise(newScript, true);

            return newViewModel;
        }
    }
}
;