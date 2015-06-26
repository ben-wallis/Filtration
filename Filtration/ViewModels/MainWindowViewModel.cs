using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Filtration.Common.ViewModels;
using Filtration.Interface;
using Filtration.ObjectModel.ThemeEditor;
using Filtration.Repositories;
using Filtration.ThemeEditor.Providers;
using Filtration.ThemeEditor.Services;
using Filtration.ThemeEditor.ViewModels;
using Filtration.Translators;
using Filtration.Views;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Clipboard = System.Windows.Clipboard;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Filtration.ViewModels
{
    internal interface IMainWindowViewModel
    {
        RelayCommand OpenScriptCommand { get; }
        RelayCommand NewScriptCommand { get; }
    }

    internal class MainWindowViewModel : FiltrationViewModelBase, IMainWindowViewModel
    {
        private readonly IItemFilterScriptRepository _itemFilterScriptRepository;
        private readonly IItemFilterScriptTranslator _itemFilterScriptTranslator;
        private readonly IReplaceColorsViewModel _replaceColorsViewModel;
        private readonly IAvalonDockWorkspaceViewModel _avalonDockWorkspaceViewModel;
        private readonly ISettingsWindowViewModel _settingsWindowViewModel;
        private readonly IThemeProvider _themeProvider;
        private readonly IThemeService _themeService;

        private IDocument _activeDocument;

        public MainWindowViewModel(IItemFilterScriptRepository itemFilterScriptRepository,
                                   IItemFilterScriptTranslator itemFilterScriptTranslator,
                                   IReplaceColorsViewModel replaceColorsViewModel,
                                   IAvalonDockWorkspaceViewModel avalonDockWorkspaceViewModel,
                                   ISettingsWindowViewModel settingsWindowViewModel,
                                   IThemeProvider themeProvider,
                                   IThemeService themeService)
        {
            _itemFilterScriptRepository = itemFilterScriptRepository;
            _itemFilterScriptTranslator = itemFilterScriptTranslator;
            _replaceColorsViewModel = replaceColorsViewModel;
            _avalonDockWorkspaceViewModel = avalonDockWorkspaceViewModel;
            _settingsWindowViewModel = settingsWindowViewModel;
            _themeProvider = themeProvider;
            _themeService = themeService;

            NewScriptCommand = new RelayCommand(OnNewScriptCommand);
            CopyScriptCommand = new RelayCommand(OnCopyScriptCommand, ActiveDocumentIsScript);
            OpenScriptCommand = new RelayCommand(OnOpenScriptCommand);
            OpenThemeCommand = new RelayCommand(OnOpenThemeCommand);


            SaveCommand = new RelayCommand(OnSaveDocumentCommand, ActiveDocumentIsEditable);
            SaveAsCommand = new RelayCommand(OnSaveAsCommand, ActiveDocumentIsEditable);
            CloseCommand = new RelayCommand(OnCloseDocumentCommand, () => AvalonDockWorkspaceViewModel.ActiveDocument != null);

            CopyBlockCommand = new RelayCommand(OnCopyBlockCommand, () => ActiveDocumentIsScript() && (_avalonDockWorkspaceViewModel.ActiveScriptViewModel.SelectedBlockViewModel != null));
            PasteCommand = new RelayCommand(OnPasteCommand, () => ActiveDocumentIsScript() && (_avalonDockWorkspaceViewModel.ActiveScriptViewModel.SelectedBlockViewModel != null));

            OpenAboutWindowCommand = new RelayCommand(OnOpenAboutWindowCommand);
            OpenSettingsWindowCommand = new RelayCommand(OnOpenSettingsWindowCommand);
            ReplaceColorsCommand = new RelayCommand(OnReplaceColorsCommand, ActiveDocumentIsScript);
            CreateThemeCommand = new RelayCommand(OnCreateThemeCommand, ActiveDocumentIsScript);
            ApplyThemeToScriptCommand = new RelayCommand(OnApplyThemeToScriptCommand, ActiveDocumentIsScript);

            //LoadScriptFromFile("C:\\ThioleLootFilter.txt");

            if (string.IsNullOrEmpty(_itemFilterScriptRepository.GetItemFilterScriptDirectory()))
            {
                SetItemFilterScriptDirectory();
            }

            Messenger.Default.Register<NotificationMessage>(this, message =>
            {
                switch (message.Notification)
                {
                    case "ActiveDocumentChanged":
                    {
                        _activeDocument = _avalonDockWorkspaceViewModel.ActiveDocument;
                        
                        CopyScriptCommand.RaiseCanExecuteChanged();
                        SaveCommand.RaiseCanExecuteChanged();
                        SaveAsCommand.RaiseCanExecuteChanged();
                        CloseCommand.RaiseCanExecuteChanged();
                        CopyBlockCommand.RaiseCanExecuteChanged();
                        PasteCommand.RaiseCanExecuteChanged();
                        ReplaceColorsCommand.RaiseCanExecuteChanged();
                        ApplyThemeToScriptCommand.RaiseCanExecuteChanged();
                        CreateThemeCommand.RaiseCanExecuteChanged();
                        break;
                    }
                    case "NewScript":
                    {
                        OnNewScriptCommand();
                        break;
                    }
                    case "OpenScript":
                    {
                        OnOpenScriptCommand();
                        break;
                    }
                }
            });
        }

        public RelayCommand OpenScriptCommand { get; private set; }
        public RelayCommand OpenThemeCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand SaveAsCommand { get; private set; }
        public RelayCommand CopyBlockCommand { get; private set; }
        public RelayCommand PasteCommand { get; private set; }
        public RelayCommand CopyScriptCommand { get; private set; }
        public RelayCommand NewScriptCommand { get; private set; }
        public RelayCommand CloseCommand { get; private set; }
        public RelayCommand OpenAboutWindowCommand { get; private set; }
        public RelayCommand OpenSettingsWindowCommand { get; private set; }
        public RelayCommand ReplaceColorsCommand { get; private set; }
        public RelayCommand CreateThemeCommand { get; private set; }
        public RelayCommand ApplyThemeToScriptCommand { get; private set; }

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
            return _activeDocument is IItemFilterScriptViewModel;
        }

        private bool ActiveDocumentIsEditable()
        {
            return _activeDocument is IEditableDocument;
        }

        private void OnCreateThemeCommand()
        {
            var themeViewModel = _themeProvider.NewThemeForScript(AvalonDockWorkspaceViewModel.ActiveScriptViewModel.Script);
            OpenTheme(themeViewModel);
        }
        
        private void OpenTheme(IThemeViewModel themeViewModel)
        {
            if (AvalonDockWorkspaceViewModel.OpenDocuments.Contains(themeViewModel))
            {
                AvalonDockWorkspaceViewModel.SwitchActiveDocument(themeViewModel);
            }
            else
            {
                AvalonDockWorkspaceViewModel.AddDocument(themeViewModel);
            }
        }

        private void OnOpenAboutWindowCommand()
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void OnOpenSettingsWindowCommand()
        {
            var settingsWindow = new SettingsWindow {DataContext = _settingsWindowViewModel};
            _settingsWindowViewModel.OnRequestClose += (s, e) => settingsWindow.Close();
            settingsWindow.ShowDialog();
        }

        private void OnOpenScriptCommand()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Filter Files (*.filter)|*.filter|All Files (*.*)|*.*",
                InitialDirectory = _itemFilterScriptRepository.GetItemFilterScriptDirectory()
            };

            if (openFileDialog.ShowDialog() != true) return;

            IItemFilterScriptViewModel loadedViewModel;

            try
            {
                loadedViewModel = _itemFilterScriptRepository.LoadScriptFromFile(openFileDialog.FileName);
            }
            catch(IOException e)
            {
                MessageBox.Show(@"Error loading filter script - " + e.Message, @"Script Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            _avalonDockWorkspaceViewModel.AddDocument(loadedViewModel);
        }

        private void OnOpenThemeCommand()
        {

            var filePath = ShowOpenThemeDialog();
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            IThemeViewModel loadedViewModel;

            try
            {
                loadedViewModel = _themeProvider.LoadThemeFromFile(filePath);
            }
            catch (IOException e)
            {
                MessageBox.Show(@"Error loading filter theme - " + e.Message, @"Theme Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            
            _avalonDockWorkspaceViewModel.AddDocument(loadedViewModel);
        }

        private void OnApplyThemeToScriptCommand()
        {
            var filePath = ShowOpenThemeDialog();
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            Theme loadedTheme;

            try
            {
                loadedTheme = _themeProvider.LoadThemeModelFromFile(filePath);
            }
            catch (IOException e)
            {
                MessageBox.Show(@"Error loading filter theme - " + e.Message, @"Theme Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show(@"Are you sure you wish to apply this theme to the current filter script?",
                @"Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                return;
            }

            _themeService.ApplyThemeToScript(loadedTheme, AvalonDockWorkspaceViewModel.ActiveScriptViewModel.Script);
            AvalonDockWorkspaceViewModel.ActiveScriptViewModel.SetDirtyFlag();
        }


        private string ShowOpenThemeDialog()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Filter Theme Files (*.filtertheme)|*.filtertheme|All Files (*.*)|*.*",
                InitialDirectory = _itemFilterScriptRepository.GetItemFilterScriptDirectory()
            };

            return openFileDialog.ShowDialog() != true ? string.Empty : openFileDialog.FileName;
        }



        private void SetItemFilterScriptDirectory()
        {
            var dlg = new FolderBrowserDialog
            {
                Description = @"Select your Path of Exile data directory, usually in Documents\My Games",
                ShowNewFolderButton = false
            };
            var result = dlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                _itemFilterScriptRepository.SetItemFilterScriptDirectory(dlg.SelectedPath);
            }
        }

        private void OnSaveDocumentCommand()
        {
            ((IEditableDocument)_avalonDockWorkspaceViewModel.ActiveDocument).Save();
        }

        private void OnSaveAsCommand()
        {
            ((IEditableDocument)_avalonDockWorkspaceViewModel.ActiveDocument).SaveAs();
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
            var newViewModel = _itemFilterScriptRepository.NewScript();
            _avalonDockWorkspaceViewModel.AddDocument(newViewModel);
        }

        private void OnCloseDocumentCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveDocument.Close();
        }
    }
}
