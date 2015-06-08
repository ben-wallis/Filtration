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
        private ItemFilterScript _loadedScript;

        private readonly IItemFilterScriptViewModelFactory _itemFilterScriptViewModelFactory;
        private readonly IItemFilterPersistenceService _persistenceService;
        private readonly IItemFilterScriptTranslator _itemFilterScriptTranslator;
        private readonly IReplaceColorsViewModel _replaceColorsViewModel;
        private IItemFilterScriptViewModel _currentScriptViewModel;
        private readonly ObservableCollection<IItemFilterScriptViewModel> _scriptViewModels;

        public MainWindowViewModel(IItemFilterScriptViewModelFactory itemFilterScriptViewModelFactory,
                                   IItemFilterPersistenceService persistenceService,
                                   IItemFilterScriptTranslator itemFilterScriptTranslator,
                                   IReplaceColorsViewModel replaceColorsViewModel)
        {
            _itemFilterScriptViewModelFactory = itemFilterScriptViewModelFactory;
            _persistenceService = persistenceService;
            _itemFilterScriptTranslator = itemFilterScriptTranslator;
            _replaceColorsViewModel = replaceColorsViewModel;

            _scriptViewModels = new ObservableCollection<IItemFilterScriptViewModel>();

            OpenAboutWindowCommand = new RelayCommand(OnOpenAboutWindowCommand);
            OpenScriptCommand = new RelayCommand(OnOpenScriptCommand);
            SaveScriptCommand = new RelayCommand(OnSaveScriptCommand, () => CurrentScriptViewModel != null);
            SaveScriptAsCommand = new RelayCommand(OnSaveScriptAsCommand, () => CurrentScriptViewModel != null);
            CopyScriptCommand = new RelayCommand(OnCopyScriptCommand, () => CurrentScriptViewModel != null);
            CopyBlockCommand = new RelayCommand(OnCopyBlockCommand, () => CurrentScriptViewModel != null && CurrentScriptViewModel.SelectedBlockViewModel != null);
            PasteCommand = new RelayCommand(OnPasteCommand, () => CurrentScriptViewModel != null && CurrentScriptViewModel.SelectedBlockViewModel != null);
            NewScriptCommand = new RelayCommand(OnNewScriptCommand);
            CloseScriptCommand = new RelayCommand<IItemFilterScriptViewModel>(OnCloseScriptCommand, v => CurrentScriptViewModel != null);
            ReplaceColorsCommand = new RelayCommand(OnReplaceColorsCommand, () => CurrentScriptViewModel != null);

            //LoadScriptFromFile("C:\\ThioleLootFilter.txt");

            SetItemFilterScriptDirectory();
        }

        public RelayCommand OpenScriptCommand { get; private set; }
        public RelayCommand SaveScriptCommand { get; private set; }
        public RelayCommand SaveScriptAsCommand { get; private set; }
        public RelayCommand CopyBlockCommand { get; private set; }
        public RelayCommand PasteCommand { get; private set; }
        public RelayCommand CopyScriptCommand { get; private set; }
        public RelayCommand NewScriptCommand { get; private set; }
        public RelayCommand<IItemFilterScriptViewModel> CloseScriptCommand { get; private set; }
        public RelayCommand OpenAboutWindowCommand { get; private set; }
        public RelayCommand ReplaceColorsCommand { get; private set; }

        public ObservableCollection<IItemFilterScriptViewModel> ScriptViewModels
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
        public IItemFilterScriptViewModel CurrentScriptViewModel
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
                InitialDirectory = _persistenceService.ItemFilterScriptDirectory
            };

            if (openFileDialog.ShowDialog() != true) return;

            LoadScriptFromFile(openFileDialog.FileName);
        }

        public void LoadScriptFromFile(string path)
        {
            try
            {
                _loadedScript = _persistenceService.LoadItemFilterScript(path);
            }
            catch (Exception e)
            {
                MessageBox.Show(@"Error loading filter script - " + e.Message, @"Script Load Error", MessageBoxButtons.OK,
                   MessageBoxIcon.Error);
                return;
            }

            var newViewModel = _itemFilterScriptViewModelFactory.Create();
            newViewModel.Initialise(_loadedScript);
            ScriptViewModels.Add(newViewModel);
            CurrentScriptViewModel = newViewModel;
        }

        private void SetItemFilterScriptDirectory()
        {
            var defaultDir = _persistenceService.DefaultPathOfExileDirectory();
            if (!string.IsNullOrEmpty(defaultDir))
            {
                _persistenceService.ItemFilterScriptDirectory = defaultDir;
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
                    _persistenceService.ItemFilterScriptDirectory = dlg.SelectedPath;
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
                _persistenceService.SaveItemFilterScript(CurrentScriptViewModel.Script);
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
                InitialDirectory = _persistenceService.ItemFilterScriptDirectory
            };

            var result = saveDialog.ShowDialog();

            if (result != DialogResult.OK) return;

            var previousFilePath = CurrentScriptViewModel.Script.FilePath;
            try
            {
                CurrentScriptViewModel.Script.FilePath = saveDialog.FileName;
                _persistenceService.SaveItemFilterScript(CurrentScriptViewModel.Script);
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
            Clipboard.SetText(_itemFilterScriptTranslator.TranslateItemFilterScriptToString(_currentScriptViewModel.Script));
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
            var newScript = new ItemFilterScript();
            var newViewModel = _itemFilterScriptViewModelFactory.Create();
            newViewModel.Initialise(newScript);
            newViewModel.Description = "New Script";
            ScriptViewModels.Add(newViewModel);
            CurrentScriptViewModel = newViewModel;
        }

        private void OnCloseScriptCommand(IItemFilterScriptViewModel scriptViewModel)
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
