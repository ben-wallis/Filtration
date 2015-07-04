using System;
using System.Diagnostics;
using Filtration.Models;
using Filtration.Properties;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    internal interface IUpdateAvailableViewModel
    {
        event EventHandler OnRequestClose;
        void Initialise(UpdateData updateData, int currentVersionMajorPart, int currentVersionMinorPart);
        string CurrentVersion { get; }
        string NewVersion { get; }
        string ReleaseNotes { get; }
        DateTime ReleaseDate { get; }
    }

    internal class UpdateAvailableViewModel : IUpdateAvailableViewModel
    {
        private UpdateData _updateData;
        private int _currentVersionMajorPart;
        private int _currentVersionMinorPart;

        public UpdateAvailableViewModel()
        {
            DownloadCommand = new RelayCommand(OnDownloadCommand);
            AskLaterCommand = new RelayCommand(OnAskLaterCommand);
            NeverAskAgainCommand = new RelayCommand(OnNeverAskAgainCommand);
        }

        public event EventHandler OnRequestClose;

        public RelayCommand NeverAskAgainCommand { get; private set; }
        public RelayCommand AskLaterCommand { get; private set; }
        public RelayCommand DownloadCommand { get; private set; }

        public void Initialise(UpdateData updateData, int currentVersionMajorPart, int currentVersionMinorPart)
        {
            _currentVersionMajorPart = currentVersionMajorPart;
            _currentVersionMinorPart = currentVersionMinorPart;
            _updateData = updateData;
        }

        public string CurrentVersion
        {
            get { return _currentVersionMajorPart + "." + _currentVersionMinorPart; }
        }

        public string NewVersion
        {
            get { return _updateData.LatestVersionMajorPart + "." + _updateData.LatestVersionMinorPart; }
        }

        public string ReleaseNotes
        {
            get { return _updateData.ReleaseNotes; }
        }

        public DateTime ReleaseDate
        {
            get { return _updateData.ReleaseDate; }
        }

        private void OnDownloadCommand()
        {
            Process.Start(_updateData.DownloadUrl);
        }

        private void OnNeverAskAgainCommand()
        {
            Settings.Default.SuppressUpdates = true;
            Settings.Default.SuppressUpdatesUpToVersionMajorPart = _updateData.LatestVersionMajorPart;
            Settings.Default.SuppressUpdatesUpToVersionMinorPart = _updateData.LatestVersionMinorPart;
            Settings.Default.Save();
            CloseWindow();
        }

        private void OnAskLaterCommand()
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            if (OnRequestClose != null)
            {
                OnRequestClose(this, new EventArgs());
            }
        }
    }

}

