using System.IO;
using System.Threading.Tasks;
using Filtration.Common.Services;
using Filtration.ObjectModel;
using Filtration.Parser.Interface.Services;
using Filtration.Properties;

namespace Filtration.Services
{
    internal interface IItemFilterPersistenceService
    {
        void SetItemFilterScriptDirectory(string path);
        string ItemFilterScriptDirectory { get; }
        Task<IItemFilterScript> LoadItemFilterScriptAsync(string filePath);
        Task SaveItemFilterScriptAsync(IItemFilterScript script);
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

           ItemFilterScriptDirectory = Settings.Default.DefaultFilterDirectory;
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
            Settings.Default.DefaultFilterDirectory = path;
            Settings.Default.Save();
        }

        public async Task<IItemFilterScript> LoadItemFilterScriptAsync(string filePath)
        {
            IItemFilterScript loadedScript = null;
            await Task.Run(() =>
            {
                loadedScript = _itemFilterScriptTranslator.TranslateStringToItemFilterScript(
                    _fileSystemService.ReadFileAsString(filePath));
            });

            if (loadedScript != null)
            {
                loadedScript.FilePath = filePath;
            }

            return loadedScript;
        }

        public async Task SaveItemFilterScriptAsync(IItemFilterScript script)
        {
            await Task.Run(() =>
            {
                _fileSystemService.WriteFileFromString(script.FilePath,
                    _itemFilterScriptTranslator.TranslateItemFilterScriptToString(script));
            });
        }
    }
}
