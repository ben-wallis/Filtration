using Filtration.Models;
using Filtration.Translators;

namespace Filtration.Services
{
    internal interface ILootFilterPersistenceService
    {
        string LootFilterScriptDirectory { get; set; }
        LootFilterScript LoadLootFilterScript(string filePath);
        void SaveLootFilterScript(LootFilterScript script);
        string DefaultPathOfExileDirectory();
    }

    internal class LootFilterPersistenceService : ILootFilterPersistenceService
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly ILootFilterScriptTranslator _lootFilterScriptTranslator;

        public LootFilterPersistenceService(IFileSystemService fileSystemService, ILootFilterScriptTranslator lootFilterScriptTranslator)
        {
            _fileSystemService = fileSystemService;
            _lootFilterScriptTranslator = lootFilterScriptTranslator;
        }

        public string LootFilterScriptDirectory { get; set; }

        public LootFilterScript LoadLootFilterScript(string filePath)
        {
            var script = 
                _lootFilterScriptTranslator.TranslateStringToLootFilterScript(
                    _fileSystemService.ReadFileAsString(filePath));

            script.FilePath = filePath;
            return script;
        }

        public void SaveLootFilterScript(LootFilterScript script)
        {
            _fileSystemService.WriteFileFromString(script.FilePath,
                _lootFilterScriptTranslator.TranslateLootFilterScriptToString(script));
        }

        public string DefaultPathOfExileDirectory()
        {
            var defaultDir = _fileSystemService.GetUserProfilePath() + "\\Documents\\My Games\\Path of Exile";
            var defaultDirExists = _fileSystemService.DirectoryExists(defaultDir);

            return defaultDirExists ? defaultDir : string.Empty;
        }
    }
}
