using System.Collections.ObjectModel;
using AutoMapper;
using Filtration.ObjectModel;
using Filtration.ObjectModel.ThemeEditor;
using Filtration.ThemeEditor.Services;
using Filtration.ThemeEditor.ViewModels;

namespace Filtration.ThemeEditor.Providers
{
    public interface IThemeProvider
    {
        IThemeViewModel NewThemeForScript(ItemFilterScript script);
        IThemeViewModel LoadThemeFromFile(string filePath);
        Theme LoadThemeModelFromFile(string filePath);
        void SaveTheme(IThemeViewModel themeViewModel, string filePath);
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

        public IThemeViewModel NewThemeForScript(ItemFilterScript script)
        {
            var themeComponentViewModels = Mapper.Map<ObservableCollection<ThemeComponentViewModel>>(script.ThemeComponents);
            var themeViewModel = _themeViewModelFactory.Create();
            themeViewModel.Initialise(themeComponentViewModels, true);
            themeViewModel.FilePath = "Untitled.filtertheme";

            return themeViewModel;
        }

        public IThemeViewModel LoadThemeFromFile(string filePath)
        {
            var model = _themePersistenceService.LoadTheme(filePath);
            var viewModel = Mapper.Map<IThemeViewModel>(model);
            viewModel.FilePath = filePath;
            return viewModel;
        }

        public Theme LoadThemeModelFromFile(string filePath)
        {
            return _themePersistenceService.LoadTheme(filePath);
        }

        public void SaveTheme(IThemeViewModel themeViewModel, string filePath)
        {
            var theme = Mapper.Map<Theme>(themeViewModel);
            _themePersistenceService.SaveTheme(theme, filePath);
        }
    }
}
