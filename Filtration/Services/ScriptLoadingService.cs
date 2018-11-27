using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Filtration.Common.Services;
using Filtration.Repositories;
using Filtration.ThemeEditor.Providers;
using Filtration.ThemeEditor.ViewModels;
using Filtration.ViewModels;
using GalaSoft.MvvmLight.Messaging;
using NLog;

namespace Filtration.Services
{
    internal interface IScriptLoadingService
    {
        Task LoadScriptAsync(string scriptFilename);
        Task LoadScriptsAsync(string[] files);
        Task LoadThemeAsync(string themeFilename);
    }

    internal sealed class ScriptLoadingService : IScriptLoadingService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IAvalonDockWorkspaceViewModel _avalonDockWorkspaceViewModel;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IThemeProvider _themeProvider;
        private readonly IItemFilterScriptRepository _itemFilterScriptRepository;

        public ScriptLoadingService(IAvalonDockWorkspaceViewModel avalonDockWorkspaceViewModel,
                                    IItemFilterScriptRepository itemFilterScriptRepository,
                                    IMessageBoxService messageBoxService,
                                    IThemeProvider themeProvider)
        {
            _avalonDockWorkspaceViewModel = avalonDockWorkspaceViewModel;
            _messageBoxService = messageBoxService;
            _themeProvider = themeProvider;
            _itemFilterScriptRepository = itemFilterScriptRepository;
        }

        public async Task LoadScriptsAsync(string[] files)
        {
            foreach (var file in files)
            {
                if (File.Exists(file))
                {
                    await LoadScriptAsync(file);
                }
            }
        }

        public async Task LoadScriptAsync(string scriptFilename)
        {
            IItemFilterScriptViewModel loadedViewModel;

            Messenger.Default.Send(new NotificationMessage("ShowLoadingBanner"));
            try
            {
                loadedViewModel = await _itemFilterScriptRepository.LoadScriptFromFileAsync(scriptFilename);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                _messageBoxService.Show("Script Load Error", "Error loading filter script - " + e.Message,
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                return;
            }
            finally
            {
                Messenger.Default.Send(new NotificationMessage("HideLoadingBanner"));
            }

            _avalonDockWorkspaceViewModel.AddDocument(loadedViewModel);
        }

        public async Task LoadThemeAsync(string themeFilename)
        {
            IThemeEditorViewModel loadedViewModel;

            try
            {
                loadedViewModel = await _themeProvider.LoadThemeFromFile(themeFilename);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                _messageBoxService.Show("Theme Load Error", "Error loading filter theme - " + e.Message,
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                return;
            }

            _avalonDockWorkspaceViewModel.AddDocument(loadedViewModel);
        }
    }
}
