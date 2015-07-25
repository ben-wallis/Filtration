using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Filtration.ObjectModel.ThemeEditor;

namespace Filtration.ThemeEditor.Services
{
    internal interface IThemePersistenceService
    {
        Task<Theme> LoadThemeAsync(string filePath);
        Task SaveThemeAsync(Theme theme, string filePath);
    }

    internal class ThemePersistenceService : IThemePersistenceService
    {
        public async Task<Theme> LoadThemeAsync(string filePath)
        {
            Theme loadedTheme = null;

            await Task.Run(() =>
            {
                var xmlSerializer = new XmlSerializer(typeof (Theme));

                using (Stream reader = new FileStream(filePath, FileMode.Open))
                {
                    loadedTheme = (Theme) xmlSerializer.Deserialize(reader);
                }

                loadedTheme.FilePath = filePath;
            });

            return loadedTheme;
        }

        public async Task SaveThemeAsync(Theme theme, string filePath)
        {
            await Task.Run(() =>
            {
                var xmlSerializer = new XmlSerializer(typeof (Theme));

                using (Stream writer = new FileStream(filePath, FileMode.Create))
                {
                    xmlSerializer.Serialize(writer, theme);
                }
            });
        }
    }
}
