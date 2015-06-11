using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Filtration.Models;
using Filtration.Services;
using Filtration.Translators;
using Filtration.Views;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Filtration.ViewModels
{
    internal interface IMainWindowViewModel
    {
        void LoadScriptFromFile(string path);
        RelayCommand OpenScriptCommand { get; }
        RelayCommand NewScriptCommand { get; }
    }

    internal class MainWindowViewModel : FiltrationViewModelBase, IMainWindowViewModel
    {
        private readonly IItemFilterScriptViewModelFactory _itemFilterScriptViewModelFactory;
        private readonly IItemFilterPersistenceService _persistenceService;
        private readonly IItemFilterScriptTranslator _itemFilterScriptTranslator;
        private readonly IReplaceColorsViewModel _replaceColorsViewModel;
        private readonly IAvalonDockWorkspaceViewModel _avalonDockWorkspaceViewModel;

        private IDocument _activeDocument;

        public MainWindowViewModel(IItemFilterScriptViewModelFactory itemFilterScriptViewModelFactory,
                                   IItemFilterPersistenceService persistenceService,
                                   IItemFilterScriptTranslator itemFilterScriptTranslator,
                                   IReplaceColorsViewModel replaceColorsViewModel,
                                   IAvalonDockWorkspaceViewModel avalonDockWorkspaceViewModel)
        {
            _itemFilterScriptViewModelFactory = itemFilterScriptViewModelFactory;
            _persistenceService = persistenceService;
            _itemFilterScriptTranslator = itemFilterScriptTranslator;
            _replaceColorsViewModel = replaceColorsViewModel;
            _avalonDockWorkspaceViewModel = avalonDockWorkspaceViewModel;


            OpenAboutWindowCommand = new RelayCommand(OnOpenAboutWindowCommand);
            OpenScriptCommand = new RelayCommand(OnOpenScriptCommand);
            SaveScriptCommand = new RelayCommand(OnSaveScriptCommand, ActiveDocumentIsScript);
            SaveScriptAsCommand = new RelayCommand(OnSaveScriptAsCommand, ActiveDocumentIsScript);
            CopyScriptCommand = new RelayCommand(OnCopyScriptCommand, ActiveDocumentIsScript);
            CopyBlockCommand = new RelayCommand(OnCopyBlockCommand, () => ActiveDocumentIsScript() && (_avalonDockWorkspaceViewModel.ActiveScriptViewModel.SelectedBlockViewModel != null));
            PasteCommand = new RelayCommand(OnPasteCommand, () => ActiveDocumentIsScript() && (_avalonDockWorkspaceViewModel.ActiveScriptViewModel.SelectedBlockViewModel != null));
            NewScriptCommand = new RelayCommand(OnNewScriptCommand);
            CloseScriptCommand = new RelayCommand(OnCloseScriptCommand, ActiveDocumentIsScript);
            ReplaceColorsCommand = new RelayCommand(OnReplaceColorsCommand, ActiveDocumentIsScript);

            //LoadScriptFromFile("C:\\ThioleLootFilter.txt");

            SetItemFilterScriptDirectory();

            Messenger.Default.Register<NotificationMessage>(this, message =>
            {
                switch (message.Notification)
                {
                    case "ActiveDocumentChanged":
                    {
                        _activeDocument = _avalonDockWorkspaceViewModel.ActiveDocument;
                        SaveScriptCommand.RaiseCanExecuteChanged();
                        SaveScriptAsCommand.RaiseCanExecuteChanged();
                        CopyScriptCommand.RaiseCanExecuteChanged();
                        CopyBlockCommand.RaiseCanExecuteChanged();
                        PasteCommand.RaiseCanExecuteChanged();
                        NewScriptCommand.RaiseCanExecuteChanged();
                        CloseScriptCommand.RaiseCanExecuteChanged();
                        ReplaceColorsCommand.RaiseCanExecuteChanged();
                        break;
                    }
                }
            });
        }

        public RelayCommand OpenScriptCommand { get; private set; }
        public RelayCommand SaveScriptCommand { get; private set; }
        public RelayCommand SaveScriptAsCommand { get; private set; }
        public RelayCommand CopyBlockCommand { get; private set; }
        public RelayCommand PasteCommand { get; private set; }
        public RelayCommand CopyScriptCommand { get; private set; }
        public RelayCommand NewScriptCommand { get; private set; }
        public RelayCommand CloseScriptCommand { get; private set; }
        public RelayCommand OpenAboutWindowCommand { get; private set; }
        public RelayCommand ReplaceColorsCommand { get; private set; }

        public IAvalonDockWorkspaceViewModel AvalonDockWorkspaceViewModel
        {
            get { return _avalonDockWorkspaceViewModel; }
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

        private bool ActiveDocumentIsScript()
        {
            return _activeDocument != null && _activeDocument.IsScript;
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
            var loadedScript = _persistenceService.LoadItemFilterScript(path);
            try
            {
                
            }
            catch (Exception e)
            {
                MessageBox.Show(@"Error loading filter script - " + e.Message, @"Script Load Error", MessageBoxButtons.OK,
                   MessageBoxIcon.Error);
                return;
            }

            var newViewModel = _itemFilterScriptViewModelFactory.Create();
            newViewModel.Initialise(loadedScript);
            _avalonDockWorkspaceViewModel.AddDocument(newViewModel);
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
           _avalonDockWorkspaceViewModel.ActiveScriptViewModel.SaveScript();
        }

        private void OnSaveScriptAsCommand()
        {
           _avalonDockWorkspaceViewModel.ActiveScriptViewModel.SaveScriptAs();
        }

        private void OnReplaceColorsCommand()
        {
            _replaceColorsViewModel.Initialise(_avalonDockWorkspaceViewModel.ActiveScriptViewModel.Script);
            var replaceColorsWindow = new ReplaceColorsWindow {DataContext = _replaceColorsViewModel};
            replaceColorsWindow.ShowDialog();
        }

        private void OnCopyScriptCommand()
        {
            Clipboard.SetText(_itemFilterScriptTranslator.TranslateItemFilterScriptToString(_avalonDockWorkspaceViewModel.ActiveScriptViewModel.Script));
        }

        private void OnCopyBlockCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.CopyBlock(_avalonDockWorkspaceViewModel.ActiveScriptViewModel.SelectedBlockViewModel);
        }

        private void OnPasteCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.PasteBlock(_avalonDockWorkspaceViewModel.ActiveScriptViewModel.SelectedBlockViewModel);
        }

        private void OnNewScriptCommand()
        {
            var newScript = new ItemFilterScript();
            var newViewModel = _itemFilterScriptViewModelFactory.Create();
            newViewModel.Initialise(newScript);
            newViewModel.Description = "New Script";
            _avalonDockWorkspaceViewModel.AddDocument(newViewModel);
        }

        private void OnCloseScriptCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.Close();
        }
    }
}
