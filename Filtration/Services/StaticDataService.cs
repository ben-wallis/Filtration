using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Filtration.Common.Services;
using Filtration.Common.Utilities;
using Filtration.Utilities;

namespace Filtration.Services
{
    public interface IStaticDataService
    {
        IEnumerable<string> ItemBaseTypes { get; }
        IEnumerable<string> ItemClasses { get; }
    }

    internal class StaticDataService : IStaticDataService
    {
        private readonly IFileSystemService _fileSystemService;

        public StaticDataService(IFileSystemService fileSystemService)
        {
            _fileSystemService = fileSystemService;
            PopulateStaticData();
        }

        public IEnumerable<string> ItemBaseTypes { get; private set; }

        public IEnumerable<string> ItemClasses { get; private set; }

        private void PopulateStaticData()
        {
            var itemBaseTypesPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Filtration\ItemBaseTypes.txt";
            var itemClassesPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Filtration\ItemClasses.txt";

            var itemBaseTypes = _fileSystemService.ReadFileAsString(itemBaseTypesPath);
            ItemBaseTypes = new LineReader(() => new StringReader(itemBaseTypes)).ToList();

            var itemClasses = _fileSystemService.ReadFileAsString(itemClassesPath);
            ItemClasses = new LineReader(() => new StringReader(itemClasses)).ToList();
        }
    }
}
