using System.Threading.Tasks;
using Filtration.ObjectModel;
using Filtration.Services;
using Filtration.ViewModels;

namespace Filtration.Repositories
{
    internal interface IItemFilterScriptRepository
    {
        Task<IItemFilterScriptViewModel> LoadScriptFromFileAsync(string path);
        IItemFilterScriptViewModel NewScript();
        string GetItemFilterScriptDirectory();
        void SetItemFilterScriptDirectory(string path);
    }

    internal class ItemFilterScriptRepository : IItemFilterScriptRepository
    {
        private readonly IItemFilterPersistenceService _itemFilterPersistenceService;
        private readonly IItemFilterScriptViewModelFactory _itemFilterScriptViewModelFactory;

        public ItemFilterScriptRepository(IItemFilterPersistenceService itemFilterPersistenceService,
                                          IItemFilterScriptViewModelFactory itemFilterScriptViewModelFactory)
        {
            _itemFilterPersistenceService = itemFilterPersistenceService;
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
            var newScript = new ItemFilterScript();
            var newViewModel = _itemFilterScriptViewModelFactory.Create();
            newViewModel.Initialise(newScript, true);

            return newViewModel;
        }
        
        public void SetItemFilterScriptDirectory(string path)
        {
            _itemFilterPersistenceService.SetItemFilterScriptDirectory(path);
        }

        public string GetItemFilterScriptDirectory()
        {
            return _itemFilterPersistenceService.ItemFilterScriptDirectory;
        }
    }
}
