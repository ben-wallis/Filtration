using Filtration.Models;
using Filtration.Translators;

namespace Filtration.Services
{
    internal interface IItemFilterPersistenceService
    {
        string ItemFilterScriptDirectory { get; set; }
        ItemFilterScript LoadItemFilterScript(string filePath);
        void SaveItemFilterScript(ItemFilterScript script);
        string DefaultPathOfExileDirectory();
    }

    internal class ItemFilterPersistenceService : IItemFilterPersistenceService
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly IItemFilterScriptTranslator _itemFilterScriptTranslator;

        public ItemFilterPersistenceService(IFileSystemService fileSystemService, IItemFilterScriptTranslator itemFilterScriptTranslator)
        {
            _fileSystemService = fileSystemService;
            _itemFilterScriptTranslator = itemFilterScriptTranslator;
        }

        public string ItemFilterScriptDirectory { get; set; }

        public ItemFilterScript LoadItemFilterScript(string filePath)
        {
            var script = 
                _itemFilterScriptTranslator.TranslateStringToItemFilterScript(
                    _fileSystemService.ReadFileAsString(filePath));

            script.FilePath = filePath;
            return script;
        }

        public void SaveItemFilterScript(ItemFilterScript script)
        {
            _fileSystemService.WriteFileFromString(script.FilePath,
                _itemFilterScriptTranslator.TranslateItemFilterScriptToString(script));
        }

        public string DefaultPathOfExileDirectory()
        {
            var defaultDir = _fileSystemService.GetUserProfilePath() + "\\Documents\\My Games\\Path of Exile";
            var defaultDirExists = _fileSystemService.DirectoryExists(defaultDir);

            return defaultDirExists ? defaultDir : string.Empty;
        }
    }
}
