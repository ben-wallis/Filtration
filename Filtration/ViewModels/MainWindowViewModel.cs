using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Filtration.Common.Services;
using Filtration.Common.ViewModels;
using Filtration.Interface;
using Filtration.Models;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.ThemeEditor;
using Filtration.Properties;
using Filtration.Repositories;
using Filtration.Services;
using Filtration.ThemeEditor.Messages;
using Filtration.ThemeEditor.Providers;
using Filtration.ThemeEditor.Services;
using Filtration.ThemeEditor.ViewModels;
using Filtration.Translators;
using Filtration.Views;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NLog;
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
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IItemFilterScriptRepository _itemFilterScriptRepository;
        private readonly IItemFilterScriptTranslator _itemFilterScriptTranslator;
        private readonly IReplaceColorsViewModel _replaceColorsViewModel;
        private readonly IAvalonDockWorkspaceViewModel _avalonDockWorkspaceViewModel;
        private readonly ISettingsPageViewModel _settingsPageViewModel;
        private readonly IThemeProvider _themeProvider;
        private readonly IThemeService _themeService;
        private readonly IUpdateCheckService _updateCheckService;
        private readonly IUpdateAvailableViewModel _updateAvailableViewModel;
        private readonly IMessageBoxService _messageBoxService;

        public MainWindowViewModel(IItemFilterScriptRepository itemFilterScriptRepository,
                                   IItemFilterScriptTranslator itemFilterScriptTranslator,
                                   IReplaceColorsViewModel replaceColorsViewModel,
                                   IAvalonDockWorkspaceViewModel avalonDockWorkspaceViewModel,
                                   ISettingsPageViewModel settingsPageViewModel,
                                   IThemeProvider themeProvider,
                                   IThemeService themeService,
                                   IUpdateCheckService updateCheckService,
                                   IUpdateAvailableViewModel updateAvailableViewModel,
                                   IMessageBoxService messageBoxService)
        {
            _itemFilterScriptRepository = itemFilterScriptRepository;
            _itemFilterScriptTranslator = itemFilterScriptTranslator;
            _replaceColorsViewModel = replaceColorsViewModel;
            _avalonDockWorkspaceViewModel = avalonDockWorkspaceViewModel;
            _settingsPageViewModel = settingsPageViewModel;
            _themeProvider = themeProvider;
            _themeService = themeService;
            _updateCheckService = updateCheckService;
            _updateAvailableViewModel = updateAvailableViewModel;
            _messageBoxService = messageBoxService;

            NewScriptCommand = new RelayCommand(OnNewScriptCommand);
            CopyScriptCommand = new RelayCommand(OnCopyScriptCommand, () => ActiveDocumentIsScript);
            OpenScriptCommand = new RelayCommand(OnOpenScriptCommand);
            OpenThemeCommand = new RelayCommand(OnOpenThemeCommand);

            SaveCommand = new RelayCommand(OnSaveDocumentCommand, ActiveDocumentIsEditable);
            SaveAsCommand = new RelayCommand(OnSaveAsCommand, ActiveDocumentIsEditable);
            CloseCommand = new RelayCommand(OnCloseDocumentCommand, ActiveDocumentIsEditable);

            CopyBlockCommand = new RelayCommand(OnCopyBlockCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedBlock);
            CopyBlockStyleCommand = new RelayCommand(OnCopyBlockStyleCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedBlock);
            PasteCommand = new RelayCommand(OnPasteCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedBlock);
            PasteBlockStyleCommand = new RelayCommand(OnPasteBlockStyleCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedBlock);

            MoveBlockUpCommand = new RelayCommand(OnMoveBlockUpCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedBlock);
            MoveBlockDownCommand = new RelayCommand(OnMoveBlockDownCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedBlock);
            MoveBlockToTopCommand = new RelayCommand(OnMoveBlockToTopCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedBlock);
            MoveBlockToBottomCommand = new RelayCommand(OnMoveBlockToBottomCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedBlock);

            AddBlockCommand = new RelayCommand(OnAddBlockCommand, () => ActiveDocumentIsScript);
            AddSectionCommand = new RelayCommand(OnAddSectionCommand, () => ActiveDocumentIsScript);
            DeleteBlockCommand = new RelayCommand(OnDeleteBlockCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedBlock);
            DisableBlockCommand = new RelayCommand(OnDisableBlockCommand,
                () => ActiveDocumentIsScript && ActiveScriptHasSelectedEnabledBlock);
            EnableBlockCommand = new RelayCommand(OnEnableBlockCommand,
                () => ActiveDocumentIsScript && ActiveScriptHasSelectedDisabledBlock);
            OpenAboutWindowCommand = new RelayCommand(OnOpenAboutWindowCommand);
            ReplaceColorsCommand = new RelayCommand(OnReplaceColorsCommand, () => ActiveDocumentIsScript);

            CreateThemeCommand = new RelayCommand(OnCreateThemeCommand, () => ActiveDocumentIsScript);
            ApplyThemeToScriptCommand = new RelayCommand(OnApplyThemeToScriptCommand, () => ActiveDocumentIsScript);
            EditMasterThemeCommand = new RelayCommand(OnEditMasterThemeCommand, () => ActiveDocumentIsScript);

            AddTextColorThemeComponentCommand = new RelayCommand(OnAddTextColorThemeComponentCommand, () => ActiveDocumentIsTheme && ActiveThemeIsEditable);
            AddBackgroundColorThemeComponentCommand = new RelayCommand(OnAddBackgroundColorThemeComponentCommand, () => ActiveDocumentIsTheme && ActiveThemeIsEditable);
            AddBorderColorThemeComponentCommand = new RelayCommand(OnAddBorderColorThemeComponentCommand, () => ActiveDocumentIsTheme && ActiveThemeIsEditable);
            DeleteThemeComponentCommand = new RelayCommand(OnDeleteThemeComponentCommand,
                () =>
                    ActiveDocumentIsTheme && ActiveDocumentIsTheme &&
                    _avalonDockWorkspaceViewModel.ActiveThemeViewModel.SelectedThemeComponent != null);

            ExpandAllBlocksCommand = new RelayCommand(OnExpandAllBlocksCommand, () => ActiveDocumentIsScript);
            CollapseAllBlocksCommand = new RelayCommand(OnCollapseAllBlocksCommand, () => ActiveDocumentIsScript);

            ToggleShowAdvancedCommand = new RelayCommand<bool>(OnToggleShowAdvancedCommand, s => ActiveDocumentIsScript);
            ClearFiltersCommand = new RelayCommand(OnClearFiltersCommand, () => ActiveDocumentIsScript);

            if (string.IsNullOrEmpty(_itemFilterScriptRepository.GetItemFilterScriptDirectory()))
            {
                SetItemFilterScriptDirectory();
            }

            var icon = new BitmapImage();
            icon.BeginInit();
            icon.UriSource = new Uri("pack://application:,,,/Filtration;component/Resources/Icons/filtration_icon.png");
            icon.EndInit();
            Icon = icon;

            Messenger.Default.Register<ThemeClosedMessage>(this, message =>
            {
                if (message.ClosedViewModel == null) return;
                AvalonDockWorkspaceViewModel.CloseDocument(message.ClosedViewModel);
            });

            Messenger.Default.Register<NotificationMessage>(this, message =>
            {
                switch (message.Notification)
                {
                    case "ActiveDocumentChanged":
                    {
                        CopyScriptCommand.RaiseCanExecuteChanged();
                        SaveCommand.RaiseCanExecuteChanged();
                        SaveAsCommand.RaiseCanExecuteChanged();
                        CloseCommand.RaiseCanExecuteChanged();
                        CopyBlockCommand.RaiseCanExecuteChanged();
                        PasteCommand.RaiseCanExecuteChanged();
                        ReplaceColorsCommand.RaiseCanExecuteChanged();
                        ApplyThemeToScriptCommand.RaiseCanExecuteChanged();
                        EditMasterThemeCommand.RaiseCanExecuteChanged();
                        CreateThemeCommand.RaiseCanExecuteChanged();
                        RaisePropertyChanged("ActiveDocumentIsScript");
                        RaisePropertyChanged("ActiveDocumentIsTheme");
                        RaisePropertyChanged("ShowAdvancedStatus");
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
            CheckForUpdates();
        }

        public RelayCommand OpenScriptCommand { get; private set; }
        public RelayCommand OpenThemeCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand SaveAsCommand { get; private set; }
        public RelayCommand CopyBlockCommand { get; private set; }
        public RelayCommand CopyBlockStyleCommand { get; private set; }
        public RelayCommand PasteCommand { get; private set; }
        public RelayCommand PasteBlockStyleCommand { get; private set; }
        public RelayCommand CopyScriptCommand { get; private set; }
        public RelayCommand NewScriptCommand { get; private set; }
        public RelayCommand CloseCommand { get; private set; }
        public RelayCommand OpenAboutWindowCommand { get; private set; }
        public RelayCommand ReplaceColorsCommand { get; private set; }

        public RelayCommand EditMasterThemeCommand { get; private set; }
        public RelayCommand CreateThemeCommand { get; private set; }
        public RelayCommand ApplyThemeToScriptCommand { get; private set; }

        public RelayCommand AddTextColorThemeComponentCommand { get; private set; }
        public RelayCommand AddBackgroundColorThemeComponentCommand { get; private set; }
        public RelayCommand AddBorderColorThemeComponentCommand { get; private set; }
        public RelayCommand DeleteThemeComponentCommand { get; private set; }

        public RelayCommand AddBlockCommand { get; private set; }
        public RelayCommand AddSectionCommand { get; private set; }
        public RelayCommand DeleteBlockCommand { get; private set; }
        public RelayCommand DisableBlockCommand { get; private set; }
        public RelayCommand EnableBlockCommand { get; private set; }

        public RelayCommand MoveBlockUpCommand { get; private set; }
        public RelayCommand MoveBlockDownCommand { get; private set; }
        public RelayCommand MoveBlockToTopCommand { get; private set; }
        public RelayCommand MoveBlockToBottomCommand { get; private set; }

        public RelayCommand ExpandAllBlocksCommand { get; private set; }
        public RelayCommand CollapseAllBlocksCommand { get; private set; }

        public RelayCommand<bool> ToggleShowAdvancedCommand { get; private set; }
        public RelayCommand ClearFiltersCommand { get; private set; }


        public async void CheckForUpdates()
        {
            var assemblyVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            var result = await _updateCheckService.GetUpdateData();

            try
            {
                if (result.LatestVersionMajorPart >= assemblyVersion.FileMajorPart &&
                    result.LatestVersionMinorPart > assemblyVersion.FileMinorPart)
                {
                    if (Settings.Default.SuppressUpdates == false ||
                        LatestVersionIsNewerThanSuppressedVersion(result))
                    {
                        Settings.Default.SuppressUpdates = false;
                        Settings.Default.Save();
                        var updateAvailableView = new UpdateAvailableView {DataContext = _updateAvailableViewModel};
                        _updateAvailableViewModel.Initialise(result, assemblyVersion.FileMajorPart,
                            assemblyVersion.FileMinorPart);
                        _updateAvailableViewModel.OnRequestClose += (s, e) => updateAvailableView.Close();
                        updateAvailableView.ShowDialog();
                    }
                }
            }
            catch (Exception e)
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug(e);
                }
                // We don't care if the update check fails, because it could fail for multiple reasons 
                // including the user blocking Filtration in their firewall.
            }
        }

        private bool LatestVersionIsNewerThanSuppressedVersion(UpdateData updateData)
        {
            return Settings.Default.SuppressUpdatesUpToVersionMajorPart < updateData.LatestVersionMajorPart ||
                   (Settings.Default.SuppressUpdatesUpToVersionMajorPart <= updateData.LatestVersionMajorPart &&
                    Settings.Default.SuppressUpdatesUpToVersionMinorPart < updateData.LatestVersionMinorPart);
        }

        public ImageSource Icon { get; private set; }

        public IAvalonDockWorkspaceViewModel AvalonDockWorkspaceViewModel
        {
            get { return _avalonDockWorkspaceViewModel; }
        }

        public ISettingsPageViewModel SettingsPageViewModel
        {
            get { return _settingsPageViewModel; }
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

        public bool ActiveDocumentIsScript
        {
            get { return _avalonDockWorkspaceViewModel.ActiveDocument != null && _avalonDockWorkspaceViewModel.ActiveDocument.IsScript; }
        }

        public bool ActiveDocumentIsTheme
        {
            get { return _avalonDockWorkspaceViewModel.ActiveDocument!= null && _avalonDockWorkspaceViewModel.ActiveDocument.IsTheme; }
        }

        public bool ActiveScriptHasSelectedBlock
        {
            get { return AvalonDockWorkspaceViewModel.ActiveScriptViewModel.SelectedBlockViewModel != null; }
        }

        public bool ActiveScriptHasSelectedEnabledBlock
        {
            get { return AvalonDockWorkspaceViewModel.ActiveScriptViewModel.HasSelectedEnabledBlock(); }
        }

        public bool ActiveScriptHasSelectedDisabledBlock
        {
            get { return AvalonDockWorkspaceViewModel.ActiveScriptViewModel.HasSelectedDisabledBlock(); }
        }
        
        public bool ActiveThemeIsEditable
        {
            get { return AvalonDockWorkspaceViewModel.ActiveThemeViewModel.IsMasterTheme; }
        }

        private bool ActiveDocumentIsEditable()
        {
            return AvalonDockWorkspaceViewModel.ActiveDocument is IEditableDocument;
        }

        public bool ShowAdvancedStatus
        {
            get
            {
                return ActiveDocumentIsScript && _avalonDockWorkspaceViewModel.ActiveScriptViewModel.ShowAdvanced;
            }
        }

        private void OnCreateThemeCommand()
        {
            var themeViewModel = _themeProvider.NewThemeForScript(AvalonDockWorkspaceViewModel.ActiveScriptViewModel.Script);
            OpenTheme(themeViewModel);
        }

        private void OnEditMasterThemeCommand()
        {
            var openMasterThemForScript =
                AvalonDockWorkspaceViewModel.OpenMasterThemeForScript(AvalonDockWorkspaceViewModel.ActiveScriptViewModel);

            if (openMasterThemForScript != null)
            {
                AvalonDockWorkspaceViewModel.SwitchActiveDocument(openMasterThemForScript);
            }
            else
            {
                var themeViewModel =
                    _themeProvider.MasterThemeForScript(AvalonDockWorkspaceViewModel.ActiveScriptViewModel.Script);
                OpenTheme(themeViewModel);
            }
        }
        
        private void OpenTheme(IThemeEditorViewModel themeEditorViewModel)
        {
            if (AvalonDockWorkspaceViewModel.OpenDocuments.Contains(themeEditorViewModel))
            {
                AvalonDockWorkspaceViewModel.SwitchActiveDocument(themeEditorViewModel);
            }
            else
            {
                AvalonDockWorkspaceViewModel.AddDocument(themeEditorViewModel);
            }
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
                if (_logger.IsErrorEnabled)
                {
                    _logger.Error(e);
                }
                _messageBoxService.Show("Script Load Error", "Error loading filter script - " + e.Message,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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

            IThemeEditorViewModel loadedViewModel;

            try
            {
                loadedViewModel = _themeProvider.LoadThemeFromFile(filePath);
            }
            catch (IOException e)
            {
                if (_logger.IsErrorEnabled)
                {
                    _logger.Error(e);
                }
                _messageBoxService.Show("Theme Load Error", "Error loading filter theme - " + e.Message,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
                if (_logger.IsErrorEnabled)
                {
                    _logger.Error(e);
                }
                _messageBoxService.Show("Theme Load Error", "Error loading filter theme - " + e.Message,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var result = _messageBoxService.Show("Confirm",
                "Are you sure you wish to apply this theme to the current filter script?", MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
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
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.CopyBlockCommand.Execute(null);
        }

        private void OnCopyBlockStyleCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.CopyBlockStyleCommand.Execute(null);
        }

        private void OnPasteCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.PasteBlockCommand.Execute(null);
        }

        private void OnPasteBlockStyleCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.PasteBlockStyleCommand.Execute(null);
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

        private void OnMoveBlockUpCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.MoveBlockUpCommand.Execute(null);
        }

        private void OnMoveBlockDownCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.MoveBlockDownCommand.Execute(null);
        }

        private void OnMoveBlockToTopCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.MoveBlockToTopCommand.Execute(null);
        }

        private void OnMoveBlockToBottomCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.MoveBlockToBottomCommand.Execute(null);
        }

        private void OnAddBlockCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.AddBlockCommand.Execute(null);
        }

        private void OnAddSectionCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.AddSectionCommand.Execute(null);
        }

        private void OnDeleteBlockCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.DeleteBlockCommand.Execute(null);
        }

        private void OnDisableBlockCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.DisableBlockCommand.Execute(null);
        }

        private void OnEnableBlockCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.EnableBlockCommand.Execute(null);
        }

        private void OnExpandAllBlocksCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.ExpandAllBlocksCommand.Execute(null);
        }

        private void OnCollapseAllBlocksCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.CollapseAllBlocksCommand.Execute(null);
        }

        private void OnToggleShowAdvancedCommand(bool showAdvanced)
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.ToggleShowAdvancedCommand.Execute(showAdvanced);
        }

        private void OnClearFiltersCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.ClearFilterCommand.Execute(null);
        }

        private void OnAddTextColorThemeComponentCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveThemeViewModel.AddThemeComponentCommand.Execute(ThemeComponentType.TextColor);
        }

        private void OnAddBackgroundColorThemeComponentCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveThemeViewModel.AddThemeComponentCommand.Execute(ThemeComponentType.BackgroundColor);
        }

        private void OnAddBorderColorThemeComponentCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveThemeViewModel.AddThemeComponentCommand.Execute(ThemeComponentType.BorderColor);
        }

        private void OnDeleteThemeComponentCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveThemeViewModel.DeleteThemeComponentCommand.Execute(
                _avalonDockWorkspaceViewModel.ActiveThemeViewModel.SelectedThemeComponent);
        }
    }
}
