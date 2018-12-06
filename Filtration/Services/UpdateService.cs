using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Filtration.Enums;
using Filtration.Properties;
using Filtration.Utility;
using NLog;
using Squirrel;

namespace Filtration.Services
{
    public enum UpdateStatus
    {
        NoUpdateAvailable,
        CheckingForUpdate,
        UpdateAvailable,
        Downloading,
        ReadyToApplyUpdate,
        Updating,
        UpdateComplete,
        Error
    }

    internal class UpdateStatusChangedEventArgs : EventArgs
    {
        public UpdateStatusChangedEventArgs(UpdateStatus updateStatus)
        {
            UpdateStatus = updateStatus;
        }

        public UpdateStatus UpdateStatus { get; }
    }

    internal class UpdateProgressChangedEventArgs : EventArgs
    {
        public UpdateProgressChangedEventArgs(int progress)
        {
            Progress = progress;
        }

        public int Progress { get; }
    }

    internal interface IUpdateService
    {
        event EventHandler<UpdateStatusChangedEventArgs> UpdateStatusChanged;

        event EventHandler<UpdateProgressChangedEventArgs> UpdateProgressChanged;

        UpdateStatus UpdateStatus { get; }

        string LatestReleaseNotes { get; }

        string LatestReleaseVersion { get; }

        Task CheckForUpdatesAsync();

        Task DownloadUpdatesAsync();

        Task ApplyUpdatesAsync();

        void RestartAfterUpdate();
    }

    internal class UpdateService : IUpdateService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string _localUpdatePath = @"C:\Repos\Filtration\Releases";

        private readonly ISettingsService _settingsService;
        private readonly UpdateSource _updateSource = UpdateSource.GitHub;

        private ReleaseEntry _latestRelease;
        private UpdateInfo _updates;
        private bool _downloadPrereleaseUpdates;
        private UpdateStatus _updateStatus;

        public UpdateService(ISettingsService settingsService,
                             ISplatNLogAdapter splatNLogAdapter)
        {
            _settingsService = settingsService;

            UpdateStatus = UpdateStatus.NoUpdateAvailable;

            Splat.Locator.CurrentMutable.Register(() => splatNLogAdapter, typeof(Splat.ILogger));
        }

        public event EventHandler<UpdateStatusChangedEventArgs> UpdateStatusChanged;
        public event EventHandler<UpdateProgressChangedEventArgs> UpdateProgressChanged;

        public UpdateStatus UpdateStatus
        {
            get => _updateStatus;
            private set
            {
                _updateStatus = value;
                UpdateStatusChanged?.Invoke(this, new UpdateStatusChangedEventArgs(value));
            }
        }

        public string LatestReleaseNotes { get; private set; }

        public string LatestReleaseVersion { get; private set; }

