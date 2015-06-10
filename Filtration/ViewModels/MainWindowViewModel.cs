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
using Clipboard = System.Windows.Clipboard;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Filtration.ViewModels
{
    internal interface IMainWindowViewModel
    {
        IDocument ActiveDocument { get; set; }
        IItemFilterScriptViewModel ActiveScriptViewModel { get; }
        event EventHandler ActiveDocumentChanged;
        void LoadScriptFromFile(string path);
        RelayCommand OpenScriptCommand { get; }
        RelayCommand NewScriptCommand { get; }
        void Close(IDocument scriptToClose);
    }

    internal class MainWindowViewModel : FiltrationViewModelBase, IMainWindowViewModel
    {

        private ItemFilterScript _loadedScript;

        private readonly IItemFilterScriptViewModelFactory _itemFilterScriptViewModelFactory;
        private readonly IItemFilterPersistenceService _persistenceService;
        private readonly IItemFilterScriptTranslator _itemFilterScriptTranslator;
        private readonly IReplaceColorsViewModel _replaceColorsViewModel;
        private IDocument _activeDocument;
        private IItemFilterScriptViewModel _activeScriptViewModel;
        private readonly ObservableCollection<IDocument> _openDocuments;
        private readonly ISectionBrowserViewModel _sectionBrowserViewModel;
        private readonly IBlockGroupBrowserViewModel _blockGroupBrowserViewModel;
        private readonly IStartPageViewModel _startPageViewModel;

        public MainWindowViewModel(IItemFilterScriptViewModelFactory itemFilterScriptViewModelFactory,
                                   IItemFilterPersistenceService persistenceService,
                                   IItemFilterScriptTranslator itemFilterScriptTranslator,
                                   IReplaceColorsViewModel replaceColorsViewModel,
                                   ISectionBrowserViewModel sectionBrowserViewModel,
                                   IBlockGroupBrowserViewModel blockGroupBrowserViewModel,
                                   IStartPageViewModel startPageViewModel)
        {
            _itemFilterScriptViewModelFactory = itemFilterScriptViewModelFactory;
            _persistenceService = persistenceService;
            _itemFilterScriptTranslator = itemFilterScriptTranslator;
            _replaceColorsViewModel = replaceColorsViewModel;
            _sectionBrowserViewModel = sectionBrowserViewModel;
            _blockGroupBrowserViewModel = blockGroupBrowserViewModel;
            _startPageViewModel = startPageViewModel;

            _sectionBrowserViewModel.Initialise(this);
            _blockGroupBrowserViewModel.Initialise(this);
            _startPageViewModel.Initialise(this);

            _openDocuments = new ObservableCollection<IDocument>();

            OpenAboutWindowCommand = new RelayCommand(OnOpenAboutWindowCommand);
            OpenScriptCommand = new RelayCommand(OnOpenScriptCommand);
            SaveScriptCommand = new RelayCommand(OnSaveScriptCommand, () => ActiveDocument != null && ActiveDocument.IsScript);
            SaveScriptAsCommand = new RelayCommand(OnSaveScriptAsCommand, () => ActiveDocument != null && ActiveDocument.IsScript);
            CopyScriptCommand = new RelayCommand(OnCopyScriptCommand, () => ActiveDocument != null && ActiveDocument.IsScript);
            CopyBlockCommand = new RelayCommand(OnCopyBlockCommand, () => ActiveDocument != null && ActiveDocument.IsScript && ((IItemFilterScriptViewModel)ActiveDocument).SelectedBlockViewModel != null);
            PasteCommand = new RelayCommand(OnPasteCommand, () => ActiveDocument != null && ActiveDocument.IsScript && ((IItemFilterScriptViewModel)ActiveDocument).SelectedBlockViewModel != null);
            NewScriptCommand = new RelayCommand(OnNewScriptCommand);
            CloseScriptCommand = new RelayCommand<IDocument>(OnCloseScriptCommand, v => ActiveDocument != null && ActiveDocument.IsScript);
            ReplaceColorsCommand = new RelayCommand(OnReplaceColorsCommand, () => ActiveDocument != null && ActiveDocument.IsScript);

            //LoadScriptFromFile("C:\\ThioleLootFilter.txt");

            SetItemFilterScriptDirectory();
            
            _openDocuments.Add(_startPageViewModel);
            ActiveDocument = startPageViewModel;
        }

        public RelayCommand OpenScriptCommand { get; private set; }
        public RelayCommand SaveScriptCommand { get; private set; }
        public RelayCommand SaveScriptAsCommand { get; private set; }
        public RelayCommand CopyBlockCommand { get; private set; }
        public RelayCommand PasteCommand { get; private set; }
        public RelayCommand CopyScriptCommand { get; private set; }
        public RelayCommand NewScriptCommand { get; private set; }
        public RelayCommand<IDocument> CloseScriptCommand { get; private set; }
        public RelayCommand OpenAboutWindowCommand { get; private set; }
        public RelayCommand ReplaceColorsCommand { get; private set; }

        public ObservableCollection<IDocument> OpenDocuments
        {
            get { return _openDocuments; }
        }

        private List<IToolViewModel> _tools;

        public IEnumerable<IToolViewModel> Tools
        {
            get
            {
                if (_tools == null)
                {
                    _tools = new List<IToolViewModel> {_sectionBrowserViewModel, _blockGroupBrowserViewModel};
                }

                return _tools;
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

        public IDocument ActiveDocument
        {
            get { return _activeDocument; }
            set
            {
                _activeDocument = value;
                RaisePropertyChanged();
                
                if (value.IsScript)
                {
                    _activeScriptViewModel = (IItemFilterScriptViewModel)value;
                }

                if (ActiveDocumentChanged != null)
                {
                    ActiveDocumentChanged(this, EventArgs.Empty);
                }

                RaisePropertyChanged("NoScriptsOpen");
                SaveScriptCommand.RaiseCanExecuteChanged();
                SaveScriptAsCommand.RaiseCanExecuteChanged();
            }
        }

        public IItemFilterScriptViewModel ActiveScriptViewModel
        {
            get { return _activeScriptViewModel; }
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
            _activeScriptViewModel = newViewModel;
            OpenDocuments.Add((IDocument)newViewModel);
            ActiveDocument = (IDocument)newViewModel;
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

            if (string.IsNullOrEmpty(_activeScriptViewModel.Script.FilePath))
            {
                OnSaveScriptAsCommand();
                return;
            }

            try
            {
                _persistenceService.SaveItemFilterScript(_activeScriptViewModel.Script);
                _activeScriptViewModel.RemoveDirtyFlag();
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

            var previousFilePath = _activeScriptViewModel.Script.FilePath;
            try
            {
                _activeScriptViewModel.Script.FilePath = saveDialog.FileName;
                _persistenceService.SaveItemFilterScript(_activeScriptViewModel.Script);
                _activeScriptViewModel.RemoveDirtyFlag();
            }
            catch (Exception e)
            {
                MessageBox.Show(@"Error saving filter file - " + e.Message, @"Save Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                _activeScriptViewModel.Script.FilePath = previousFilePath;
            }
        }

        private void OnReplaceColorsCommand()
        {
            _replaceColorsViewModel.Initialise(_activeScriptViewModel.Script);
            var replaceColorsWindow = new ReplaceColorsWindow {DataContext = _replaceColorsViewModel};
            replaceColorsWindow.ShowDialog();
        }

        private bool ValidateScript()
        {
            var result = _activeScriptViewModel.Script.Validate();

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
            Clipboard.SetText(_itemFilterScriptTranslator.TranslateItemFilterScriptToString(_activeScriptViewModel.Script));
        }

        private void OnCopyBlockCommand()
        {
            _activeScriptViewModel.CopyBlock(_activeScriptViewModel.SelectedBlockViewModel);
        }

        private void OnPasteCommand()
        {
            _activeScriptViewModel.PasteBlock(_activeScriptViewModel.SelectedBlockViewModel);
        }

        private void OnNewScriptCommand()
        {
            var newScript = new ItemFilterScript();
            var newViewModel = _itemFilterScriptViewModelFactory.Create();
            newViewModel.Initialise(newScript);
            newViewModel.Description = "New Script";
            _activeScriptViewModel = newViewModel;
            OpenDocuments.Add((IDocument)newViewModel);
            ActiveDocument = (IDocument)newViewModel;
        }

        private void OnCloseScriptCommand(IDocument documentToClose)
        {
            Close(documentToClose);
        }

        public void Close(IDocument documentToClose)
        {
            ActiveDocument = documentToClose;
            if (ActiveDocument.IsScript)
            {
                if (!_activeScriptViewModel.IsDirty)
                {
                    RemoveDocument(ActiveDocument);
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
                            RemoveDocument(ActiveDocument);
                            break;
                        }
                        case DialogResult.No:
                        {
                            RemoveDocument(ActiveDocument);
                            break;
                        }
                        case DialogResult.Cancel:
                        {
                            return;
                        }
                    }
                }
            }
            else
            {
                RemoveDocument(documentToClose);
            }

        }

        private void RemoveDocument(IDocument documentToRemove)
        {
            if (documentToRemove.IsScript)
            {
                _sectionBrowserViewModel.ClearDown();
            }

            OpenDocuments.Remove(documentToRemove);
        }


    }
}
