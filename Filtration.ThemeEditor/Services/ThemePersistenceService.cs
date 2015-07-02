using System.IO;
using System.Xml.Serialization;
using Filtration.ObjectModel.ThemeEditor;

namespace Filtration.ThemeEditor.Services
{
    internal interface IThemePersistenceService
    {
        Theme LoadTheme(string filePath);
        void SaveTheme(Theme theme, string filePath);
    }

    internal class ThemePersistenceService : IThemePersistenceService
    {
        public Theme LoadTheme(string filePath)
        {
            var xmlSerializer = new XmlSerializer(typeof(Theme));

            Theme loadedTheme;

            using (Stream reader = new FileStream(filePath, FileMode.Open))
            {
                loadedTheme = (Theme)xmlSerializer.Deserialize(reader);
            }
            
            loadedTheme.FilePath = filePath;
            return loadedTheme;
        }

        public void SaveTheme(Theme theme, string filePath)
        {
            var xmlSerializer = new XmlSerializer(typeof(Theme));

            using (Stream writer = new FileStream(filePath, FileMode.Create))
            {
                xmlSerializer.Serialize(writer, theme);
            }
        }
    }
}
