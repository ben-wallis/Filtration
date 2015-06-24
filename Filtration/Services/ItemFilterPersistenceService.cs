using System.IO;
using Filtration.ObjectModel;
using Filtration.Translators;

namespace Filtration.Services
{
    internal interface IItemFilterPersistenceService
    {
        void SetItemFilterScriptDirectory(string path);
        string ItemFilterScriptDirectory { get; }
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

            ItemFilterScriptDirectory = DefaultPathOfExileDirectory();
        }

        public string ItemFilterScriptDirectory { get; private set; }

        public string DefaultPathOfExileDirectory()
        {
            var defaultDir = _fileSystemService.GetUserProfilePath() + "\\Documents\\My Games\\Path of Exile";
            var defaultDirExists = _fileSystemService.DirectoryExists(defaultDir);

            return defaultDirExists ? defaultDir : string.Empty;
        }

        public void SetItemFilterScriptDirectory(string path)
        {
            var validPath = _fileSystemService.DirectoryExists(path);
            if (!validPath)
            {
                throw new DirectoryNotFoundException();
            }

            ItemFilterScriptDirectory = path;
        }

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


    }
}
