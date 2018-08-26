using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Filtration.ObjectModel;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.ThemeEditor;
using Filtration.ThemeEditor.Services;
using Filtration.ThemeEditor.ViewModels;

namespace Filtration.ThemeEditor.Providers
{
    public interface IThemeProvider
    {
        IThemeEditorViewModel NewThemeForScript(IItemFilterScript script);
        IThemeEditorViewModel MasterThemeForScript(IItemFilterScript script);
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

        public IThemeEditorViewModel NewThemeForScript(IItemFilterScript script)
        {
            var themeComponentCollection = script.ThemeComponents.Aggregate(new ThemeComponentCollection(),
                (c, component) =>
                {
                    switch(component.ComponentType)
                    {
                        case ThemeComponentType.BackgroundColor:
                        case ThemeComponentType.BorderColor:
                        case ThemeComponentType.TextColor:
                            c.Add(new ColorThemeComponent(component.ComponentType, component.ComponentName, ((ColorThemeComponent)component).Color));
                            break;
                        case ThemeComponentType.FontSize:
                            c.Add(new IntegerThemeComponent(component.ComponentType, component.ComponentName, ((IntegerThemeComponent)component).Value));
                            break;
                        case ThemeComponentType.AlertSound:
                            c.Add(new StrIntThemeComponent(component.ComponentType, component.ComponentName, ((StrIntThemeComponent)component).Value, ((StrIntThemeComponent)component).SecondValue));
                            break;
                    }
                    return c;
                });
                
            var themeViewModel = _themeViewModelFactory.Create();
            themeViewModel.InitialiseForNewTheme(themeComponentCollection);
            themeViewModel.FilePath = "Untitled.filtertheme";

            return themeViewModel;
        }

        public IThemeEditorViewModel MasterThemeForScript(IItemFilterScript script)
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
