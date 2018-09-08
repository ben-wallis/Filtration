using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Filtration.Properties;
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

        Task CheckForUpdates();

        Task DownloadUpdatesAsync();

        Task ApplyUpdatesAsync();

        void RestartAfterUpdate();
    }

    internal class UpdateService : IUpdateService
    {
        private readonly ISettingsService _settingsService;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string _localUpdatePath = @"C:\Repos\Filtration\Releases";
        private ReleaseEntry _latestRelease;
        private UpdateInfo _updates;

        private UpdateStatus _updateStatus;

        public UpdateService(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            UpdateStatus = UpdateStatus.NoUpdateAvailable;
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

        public async Task CheckForUpdates()
        {
            if (UpdateStatus != UpdateStatus.NoUpdateAvailable)
            {
                throw new InvalidOperationException();
            }


            Logger.Debug("Checking for update...");
            UpdateStatus = UpdateStatus.CheckingForUpdate;

            try
            {
                bool downloadPrereleaseUpdates;
                downloadPrereleaseUpdates = Settings.Default.DownloadPrereleaseUpdates;
#if DEBUG
                downloadPrereleaseUpdates = true;
#endif
                using (var mgr = await UpdateManager.GitHubUpdateManager("https://github.com/ben-wallis/Filtration", prerelease: downloadPrereleaseUpdates))
                {
                    _updates = await mgr.CheckForUpdate(progress: progress => UpdateProgressChanged?.Invoke(this, new UpdateProgressChangedEventArgs(progress)));
                }

                // Local file update source for testing
                //using (var mgr = new UpdateManager(_localUpdatePath))
                //{

                //    _updates = await mgr.CheckForUpdate(progress: progress => UpdateProgressChanged?.Invoke(this, new UpdateProgressChangedEventArgs(progress)));
                //}
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

                Logger.Debug($"Update found ({LatestReleaseVersion}), fetching release notes...");

                try
                {
                    var releaseNotes = _latestRelease.GetReleaseNotes(_localUpdatePath);
                    LatestReleaseNotes = ProcessReleaseNotes(releaseNotes);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    UpdateStatus = UpdateStatus.Error;
                    return;
                }

                UpdateStatus = UpdateStatus.UpdateAvailable;
            }
            else
            {
                UpdateStatus = UpdateStatus.NoUpdateAvailable;
                Logger.Debug("No update available");
            }
        }

        private string ProcessReleaseNotes(string rawReleaseNotes)
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

            try
            {
                using (var updateManager = new UpdateManager(_localUpdatePath))
                {
                    await updateManager.DownloadReleases(_updates.ReleasesToApply, OnProgressChanged);
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

            try
            {
                using (var updateManager = new UpdateManager(_localUpdatePath))
                {
                    await updateManager.ApplyReleases(_updates, OnProgressChanged);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                UpdateStatus = UpdateStatus.Error;
                return;
            }

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
