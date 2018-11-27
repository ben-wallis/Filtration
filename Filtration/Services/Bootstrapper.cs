using System;
using System.Threading.Tasks;
using System.Windows;
using Filtration.Views;
using NLog;

namespace Filtration.Services
{
    internal interface IBootstrapper
    {
        Task GoAsync();
    }

    internal class Bootstrapper : IBootstrapper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IItemFilterScriptDirectoryService _itemFilterScriptDirectoryService;
        private readonly IMainWindow _mainWindow;
        private readonly ISettingsService _settingsService;
        private readonly IUpdateService _updateService;

        public Bootstrapper(IItemFilterScriptDirectoryService itemFilterScriptDirectoryService,
                            IMainWindow mainWindow,
                            ISettingsService settingsService,
                            IUpdateService updateService)
        {
            _itemFilterScriptDirectoryService = itemFilterScriptDirectoryService;
            _mainWindow = mainWindow;
            _settingsService = settingsService;
            _updateService = updateService;
        }

        public async Task GoAsync()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            // Attempt to restore user settings - this is required because after an update any
            // user settings will have been lost due to Squirrel changing the app directory
            // with each update
            _settingsService.RestoreSettings();

            _itemFilterScriptDirectoryService.PromptForFilterScriptDirectoryIfRequired();

            _mainWindow.Show();

            await _updateService.CheckForUpdatesAsync();
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception) e.ExceptionObject;

            Logger.Fatal(exception);
            var exceptionMessage = exception.Message + Environment.NewLine + exception.StackTrace;
            var innerException = exception.InnerException != null
                ? exception.InnerException.Message + Environment.NewLine +
                  exception.InnerException.StackTrace
                : string.Empty;

            MessageBox.Show(exceptionMessage + Environment.NewLine + innerException, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