        public async Task CheckForUpdatesAsync()
        {
            if (UpdateStatus != UpdateStatus.NoUpdateAvailable)
            {
                throw new InvalidOperationException();
            }

            Logger.Debug("Checking for update...");
            UpdateStatus = UpdateStatus.CheckingForUpdate;

            try
            {
                _downloadPrereleaseUpdates = Settings.Default.DownloadPrereleaseUpdates;
#if DEBUG
                _downloadPrereleaseUpdates = true;
#endif

                var expectedInstallationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Filtration");
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                var runningInstalled = baseDirectory.StartsWith(expectedInstallationPath);

                if (!runningInstalled)
                {
                    Logger.Debug($"Skipping update check since base directory of {baseDirectory} does not start with the expected installation path of {expectedInstallationPath}");
                    return;
                }

                async Task CheckForUpdatesAsync(IUpdateManager updateManager)
                {
                    _updates = await updateManager.CheckForUpdate(progress: progress => UpdateProgressChanged?.Invoke(this, new UpdateProgressChangedEventArgs(progress)));
                }

                if (_updateSource == UpdateSource.GitHub)
                {
                    using (var updateManager = await UpdateManager.GitHubUpdateManager("https://github.com/ben-wallis/Filtration", prerelease: _downloadPrereleaseUpdates))
                    {
                        await CheckForUpdatesAsync(updateManager);
                    }
                }
                else
                {
                    using (var updateManager = new UpdateManager(_localUpdatePath))
                    {
                        await CheckForUpdatesAsync(updateManager);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                UpdateStatus = UpdateStatus.Error;
                return;
            }

            if (_updates.ReleasesToApply.Any())
            {
                _latestRelease = _updates.ReleasesToApply.OrderBy(x => x.Version).Last();
                LatestReleaseVersion = _latestRelease.Version.ToString();

                Logger.Debug($"Update found ({LatestReleaseVersion})");
                
                UpdateStatus = UpdateStatus.UpdateAvailable;
            }
            else
            {
                UpdateStatus = UpdateStatus.NoUpdateAvailable;
                Logger.Debug("No update available");
            }
        }

        private static string ProcessReleaseNotes(string rawReleaseNotes)
        {
            var regex = new Regex(@"<!\[CDATA\[(.*)]]>", RegexOptions.Singleline);
            var matches = regex.Match(rawReleaseNotes);
            if (matches.Success)
            {
                var releaseNotes = matches.Groups[1].Value;
                return "<font face=\"Segoe UI\" size=\"2\">" + releaseNotes + "</font>";
            }

            return string.Empty;
        }

        public async Task DownloadUpdatesAsync()
        {
            if (_updates == null || UpdateStatus != UpdateStatus.UpdateAvailable)
            {
                throw new InvalidOperationException();
            }

            UpdateStatus = UpdateStatus.Downloading;

            async Task DownloadUpdatesAsync(IUpdateManager updateManager)
            {
                Logger.Debug("Downloading update...");
                await updateManager.DownloadReleases(_updates.ReleasesToApply, OnProgressChanged);

                Logger.Debug("Fetching release notes...");
                var releaseNotes = _updates.FetchReleaseNotes();
                LatestReleaseNotes = ProcessReleaseNotes(releaseNotes[_latestRelease]);
            }

            try
            {
                if (_updateSource == UpdateSource.GitHub)
                {
                    using (var updateManager = await UpdateManager.GitHubUpdateManager("https://github.com/ben-wallis/Filtration", prerelease: _downloadPrereleaseUpdates))
                    {
                        await DownloadUpdatesAsync(updateManager);
                    }
                }
                else
                {
                    using (var updateManager = new UpdateManager(_localUpdatePath))
                    {
                        await DownloadUpdatesAsync(updateManager);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                UpdateStatus = UpdateStatus.Error;
                return;
            }

            UpdateStatus = UpdateStatus.ReadyToApplyUpdate;
        }


        public async Task ApplyUpdatesAsync()
        {
            if (UpdateStatus != UpdateStatus.ReadyToApplyUpdate)
            {
                throw new InvalidOperationException();
            }

            UpdateStatus = UpdateStatus.Updating;

            // Back up the current user settings before updating as updating using Squirrel
            // wipes out user settings due to the application directory changing with each update
            _settingsService.BackupSettings();

            Logger.Debug("Applying update...");
            try
            {
                if (_updateSource == UpdateSource.GitHub)
                {
                    using (var mgr = await UpdateManager.GitHubUpdateManager("https://github.com/ben-wallis/Filtration", prerelease: _downloadPrereleaseUpdates))
                    {
                        await mgr.ApplyReleases(_updates, OnProgressChanged);
                    }
                }
                else
                {
                    using (var updateManager = new UpdateManager(_localUpdatePath))
                    {
                        await updateManager.ApplyReleases(_updates, OnProgressChanged);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                UpdateStatus = UpdateStatus.Error;
                return;
            }

            Logger.Debug("Update complete");
            UpdateStatus = UpdateStatus.UpdateComplete;
        }

        public void RestartAfterUpdate()
        {
            if (UpdateStatus != UpdateStatus.UpdateComplete)
            {
                throw new InvalidOperationException();
            }

            try
            {
                UpdateManager.RestartApp();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                UpdateStatus = UpdateStatus.Error;
            }
        }

        private void OnProgressChanged(int progress)
        {
            UpdateProgressChanged?.Invoke(this, new UpdateProgressChangedEventArgs(progress));
        }
    }
}
