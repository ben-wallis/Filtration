using System;
using System.Threading.Tasks;
using Filtration.Common.ViewModels;
using Filtration.Interface;
using Filtration.Services;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    public interface IUpdateViewModel : IDocument
    {
        RelayCommand HideUpdateWindowCommand { get; }

        RelayCommand NextStepCommand { get; }

        UpdateStatus UpdateStatus { get; }

        string ReleaseNotes { get; }

        string Version { get; }

        string NextStepButtonText { get; }

        bool Visible { get; }
        bool IsInErrorState { get; }
    }

    /// <summary>
    /// This ViewModel is shared between UpdateView.xaml and UpdateTabView.xaml since they are both
    /// parts of the same update process, first the user gets given the opportunity to download the
    /// update from the UpdateView box and then once it is ready to install they click Update
    /// and are taken to the UpdateTabView tab where they can see the changes and begin the installation
    /// of the update.
    /// </summary>
    internal class UpdateViewModel : PaneViewModel, IUpdateViewModel
    {
        private readonly IAvalonDockWorkspaceViewModel _avalonDockWorkspaceViewModel;

        private readonly IUpdateService _updateService;
        private string _nextStepButtonText;
        private bool _visible;
        private bool _updateTabShown;

        public UpdateViewModel(IAvalonDockWorkspaceViewModel avalonDockWorkspaceViewModel,
                               IUpdateService updateService)
        {
            _avalonDockWorkspaceViewModel = avalonDockWorkspaceViewModel;
            _updateService = updateService;

            updateService.UpdateProgressChanged += UpdateServiceOnUpdateProgressChanged;
            updateService.UpdateStatusChanged += UpdateServiceOnUpdateStatusChanged;


            HideUpdateWindowCommand = new RelayCommand(OnHideUpdateWindowCommand, () => UpdateStatus == UpdateStatus.UpdateAvailable || UpdateStatus == UpdateStatus.Error);
            NextStepCommand = new RelayCommand(async () => await OnNextStepCommandAsync(), () => NextStepCommandEnabled);

            Title = "Update Available";
        }

        public RelayCommand NextStepCommand { get; }

        public RelayCommand HideUpdateWindowCommand { get; }

        public UpdateStatus UpdateStatus => _updateService.UpdateStatus;

        public bool IsInErrorState => UpdateStatus == UpdateStatus.Error;

        public async Task<bool> Close()
        {
            await Task.FromResult(true);
            return true;
        }

        public RelayCommand CloseCommand { get; }

        public bool IsScript => false;
        public bool IsTheme => false;

        public bool Visible
        {
            get => _visible;
            private set
            {
                _visible = value;
                RaisePropertyChanged();
            }
        }

        public string ReleaseNotes => _updateService.LatestReleaseNotes;

        public string Version => _updateService.LatestReleaseVersion;

        public string NextStepButtonText
        {
            get => _nextStepButtonText;
            set
            {
                _nextStepButtonText = value;
                RaisePropertyChanged();
            }
        }

        private bool NextStepCommandEnabled => UpdateStatus == UpdateStatus.UpdateAvailable || UpdateStatus == UpdateStatus.ReadyToApplyUpdate || UpdateStatus == UpdateStatus.UpdateComplete;

        private async Task OnNextStepCommandAsync()
        {
            switch (UpdateStatus)
            {
                case UpdateStatus.UpdateAvailable:
                    {
                        await _updateService.DownloadUpdatesAsync();
                        break;
                    }
                case UpdateStatus.ReadyToApplyUpdate:
                    {
                        if (!_updateTabShown)
                        {
                            // When the update has downloaded and is ready to apply, clicking the button
                            // closes the update popup and shows the update tab.
                            _avalonDockWorkspaceViewModel.AddDocument(this);
                            Visible = false;
                            _updateTabShown = true;
                            NextStepButtonText = "Update";
                        }
                        else
                        {
                            await _updateService.ApplyUpdatesAsync();
                        }

                        break;
                    }
                case UpdateStatus.UpdateComplete:
                    {
                        _updateService.RestartAfterUpdate();
                        break;
                    }
            }
        }

        private void UpdateServiceOnUpdateStatusChanged(object sender, UpdateStatusChangedEventArgs e)
        {
            switch (UpdateStatus)
            {
                case UpdateStatus.UpdateAvailable:
                {
                    Visible = true;
                    NextStepButtonText = "Download";
                    RaisePropertyChanged(nameof(Version));
                    break;
                }
                case UpdateStatus.Downloading:
                {
                    HideUpdateWindowCommand.RaiseCanExecuteChanged();
                    NextStepButtonText = "Downloading (0%)";
                    break;
                }
                case UpdateStatus.ReadyToApplyUpdate:
                {
                    RaisePropertyChanged(nameof(ReleaseNotes));
                    NextStepButtonText = "Update Ready";
                    break;
                }
                case UpdateStatus.Updating:
                {
                    NextStepButtonText = "Updating (0%)";
                    break;
                }
                case UpdateStatus.UpdateComplete:
                {
                    NextStepButtonText = "Restart";
                    break;
                }
                case UpdateStatus.Error:
                {
                    NextStepButtonText = "Update Error - Check Logs";
                    Visible = true;
                    break;
                }
            }

            NextStepCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged(nameof(UpdateStatus));
            RaisePropertyChanged(nameof(IsInErrorState));

        }

        private void UpdateServiceOnUpdateProgressChanged(object sender, UpdateProgressChangedEventArgs e)
        {
            if (UpdateStatus == UpdateStatus.Downloading)
            {
                NextStepButtonText = $"Downloading ({e.Progress}%)";
            }
            else if (UpdateStatus == UpdateStatus.Updating)
            {
                NextStepButtonText = $"Updating ({e.Progress}%)";
            }
        }

        private void OnHideUpdateWindowCommand()
        {
            Visible = false;
        }
    }
}
