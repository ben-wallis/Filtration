using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Filtration.ObjectModel;
using Filtration.ObjectModel.ThemeEditor;
using Filtration.ThemeEditor.Services;
using Filtration.ThemeEditor.ViewModels;

namespace Filtration.ThemeEditor.Providers
{
    public interface IThemeProvider
    {
        IThemeEditorViewModel NewThemeForScript(ItemFilterScript script);
        IThemeEditorViewModel MasterThemeForScript(ItemFilterScript script);
        Task<IThemeEditorViewModel> LoadThemeFromFile(string filePath);
        Task<Theme> LoadThemeModelFromFile(string filePath);
        Task SaveThemeAsync(IThemeEditorViewModel themeEditorViewModel, string filePath);
    }

    internal class ThemeProvider : IThemeProvider
    {
        private readonly IThemeViewModelFactory _themeViewModelFactory;
        private readonly IThemePersistenceService _themePersistenceService;

        public ThemeProvider(IThemeViewModelFactory themeViewModelFactory, IThemePersistenceService themePersistenceService)
        {
            _themeViewModelFactory = themeViewModelFactory;
            _themePersistenceService = themePersistenceService;
        }

        public IThemeEditorViewModel NewThemeForScript(ItemFilterScript script)
        {
            var themeComponentCollection = script.ThemeComponents.Aggregate(new ThemeComponentCollection(),
                (c, component) =>
                {
                    c.Add(new ThemeComponent(component.ComponentType, component.ComponentName, component.Color));
                    return c;
                });
                
            var themeViewModel = _themeViewModelFactory.Create();
            themeViewModel.InitialiseForNewTheme(themeComponentCollection);
            themeViewModel.FilePath = "Untitled.filtertheme";

            return themeViewModel;
        }

        public IThemeEditorViewModel MasterThemeForScript(ItemFilterScript script)
        {
            var themeViewModel = _themeViewModelFactory.Create();
            themeViewModel.InitialiseForMasterTheme(script);
            themeViewModel.FilePath = "<Master Theme> " + Path.GetFileName(script.FilePath);

            return themeViewModel;
        }

        public async Task<IThemeEditorViewModel> LoadThemeFromFile(string filePath)
        {
            var model = await _themePersistenceService.LoadThemeAsync(filePath);
            var viewModel = Mapper.Map<IThemeEditorViewModel>(model);
            viewModel.FilePath = filePath;
            return viewModel;
        }

        public async Task<Theme> LoadThemeModelFromFile(string filePath)
        {
            return await _themePersistenceService.LoadThemeAsync(filePath);
        }

        public async Task SaveThemeAsync(IThemeEditorViewModel themeEditorViewModel, string filePath)
        {
            await Task.Run(() =>
            {
                var theme = Mapper.Map<Theme>(themeEditorViewModel);
                _themePersistenceService.SaveThemeAsync(theme, filePath);
            });
        }
    }
}
