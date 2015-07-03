using System;
using System.IO;
using Filtration.ObjectModel;
using Filtration.Services;
using Filtration.ViewModels;

namespace Filtration.Repositories
{
    internal interface IItemFilterScriptRepository
    {
        IItemFilterScriptViewModel LoadScriptFromFile(string path);
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

        public IItemFilterScriptViewModel LoadScriptFromFile(string path)
        {
            var loadedScript = _itemFilterPersistenceService.LoadItemFilterScript(path);

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
