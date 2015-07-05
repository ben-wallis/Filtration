using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        IThemeEditorViewModel LoadThemeFromFile(string filePath);
        Theme LoadThemeModelFromFile(string filePath);
        void SaveTheme(IThemeEditorViewModel themeEditorViewModel, string filePath);
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
            themeViewModel.Initialise(themeComponentCollection, true);
            themeViewModel.FilePath = "Untitled.filtertheme";

            return themeViewModel;
        }

        public IThemeEditorViewModel MasterThemeForScript(ItemFilterScript script)
        {
            var themeViewModel = _themeViewModelFactory.Create();
            themeViewModel.Initialise(script.ThemeComponents, true);
            themeViewModel.FilePath = "<Master Theme> " + Path.GetFileName(script.FilePath);

            return themeViewModel;
        }

        public IThemeEditorViewModel LoadThemeFromFile(string filePath)
        {
            var model = _themePersistenceService.LoadTheme(filePath);
            var viewModel = Mapper.Map<IThemeEditorViewModel>(model);
            viewModel.FilePath = filePath;
            return viewModel;
        }

        public Theme LoadThemeModelFromFile(string filePath)
        {
            return _themePersistenceService.LoadTheme(filePath);
        }

        public void SaveTheme(IThemeEditorViewModel themeEditorViewModel, string filePath)
        {
            var theme = Mapper.Map<Theme>(themeEditorViewModel);
            _themePersistenceService.SaveTheme(theme, filePath);
        }
    }
}
