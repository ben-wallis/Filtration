using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Castle.Core;
using Filtration.Models;
using Filtration.Services;
using Filtration.Translators;
using Filtration.Views;
using GalaSoft.MvvmLight.CommandWpf;
using Clipboard = System.Windows.Clipboard;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Filtration.ViewModels
{
    internal interface IMainWindowViewModel
    {
        void LoadScriptFromFile(string path);
    }

    internal class MainWindowViewModel : FiltrationViewModelBase, IMainWindowViewModel
    {
        private LootFilterScript _loadedScript;

        private readonly ILootFilterScriptViewModelFactory _lootFilterScriptViewModelFactory;
        private readonly ILootFilterPersistenceService _persistenceService;
        private readonly ILootFilterScriptTranslator _lootFilterScriptTranslator;
        private readonly IReplaceColorsViewModel _replaceColorsViewModel;
        private ILootFilterScriptViewModel _currentScriptViewModel;
        private readonly ObservableCollection<ILootFilterScriptViewModel> _scriptViewModels;

        public MainWindowViewModel(ILootFilterScriptViewModelFactory lootFilterScriptViewModelFactory,
                                   ILootFilterPersistenceService persistenceService,
                                   ILootFilterScriptTranslator lootFilterScriptTranslator,
                                   IReplaceColorsViewModel replaceColorsViewModel)
        {
            _lootFilterScriptViewModelFactory = lootFilterScriptViewModelFactory;
            _persistenceService = persistenceService;
            _lootFilterScriptTranslator = lootFilterScriptTranslator;
            _replaceColorsViewModel = replaceColorsViewModel;

            _scriptViewModels = new ObservableCollection<ILootFilterScriptViewModel>();

            OpenAboutWindowCommand = new RelayCommand(OnOpenAboutWindowCommand);
            OpenScriptCommand = new RelayCommand(OnOpenScriptCommand);
            SaveScriptCommand = new RelayCommand(OnSaveScriptCommand, () => CurrentScriptViewModel != null);
            SaveScriptAsCommand = new RelayCommand(OnSaveScriptAsCommand, () => CurrentScriptViewModel != null);
            CopyScriptCommand = new RelayCommand(OnCopyScriptCommand, () => CurrentScriptViewModel != null);
            CopyBlockCommand = new RelayCommand(OnCopyBlockCommand, () => CurrentScriptViewModel != null && CurrentScriptViewModel.SelectedBlockViewModel != null);
            PasteCommand = new RelayCommand(OnPasteCommand, () => CurrentScriptViewModel != null && CurrentScriptViewModel.SelectedBlockViewModel != null);
            NewScriptCommand = new RelayCommand(OnNewScriptCommand);
            CloseScriptCommand = new RelayCommand<ILootFilterScriptViewModel>(OnCloseScriptCommand, v => CurrentScriptViewModel != null);
            ReplaceColorsCommand = new RelayCommand(OnReplaceColorsCommand, () => CurrentScriptViewModel != null);

            //LoadScriptFromFile("C:\\ThioleLootFilter.txt");

            SetLootFilterScriptDirectory();
        }

        public RelayCommand OpenScriptCommand { get; private set; }
        public RelayCommand SaveScriptCommand { get; private set; }
        public RelayCommand SaveScriptAsCommand { get; private set; }
        public RelayCommand CopyBlockCommand { get; private set; }
        public RelayCommand PasteCommand { get; private set; }
        public RelayCommand CopyScriptCommand { get; private set; }
        public RelayCommand NewScriptCommand { get; private set; }
        public RelayCommand<ILootFilterScriptViewModel> CloseScriptCommand { get; private set; }
        public RelayCommand OpenAboutWindowCommand { get; private set; }
        public RelayCommand ReplaceColorsCommand { get; private set; }

        public ObservableCollection<ILootFilterScriptViewModel> ScriptViewModels
        {
            get { return _scriptViewModels; }
        }

        public string WindowTitle
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return "Filtration v" + fvi.FileMajorPart + "." +  fvi.FileMinorPart;
            }
        }

        [DoNotWire]
        public ILootFilterScriptViewModel CurrentScriptViewModel
        {
            get { return _currentScriptViewModel; }
            set
            {
                _currentScriptViewModel = value;
                RaisePropertyChanged();
                RaisePropertyChanged("NoScriptsOpen");
                SaveScriptCommand.RaiseCanExecuteChanged();
                SaveScriptAsCommand.RaiseCanExecuteChanged();
            }
        }

        public bool NoScriptsOpen
        {
            get { return _currentScriptViewModel == null; }
        }

        private void OnOpenAboutWindowCommand()
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void OnOpenScriptCommand()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Filter Files (*.filter)|*.filter|All Files (*.*)|*.*",
                InitialDirectory = _persistenceService.LootFilterScriptDirectory
            };

            if (openFileDialog.ShowDialog() != true) return;

            LoadScriptFromFile(openFileDialog.FileName);
        }

        public void LoadScriptFromFile(string path)
        {
            try
            {
                _loadedScript = _persistenceService.LoadLootFilterScript(path);
            }
            catch (Exception e)
            {
                MessageBox.Show(@"Error loading filter script - " + e.Message, @"Script Load Error", MessageBoxButtons.OK,
                   MessageBoxIcon.Error);
                return;
            }

            var newViewModel = _lootFilterScriptViewModelFactory.Create();
            newViewModel.Initialise(_loadedScript);
            ScriptViewModels.Add(newViewModel);
            CurrentScriptViewModel = newViewModel;
        }

        private void SetLootFilterScriptDirectory()
        {
            var defaultDir = _persistenceService.DefaultPathOfExileDirectory();
            if (!string.IsNullOrEmpty(defaultDir))
            {
                _persistenceService.LootFilterScriptDirectory = defaultDir;
            }
            else
            {
                var dlg = new FolderBrowserDialog
                {
                    Description = @"Select your Path of Exile data directory, usually in Documents\My Games",
                    ShowNewFolderButton = false
                };
                var result = dlg.ShowDialog();

                if (result == DialogResult.OK)
                {
                    _persistenceService.LootFilterScriptDirectory = dlg.SelectedPath;
                }
            }
        }

        private void OnSaveScriptCommand()
        {
            if (!ValidateScript()) return;

            if (string.IsNullOrEmpty(CurrentScriptViewModel.Script.FilePath))
            {
                OnSaveScriptAsCommand();
                return;
            }

            try
            {
                _persistenceService.SaveLootFilterScript(CurrentScriptViewModel.Script);
                CurrentScriptViewModel.RemoveDirtyFlag();
            }
            catch (Exception e)
            {
                MessageBox.Show(@"Error saving filter file - " + e.Message, @"Save Error", MessageBoxButtons.OK,
                   MessageBoxIcon.Error);
            }

        }

        private void OnSaveScriptAsCommand()
        {
            if (!ValidateScript()) return;

            var saveDialog = new SaveFileDialog
            {
                DefaultExt = ".filter",
                Filter = @"Filter Files (*.filter)|*.filter|All Files (*.*)|*.*",
                InitialDirectory = _persistenceService.LootFilterScriptDirectory
            };

            var result = saveDialog.ShowDialog();

            if (result != DialogResult.OK) return;

            var previousFilePath = CurrentScriptViewModel.Script.FilePath;
            try
            {
                CurrentScriptViewModel.Script.FilePath = saveDialog.FileName;
                _persistenceService.SaveLootFilterScript(CurrentScriptViewModel.Script);
                CurrentScriptViewModel.RemoveDirtyFlag();
            }
            catch (Exception e)
            {
                MessageBox.Show(@"Error saving filter file - " + e.Message, @"Save Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                CurrentScriptViewModel.Script.FilePath = previousFilePath;
            }
        }

        private void OnReplaceColorsCommand()
        {
            _replaceColorsViewModel.Initialise(CurrentScriptViewModel.Script);
            var replaceColorsWindow = new ReplaceColorsWindow {DataContext = _replaceColorsViewModel};
            replaceColorsWindow.ShowDialog();
        }

        private bool ValidateScript()
        {
            var result = CurrentScriptViewModel.Script.Validate();

            if (result.Count == 0) return true;

            var failures = string.Empty;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string failure in result)
            {
                failures += failure + Environment.NewLine;
            }

            MessageBox.Show(@"The following script validation errors occurred:" + Environment.NewLine + failures,
                @"Script Validation Failure", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return false;
        }

        private void OnCopyScriptCommand()
        {
            Clipboard.SetText(_lootFilterScriptTranslator.TranslateLootFilterScriptToString(_currentScriptViewModel.Script));
        }

        private void OnCopyBlockCommand()
        {
            _currentScriptViewModel.CopyBlock(_currentScriptViewModel.SelectedBlockViewModel);
        }

        private void OnPasteCommand()
        {
            _currentScriptViewModel.PasteBlock(_currentScriptViewModel.SelectedBlockViewModel);
        }

        private void OnNewScriptCommand()
        {
            var newScript = new LootFilterScript();
            var newViewModel = _lootFilterScriptViewModelFactory.Create();
            newViewModel.Initialise(newScript);
            newViewModel.Description = "New Script";
            ScriptViewModels.Add(newViewModel);
            CurrentScriptViewModel = newViewModel;
        }

        private void OnCloseScriptCommand(ILootFilterScriptViewModel scriptViewModel)
        {
            CurrentScriptViewModel = scriptViewModel;
            if (!CurrentScriptViewModel.IsDirty)
            {
                ScriptViewModels.Remove(CurrentScriptViewModel);
            }
            else
            {
                var result = MessageBox.Show(@"Want to save your changes to this script?",
                    @"Filtration", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                switch (result)
                {
                    case DialogResult.Yes:
                    {
                        OnSaveScriptCommand();
                        ScriptViewModels.Remove(CurrentScriptViewModel);
                        break;
                    }
                    case DialogResult.No:
                    {
                        ScriptViewModels.Remove(CurrentScriptViewModel);
                        break;
                    }
                    case DialogResult.Cancel:
                    {
                        break;
                    }
                }
            }
        }
    }
}
