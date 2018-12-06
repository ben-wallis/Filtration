using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Filtration.Common.Services;
using Filtration.Interface;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.ThemeEditor;
using Filtration.Parser.Interface.Services;
using Filtration.Properties;
using Filtration.Repositories;
using Filtration.Services;
using Filtration.ThemeEditor.Messages;
using Filtration.ThemeEditor.Providers;
using Filtration.ThemeEditor.Services;
using Filtration.ThemeEditor.ViewModels;
using Filtration.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Filtration.ViewModels
{
    internal interface IMainWindowViewModel
    {
        RelayCommand OpenScriptCommand { get; }
        RelayCommand NewScriptCommand { get; }
        int WindowWidth { get; set; }
        int WindowHeight { get; set; }
        IUpdateViewModel UpdateViewModel { get; }
        Task<bool> CloseAllDocumentsAsync();
        Task OpenDroppedFilesAsync(List<string> filenames);
    }

    internal class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IItemFilterScriptDirectoryService _itemFilterScriptDirectoryService;
        private readonly IItemFilterScriptRepository _itemFilterScriptRepository;
        private readonly IItemFilterScriptTranslator _itemFilterScriptTranslator;
        private readonly IReplaceColorsViewModel _replaceColorsViewModel;
        private readonly IAvalonDockWorkspaceViewModel _avalonDockWorkspaceViewModel;
        private readonly IScriptLoadingService _scriptLoadingService;
        private readonly IThemeProvider _themeProvider;
        private readonly IThemeService _themeService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IClipboardService _clipboardService;
        private bool _showLoadingBanner;
        private WindowState _windowState;
        private int _windowWidth;
        private int _windowHeight;

        public MainWindowViewModel(IItemFilterScriptDirectoryService itemFilterScriptDirectoryService,
                                   IItemFilterScriptRepository itemFilterScriptRepository,
                                   IItemFilterScriptTranslator itemFilterScriptTranslator,
                                   IReplaceColorsViewModel replaceColorsViewModel,
                                   IAvalonDockWorkspaceViewModel avalonDockWorkspaceViewModel,
                                   IScriptLoadingService scriptLoadingService,
                                   ISettingsPageViewModel settingsPageViewModel,
                                   IThemeProvider themeProvider,
                                   IThemeService themeService,
                                   IMessageBoxService messageBoxService,
                                   IClipboardService clipboardService,
                                   IUpdateViewModel updateViewModel)
        {
            _itemFilterScriptDirectoryService = itemFilterScriptDirectoryService;
            _itemFilterScriptRepository = itemFilterScriptRepository;
            _itemFilterScriptTranslator = itemFilterScriptTranslator;
            _replaceColorsViewModel = replaceColorsViewModel;
            _avalonDockWorkspaceViewModel = avalonDockWorkspaceViewModel;
            _scriptLoadingService = scriptLoadingService;
            SettingsPageViewModel = settingsPageViewModel;
            _themeProvider = themeProvider;
            _themeService = themeService;
            _messageBoxService = messageBoxService;
            _clipboardService = clipboardService;
            UpdateViewModel = updateViewModel;

            _windowState = Settings.Default.WindowState;
            _windowWidth = Settings.Default.WindowWidth;
            _windowHeight = Settings.Default.WindowHeight;

            NewScriptCommand = new RelayCommand(OnNewScriptCommand);
            CopyScriptCommand = new RelayCommand(OnCopyScriptCommand, () => ActiveDocumentIsScript);
            OpenScriptCommand = new RelayCommand(async () => await OnOpenScriptCommand());
            OpenThemeCommand = new RelayCommand(async () => await OnOpenThemeCommandAsync());

            SaveCommand = new RelayCommand(async () => await OnSaveDocumentCommandAsync(), ActiveDocumentIsEditable);
            SaveAsCommand = new RelayCommand(async () => await OnSaveAsCommandAsync(), ActiveDocumentIsEditable);
            CloseCommand = new RelayCommand(OnCloseDocumentCommand, ActiveDocumentIsEditable);

            CopyBlockCommand = new RelayCommand(OnCopyBlockCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedBlock);
            CopyBlockStyleCommand = new RelayCommand(OnCopyBlockStyleCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedBlock);
            PasteCommand = new RelayCommand(OnPasteCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedBlock);
            PasteBlockStyleCommand = new RelayCommand(OnPasteBlockStyleCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedBlock);

            // TODO: Only enabled if undo/redo available
            UndoCommand = new RelayCommand(OnUndoCommand, () => ActiveDocumentIsScript);
            RedoCommand = new RelayCommand(OnRedoCommand, () => ActiveDocumentIsScript);


            MoveBlockUpCommand = new RelayCommand(OnMoveBlockUpCommand, () => ActiveDocumentIsScript && ActiveScriptHasSingleSelectedeBlock);
            MoveBlockDownCommand = new RelayCommand(OnMoveBlockDownCommand, () => ActiveDocumentIsScript && ActiveScriptHasSingleSelectedeBlock);
            MoveBlockToTopCommand = new RelayCommand(OnMoveBlockToTopCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedBlock && ActiveScriptCanModifySelectedBlocks);
            MoveBlockToBottomCommand = new RelayCommand(OnMoveBlockToBottomCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedBlock && ActiveScriptCanModifySelectedBlocks);

            AddBlockCommand = new RelayCommand(OnAddBlockCommand, () => ActiveDocumentIsScript);
            AddSectionCommand = new RelayCommand(OnAddSectionCommand, () => ActiveDocumentIsScript);
            DeleteBlockCommand = new RelayCommand(OnDeleteBlockCommand, () => ActiveDocumentIsScript && ActiveScriptCanModifySelectedBlocks);
            DisableBlockCommand = new RelayCommand(OnDisableBlockCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedEnabledBlock && ActiveScriptCanModifySelectedBlocks);
            EnableBlockCommand = new RelayCommand(OnEnableBlockCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedDisabledBlock && ActiveScriptCanModifySelectedBlocks);
            DisableSectionCommand = new RelayCommand(OnDisableSectionCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedCommentBlock && ActiveScriptCanModifySelectedBlocks);
            EnableSectionCommand = new RelayCommand(OnEnableSectionCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedCommentBlock && ActiveScriptCanModifySelectedBlocks);
            ExpandSectionCommand = new RelayCommand(OnExpandSectionCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedCommentBlock);
            CollapseSectionCommand = new RelayCommand(OnCollapseSectionCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedCommentBlock);
            OpenAboutWindowCommand = new RelayCommand(OnOpenAboutWindowCommand);
            ReplaceColorsCommand = new RelayCommand(OnReplaceColorsCommand, () => ActiveDocumentIsScript);

            CreateThemeCommand = new RelayCommand(OnCreateThemeCommand, () => ActiveDocumentIsScript);
            ApplyThemeToScriptCommand = new RelayCommand(async () => await OnApplyThemeToScriptCommandAsync(), () => ActiveDocumentIsScript);
            EditMasterThemeCommand = new RelayCommand(OnEditMasterThemeCommand, () => ActiveDocumentIsScript);

            EnableDropSoundsCommand = new RelayCommand(OnEnableDropSoundsCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedBlock && ActiveScriptCanModifySelectedBlocks);
            DisableDropSoundsCommand = new RelayCommand(OnDisableDropSoundsCommand, () => ActiveDocumentIsScript && ActiveScriptHasSelectedBlock && ActiveScriptCanModifySelectedBlocks);

            AddTextColorThemeComponentCommand = new RelayCommand(OnAddTextColorThemeComponentCommand, () => ActiveDocumentIsTheme && ActiveThemeIsEditable);
            AddBackgroundColorThemeComponentCommand = new RelayCommand(OnAddBackgroundColorThemeComponentCommand, () => ActiveDocumentIsTheme && ActiveThemeIsEditable);
            AddBorderColorThemeComponentCommand = new RelayCommand(OnAddBorderColorThemeComponentCommand, () => ActiveDocumentIsTheme && ActiveThemeIsEditable);
            AddFontSizeThemeComponentCommand = new RelayCommand(OnAddFontSizeThemeComponentCommand, () => ActiveDocumentIsTheme && ActiveThemeIsEditable);
            AddAlertSoundThemeComponentCommand = new RelayCommand(OnAddAlertSoundThemeComponentCommand, () => ActiveDocumentIsTheme && ActiveThemeIsEditable);
            AddCustomSoundThemeComponentCommand = new RelayCommand(OnAddCustomSoundThemeComponentCommand, () => ActiveDocumentIsTheme && ActiveThemeIsEditable);
            AddIconThemeComponentCommand = new RelayCommand(OnAddIconThemeComponentCommand, () => ActiveDocumentIsTheme && ActiveThemeIsEditable);
            AddEffectColorThemeComponentCommand = new RelayCommand(OnAddEffectColorThemeComponentCommand, () => ActiveDocumentIsTheme && ActiveThemeIsEditable);
            DeleteThemeComponentCommand = new RelayCommand(OnDeleteThemeComponentCommand, () => ActiveDocumentIsTheme && ActiveThemeIsEditable && _avalonDockWorkspaceViewModel.ActiveThemeViewModel.SelectedThemeComponent != null);

            ExpandAllBlocksCommand = new RelayCommand(OnExpandAllBlocksCommand, () => ActiveDocumentIsScript);
            CollapseAllBlocksCommand = new RelayCommand(OnCollapseAllBlocksCommand, () => ActiveDocumentIsScript);

            ExpandAllSectionsCommand = new RelayCommand(OnExpandAllSectionsCommand, () => ActiveDocumentIsScript);
            CollapseAllSectionsCommand = new RelayCommand(OnCollapseAllSectionsCommand, () => ActiveDocumentIsScript);

            ToggleShowAdvancedCommand = new RelayCommand<bool>(OnToggleShowAdvancedCommand, s => ActiveDocumentIsScript);
            ClearFiltersCommand = new RelayCommand(OnClearFiltersCommand, () => ActiveDocumentIsScript);
            ClearStylesCommand = new RelayCommand(OnClearStylesCommand, () => ActiveDocumentIsScript);

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
#pragma warning disable 4014
                        OnOpenScriptCommand();
#pragma warning restore 4014
                        break;
                    }
                    case "ShowLoadingBanner":
                    {
                        ShowLoadingBanner = true;
                        break;
                    }
                    case "HideLoadingBanner":
                    {
                        ShowLoadingBanner = false;
                        break;
                    }
                }
            });

        }

        public RelayCommand OpenScriptCommand { get; }
        public RelayCommand OpenThemeCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SaveAsCommand { get; }
        public RelayCommand CopyBlockCommand { get; }
        public RelayCommand CopyBlockStyleCommand { get; }
        public RelayCommand PasteCommand { get; }
        public RelayCommand PasteBlockStyleCommand { get; }
        public RelayCommand CopyScriptCommand { get; }
        public RelayCommand NewScriptCommand { get; }
        public RelayCommand CloseCommand { get; }
        public RelayCommand OpenAboutWindowCommand { get; }
        public RelayCommand ReplaceColorsCommand { get; }

        public RelayCommand UndoCommand { get;}
        public RelayCommand RedoCommand { get; }

        public RelayCommand EditMasterThemeCommand { get; }
        public RelayCommand CreateThemeCommand { get; }
        public RelayCommand ApplyThemeToScriptCommand { get; }

        public RelayCommand EnableDropSoundsCommand { get; }
        public RelayCommand DisableDropSoundsCommand { get; }

        public RelayCommand AddTextColorThemeComponentCommand { get; }
        public RelayCommand AddBackgroundColorThemeComponentCommand { get; }
        public RelayCommand AddBorderColorThemeComponentCommand { get; }
        public RelayCommand AddFontSizeThemeComponentCommand { get; }
        public RelayCommand AddAlertSoundThemeComponentCommand { get; }
        public RelayCommand AddCustomSoundThemeComponentCommand { get; }
        public RelayCommand AddIconThemeComponentCommand { get; }
        public RelayCommand AddEffectColorThemeComponentCommand { get; }
        public RelayCommand DeleteThemeComponentCommand { get; }

        public RelayCommand AddBlockCommand { get; }
        public RelayCommand AddSectionCommand { get; }
        public RelayCommand DeleteBlockCommand { get; }
        public RelayCommand DisableBlockCommand { get; }
        public RelayCommand EnableBlockCommand { get; }
        public RelayCommand DisableSectionCommand { get; }
        public RelayCommand EnableSectionCommand { get; }
        public RelayCommand ExpandSectionCommand { get; }
        public RelayCommand CollapseSectionCommand { get; }

        public RelayCommand MoveBlockUpCommand { get; }
        public RelayCommand MoveBlockDownCommand { get; }
        public RelayCommand MoveBlockToTopCommand { get; }
        public RelayCommand MoveBlockToBottomCommand { get; }

        public RelayCommand ExpandAllBlocksCommand { get; }
        public RelayCommand CollapseAllBlocksCommand { get; }

        public RelayCommand ExpandAllSectionsCommand { get; }
        public RelayCommand CollapseAllSectionsCommand { get; }

        public RelayCommand<bool> ToggleShowAdvancedCommand { get; }
        public RelayCommand ClearFiltersCommand { get; }
        public RelayCommand ClearStylesCommand { get; }

        public ImageSource Icon { get; private set; }

        public IAvalonDockWorkspaceViewModel AvalonDockWorkspaceViewModel => _avalonDockWorkspaceViewModel;
        public ISettingsPageViewModel SettingsPageViewModel { get; }

        public IUpdateViewModel UpdateViewModel { get; }

        public string WindowTitle
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return "Filtration v" + fvi.ProductVersion;
            }
        }

        public WindowState WindowState
        {
            get => _windowState;
            set
            {
                _windowState = value;
                if(value != WindowState.Minimized)
                {
                    Settings.Default.WindowState = value;
                }
            }
        }

        public int WindowWidth
        {
            get => _windowWidth;
            set
            {
                _windowWidth = value;
                if (WindowState == WindowState.Normal)
                {
                    Settings.Default.WindowWidth = value;
                }
            }
        }

        public int WindowHeight
        {
            get => _windowHeight;
            set
            {
                _windowHeight = value;
                if (WindowState == WindowState.Normal)
                {
                    Settings.Default.WindowHeight = value;
                }
            }
        }

        public bool ShowLoadingBanner
        {
            get { return _showLoadingBanner; }
            private set
            {
                _showLoadingBanner = value;
                RaisePropertyChanged();
            }
        }

        public bool ActiveDocumentIsScript => _avalonDockWorkspaceViewModel.ActiveDocument != null && _avalonDockWorkspaceViewModel.ActiveDocument.IsScript;

        public bool ActiveDocumentIsTheme => _avalonDockWorkspaceViewModel.ActiveDocument!= null && _avalonDockWorkspaceViewModel.ActiveDocument.IsTheme;

        public bool ActiveScriptHasSelectedBlock => AvalonDockWorkspaceViewModel.ActiveScriptViewModel.LastSelectedBlockViewModel != null;

        public bool ActiveScriptHasSingleSelectedeBlock => AvalonDockWorkspaceViewModel.ActiveScriptViewModel.SelectedBlockViewModels.Count == 1;

        public bool ActiveScriptHasSelectedEnabledBlock => AvalonDockWorkspaceViewModel.ActiveScriptViewModel.HasSelectedEnabledBlock();

        public bool ActiveScriptHasSelectedDisabledBlock => AvalonDockWorkspaceViewModel.ActiveScriptViewModel.HasSelectedDisabledBlock();

        public bool ActiveScriptHasSelectedCommentBlock => AvalonDockWorkspaceViewModel.ActiveScriptViewModel.HasSelectedCommentBlock();

        public bool ActiveScriptCanModifySelectedBlocks => AvalonDockWorkspaceViewModel.ActiveScriptViewModel.CanModifySelectedBlocks();


        public bool ActiveThemeIsEditable => AvalonDockWorkspaceViewModel.ActiveThemeViewModel.IsMasterTheme;

        private bool ActiveDocumentIsEditable()
        {
            return AvalonDockWorkspaceViewModel.ActiveDocument is IEditableDocument;
        }

        public bool ShowAdvancedStatus => ActiveDocumentIsScript && _avalonDockWorkspaceViewModel.ActiveScriptViewModel.ShowAdvanced;

        public async Task OpenDroppedFilesAsync(List<string> filenames)
        {
            foreach (var filename in filenames)
            {
                var extension = Path.GetExtension(filename);
                if (extension == null) continue;

                switch (extension.ToUpperInvariant())
                {
                    case ".FILTER":
                    {
                        await _scriptLoadingService.LoadScriptAsync(filename);
                        break;
                    }
                    case ".FILTERTHEME":
                    {
                        await _scriptLoadingService.LoadThemeAsync(filename);
                        break;
                    }
                }
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

        private async Task OnOpenScriptCommand()
        {
            var filePath = ShowOpenScriptDialog();

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            await _scriptLoadingService.LoadScriptAsync(filePath);
        }

        private async Task OnOpenThemeCommandAsync()
        {
            var filePath = ShowOpenThemeDialog();
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            await _scriptLoadingService.LoadThemeAsync(filePath);
        }

        private async Task OnApplyThemeToScriptCommandAsync()
        {
            var filePath = ShowOpenThemeDialog();
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            Theme loadedTheme;

            try
            {
                loadedTheme = await _themeProvider.LoadThemeModelFromFile(filePath);
            }
            catch (IOException e)
            {
                if (Logger.IsErrorEnabled)
                {
                    Logger.Error(e);
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

        private string ShowOpenScriptDialog()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Filter Files (*.filter)|*.filter|All Files (*.*)|*.*",
                InitialDirectory = _itemFilterScriptDirectoryService.ItemFilterScriptDirectory
            };

            return openFileDialog.ShowDialog() != true ? string.Empty : openFileDialog.FileName;
        }

        private string ShowOpenThemeDialog()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Filter Theme Files (*.filtertheme)|*.filtertheme|All Files (*.*)|*.*",
                InitialDirectory = _itemFilterScriptDirectoryService.ItemFilterScriptDirectory
        };

            return openFileDialog.ShowDialog() != true ? string.Empty : openFileDialog.FileName;
        }

        private async Task OnSaveDocumentCommandAsync()
        {
            await ((IEditableDocument)_avalonDockWorkspaceViewModel.ActiveDocument).SaveAsync();
        }

        private async Task OnSaveAsCommandAsync()
        {
            await ((IEditableDocument)_avalonDockWorkspaceViewModel.ActiveDocument).SaveAsAsync();
        }

        private void OnReplaceColorsCommand()
        {
            _replaceColorsViewModel.Initialise(_avalonDockWorkspaceViewModel.ActiveScriptViewModel.Script);
            var replaceColorsWindow = new ReplaceColorsWindow {DataContext = _replaceColorsViewModel};
            replaceColorsWindow.ShowDialog();
        }

        private void OnCopyScriptCommand()
        {
            try
            {

                _clipboardService.SetClipboardText(
                    _itemFilterScriptTranslator.TranslateItemFilterScriptToString(
                        _avalonDockWorkspaceViewModel.ActiveScriptViewModel.Script));
            }
            catch
            {
                _messageBoxService.Show("Clipboard Error", "Failed to access the clipboard, copy command not completed.",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void OnUndoCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.Script.CommandManager.Undo();
        }

        private void OnRedoCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.Script.CommandManager.Redo();
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

        private void OnDisableSectionCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.DisableSectionCommand.Execute(null);
        }

        private void OnEnableSectionCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.EnableSectionCommand.Execute(null);
        }

        private void OnExpandSectionCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.ExpandSectionCommand.Execute(null);
        }

        private void OnCollapseSectionCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.CollapseSectionCommand.Execute(null);
        }

        private void OnExpandAllBlocksCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.ExpandAllBlocksCommand.Execute(null);
        }

        private void OnCollapseAllBlocksCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.CollapseAllBlocksCommand.Execute(null);
        }

        private void OnExpandAllSectionsCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.ExpandAllSectionsCommand.Execute(null);
        }

        private void OnCollapseAllSectionsCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.CollapseAllSectionsCommand.Execute(null);
        }

        private void OnToggleShowAdvancedCommand(bool showAdvanced)
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.ToggleShowAdvancedCommand.Execute(showAdvanced);
        }

        private void OnClearFiltersCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.ClearFilterCommand.Execute(null);
        }

        private void OnClearStylesCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.ClearStylesCommand.Execute(null);
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

        private void OnAddFontSizeThemeComponentCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveThemeViewModel.AddThemeComponentCommand.Execute(ThemeComponentType.FontSize);
        }

        private void OnAddAlertSoundThemeComponentCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveThemeViewModel.AddThemeComponentCommand.Execute(ThemeComponentType.AlertSound);
        }

        private void OnAddCustomSoundThemeComponentCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveThemeViewModel.AddThemeComponentCommand.Execute(ThemeComponentType.CustomSound);
        }

        private void OnAddIconThemeComponentCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveThemeViewModel.AddThemeComponentCommand.Execute(ThemeComponentType.Icon);
        }

        private void OnAddEffectColorThemeComponentCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveThemeViewModel.AddThemeComponentCommand.Execute(ThemeComponentType.Effect);
        }

        private void OnDeleteThemeComponentCommand()
        {
            _avalonDockWorkspaceViewModel.ActiveThemeViewModel.DeleteThemeComponentCommand.Execute(
                _avalonDockWorkspaceViewModel.ActiveThemeViewModel.SelectedThemeComponent);
        }

        private void OnEnableDropSoundsCommand()
        {
            var result = _messageBoxService.Show("Confirm",
                "Are you sure you wish to enable drop sounds on all selected blocks?",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                return;
            }

            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.EnableDropSoundsCommand.Execute(null);
        }

        private void OnDisableDropSoundsCommand()
        {
            var result = _messageBoxService.Show("Confirm",
                "Are you sure you wish to disable drop sounds on all selected blocks?",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                return;
            }

            _avalonDockWorkspaceViewModel.ActiveScriptViewModel.DisableDropSoundsCommand.Execute(null);
        }

        public async Task<bool> CloseAllDocumentsAsync()
        {
            Settings.Default.LastOpenScripts = string.Join("|", _avalonDockWorkspaceViewModel.OpenDocuments.OfType<IItemFilterScriptViewModel>().Select(sc => sc.Script.FilePath));
            var openDocuments = _avalonDockWorkspaceViewModel.OpenDocuments.OfType<IEditableDocument>().ToList();

            foreach (var document in openDocuments)
            {
                if (!_avalonDockWorkspaceViewModel.OpenDocuments.Contains(document))
                {
                    continue;
                }

                if (!await document.Close())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
