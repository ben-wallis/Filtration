using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Filtration.Models;
using Filtration.Properties;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    internal interface IUpdateAvailableViewModel
    {
        event EventHandler OnRequestClose;
        void Initialise(UpdateData updateData, decimal currentVersion);
        decimal CurrentVersion { get; }
        decimal NewVersion { get; }
        string ReleaseNotes { get; }
        DateTime ReleaseDate { get; }
    }

    internal class UpdateAvailableViewModel : IUpdateAvailableViewModel
    {
        private UpdateData _updateData;
        private decimal _currentVersion;

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

        public void Initialise(UpdateData updateData, decimal currentVersion)
        {
            _currentVersion = currentVersion;
            _updateData = updateData;
        }

        public decimal CurrentVersion
        {
            get { return _currentVersion; }
        }

        public decimal NewVersion
        {
            get { return _updateData.CurrentVersion; }
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
            Settings.Default.SuppressUpdatesUpToVersion = _updateData.CurrentVersion;
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

