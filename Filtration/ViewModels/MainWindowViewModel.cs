using System;
using System.Collections.Generic;
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
using Xceed.Wpf.AvalonDock.Layout;
using Clipboard = System.Windows.Clipboard;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Filtration.ViewModels
{
    internal interface IMainWindowViewModel
    {
        IItemFilterScriptViewModel ActiveDocument { get; set; }
        event EventHandler ActiveDocumentChanged;
        void LoadScriptFromFile(string path);
    }

    internal class MainWindowViewModel : FiltrationViewModelBase, IMainWindowViewModel
    {

        private ItemFilterScript _loadedScript;

        private readonly IItemFilterScriptViewModelFactory _itemFilterScriptViewModelFactory;
        private readonly IItemFilterPersistenceService _persistenceService;
        private readonly IItemFilterScriptTranslator _itemFilterScriptTranslator;
        private readonly IReplaceColorsViewModel _replaceColorsViewModel;
        private IItemFilterScriptViewModel _activeDocument;
        private readonly ObservableCollection<IItemFilterScriptViewModel> _scriptViewModels;
        private readonly SectionBrowserViewModel _sectionBrowserViewModel;

        public MainWindowViewModel(IItemFilterScriptViewModelFactory itemFilterScriptViewModelFactory,
                                   IItemFilterPersistenceService persistenceService,
                                   IItemFilterScriptTranslator itemFilterScriptTranslator,
                                   IReplaceColorsViewModel replaceColorsViewModel)
        {
            _itemFilterScriptViewModelFactory = itemFilterScriptViewModelFactory;
            _persistenceService = persistenceService;
            _itemFilterScriptTranslator = itemFilterScriptTranslator;
            _replaceColorsViewModel = replaceColorsViewModel;
            _sectionBrowserViewModel = new SectionBrowserViewModel();
            _sectionBrowserViewModel.Initialise(this);

            _scriptViewModels = new ObservableCollection<IItemFilterScriptViewModel>();

            OpenAboutWindowCommand = new RelayCommand(OnOpenAboutWindowCommand);
            OpenScriptCommand = new RelayCommand(OnOpenScriptCommand);
            SaveScriptCommand = new RelayCommand(OnSaveScriptCommand, () => ActiveDocument != null);
            SaveScriptAsCommand = new RelayCommand(OnSaveScriptAsCommand, () => ActiveDocument != null);
            CopyScriptCommand = new RelayCommand(OnCopyScriptCommand, () => ActiveDocument != null);
            CopyBlockCommand = new RelayCommand(OnCopyBlockCommand, () => ActiveDocument != null && ActiveDocument.SelectedBlockViewModel != null);
            PasteCommand = new RelayCommand(OnPasteCommand, () => ActiveDocument != null && ActiveDocument.SelectedBlockViewModel != null);
            NewScriptCommand = new RelayCommand(OnNewScriptCommand);
            CloseScriptCommand = new RelayCommand<IItemFilterScriptViewModel>(OnCloseScriptCommand, v => ActiveDocument != null);
            ReplaceColorsCommand = new RelayCommand(OnReplaceColorsCommand, () => ActiveDocument != null);

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

        private List<ToolViewModel> _tools;

        public IEnumerable<ToolViewModel> Tools
        {
            get
            {
                if (_tools == null)
                {
                    _tools = new List<ToolViewModel> { _sectionBrowserViewModel };
                }

                return _tools;
            }
        }

        public SectionBrowserViewModel SectionBrowserViewModel
        {
            get
            {
                return _sectionBrowserViewModel;
            }
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
        public IItemFilterScriptViewModel ActiveDocument
        {
            get { return _activeDocument; }
            set
            {
                _activeDocument = value;
                RaisePropertyChanged();
                if (ActiveDocumentChanged != null)
                {
                    ActiveDocumentChanged(this, EventArgs.Empty);
                }

                RaisePropertyChanged("NoScriptsOpen");
                SaveScriptCommand.RaiseCanExecuteChanged();
                SaveScriptAsCommand.RaiseCanExecuteChanged();
            }
        }

        public event EventHandler ActiveDocumentChanged;

        public bool NoScriptsOpen
        {
            get { return _activeDocument == null; }
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
            ActiveDocument = newViewModel;
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

            if (string.IsNullOrEmpty(ActiveDocument.Script.FilePath))
            {
                OnSaveScriptAsCommand();
                return;
            }

            try
            {
                _persistenceService.SaveItemFilterScript(ActiveDocument.Script);
                ActiveDocument.RemoveDirtyFlag();
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

            var previousFilePath = ActiveDocument.Script.FilePath;
            try
            {
                ActiveDocument.Script.FilePath = saveDialog.FileName;
                _persistenceService.SaveItemFilterScript(ActiveDocument.Script);
                ActiveDocument.RemoveDirtyFlag();
            }
            catch (Exception e)
            {
                MessageBox.Show(@"Error saving filter file - " + e.Message, @"Save Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                ActiveDocument.Script.FilePath = previousFilePath;
            }
        }

        private void OnReplaceColorsCommand()
        {
            _replaceColorsViewModel.Initialise(ActiveDocument.Script);
            var replaceColorsWindow = new ReplaceColorsWindow {DataContext = _replaceColorsViewModel};
            replaceColorsWindow.ShowDialog();
        }

        private bool ValidateScript()
        {
            var result = ActiveDocument.Script.Validate();

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
            Clipboard.SetText(_itemFilterScriptTranslator.TranslateItemFilterScriptToString(_activeDocument.Script));
        }

        private void OnCopyBlockCommand()
        {
            _activeDocument.CopyBlock(_activeDocument.SelectedBlockViewModel);
        }

        private void OnPasteCommand()
        {
            _activeDocument.PasteBlock(_activeDocument.SelectedBlockViewModel);
        }

        private void OnNewScriptCommand()
        {
            var newScript = new ItemFilterScript();
            var newViewModel = _itemFilterScriptViewModelFactory.Create();
            newViewModel.Initialise(newScript);
            newViewModel.Description = "New Script";
            ScriptViewModels.Add(newViewModel);
            ActiveDocument = newViewModel;
        }

        private void OnCloseScriptCommand(IItemFilterScriptViewModel scriptViewModel)
        {
            ActiveDocument = scriptViewModel;
            if (!ActiveDocument.IsDirty)
            {
                ScriptViewModels.Remove(ActiveDocument);
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
                        ScriptViewModels.Remove(ActiveDocument);
                        break;
                    }
                    case DialogResult.No:
                    {
                        ScriptViewModels.Remove(ActiveDocument);
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
