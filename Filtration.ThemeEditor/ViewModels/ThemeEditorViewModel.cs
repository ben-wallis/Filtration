using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Filtration.Common.Services;
using Filtration.Common.ViewModels;
using Filtration.Interface;
using Filtration.ObjectModel;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.ThemeEditor;
using Filtration.ThemeEditor.Messages;
using Filtration.ThemeEditor.Providers;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NLog;

namespace Filtration.ThemeEditor.ViewModels
{
    public interface IThemeEditorViewModel : IEditableDocument
    {
        RelayCommand<ThemeComponentType> AddThemeComponentCommand { get; }
        RelayCommand<ThemeComponent> DeleteThemeComponentCommand { get; }
        RelayCommand CloseCommand { get; }

        void InitialiseForNewTheme(ThemeComponentCollection themeComponentCollection);
        void InitialiseForMasterTheme(ItemFilterScript script);
        bool IsMasterTheme { get; }
        ItemFilterScript IsMasterThemeForScript { get; }
        string Title { get; }
        string FilePath { get; set; }
        string Filename { get; }
        string Name { get; set; }
        ThemeComponentCollection Components { get; set; }
        ThemeComponent SelectedThemeComponent { get; }
    }

    public class ThemeEditorViewModel : PaneViewModel, IThemeEditorViewModel
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IThemeProvider _themeProvider;
        private readonly IMessageBoxService _messageBoxService;
        private bool _filenameIsFake;
        private string _filePath;
        private ThemeComponent _selectedThemeComponent;

        public ThemeEditorViewModel(IThemeProvider themeProvider,
                              IMessageBoxService messageBoxService)
        {
            _themeProvider = themeProvider;
            _messageBoxService = messageBoxService;

            AddThemeComponentCommand = new RelayCommand<ThemeComponentType>(OnAddThemeComponentCommand, t => IsMasterTheme);
            DeleteThemeComponentCommand = new RelayCommand<ThemeComponent>(OnDeleteThemeComponentCommand,
                t => IsMasterTheme && SelectedThemeComponent != null);
            CloseCommand = new RelayCommand(OnCloseCommand);

            var icon = new BitmapImage();
            icon.BeginInit();
            icon.UriSource = new Uri("pack://application:,,,/Filtration;component/Resources/Icons/Theme.ico");
            icon.EndInit();
            IconSource = icon;
            
        }

        public RelayCommand<ThemeComponentType> AddThemeComponentCommand { get; private set; }
        public RelayCommand<ThemeComponent> DeleteThemeComponentCommand { get; private set; }
        public RelayCommand CloseCommand { get; private set; }

        public bool IsMasterTheme
        {
            get { return Components.IsMasterCollection; }
        }

        public ItemFilterScript IsMasterThemeForScript { get; private set; }

        public void InitialiseForNewTheme(ThemeComponentCollection themeComponentCollection)
        {
            Components = themeComponentCollection;
            _filenameIsFake = true;
        }

        public void InitialiseForMasterTheme(ItemFilterScript script)
        {
            Components = script.ThemeComponents;
            IsMasterThemeForScript = script;
            _filenameIsFake = true;

        }

        public bool IsScript { get { return false; } }
        public bool IsTheme { get { return true; } }
        public bool IsDirty { get; private set; }

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                Title = Filename;
            }
        }

        public string Filename
        {
            get { return _filenameIsFake ? FilePath : Path.GetFileName(FilePath); }
        }

        public string Name { get; set; }

        public ThemeComponentCollection Components { get; set; }

        public ThemeComponent SelectedThemeComponent
        {
            get { return _selectedThemeComponent; }
            set
            {
                _selectedThemeComponent = value;
                RaisePropertyChanged();
            }
        }

        public void Save()
        {
            if (IsMasterTheme) return;

            if (_filenameIsFake)
            {
                SaveAs();
                return;
            }

            try
            {
                _themeProvider.SaveTheme(this, FilePath);
                //RemoveDirtyFlag();
            }
            catch (Exception e)
            {
                if (_logger.IsErrorEnabled)
                {
                    _logger.Error(e);
                }

                _messageBoxService.Show("Save Error", "Error saving filter theme - " + e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SaveAs()
        {
            if (IsMasterTheme) return;

            var saveDialog = new SaveFileDialog
            {
                DefaultExt = ".filter",
                Filter = @"Filter Theme Files (*.filtertheme)|*.filtertheme|All Files (*.*)|*.*"
            };

            var result = saveDialog.ShowDialog();

            if (result != DialogResult.OK) return;

            var previousFilePath = FilePath;
            try
            {
                FilePath = saveDialog.FileName;
                _themeProvider.SaveTheme(this, FilePath);
                _filenameIsFake = false;
                Title = Filename;
                //RemoveDirtyFlag();
            }
            catch (Exception e)
            {
                if (_logger.IsErrorEnabled)
                {
                    _logger.Error(e);
                }

                _messageBoxService.Show("Save Error", "Error saving theme file - " + e.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
                FilePath = previousFilePath;
            }
        }

        public void OnCloseCommand()
        {
            Close();
        }

        public void Close()
        {
            Messenger.Default.Send(new ThemeClosedMessage { ClosedViewModel = this });
        }
        
        private void OnAddThemeComponentCommand(ThemeComponentType themeComponentType)
        {
            Components.Add(new ThemeComponent(themeComponentType, "Untitled Component",
                new Color {A = 255, R = 255, G = 255, B = 255}));
        }

        private void OnDeleteThemeComponentCommand(ThemeComponent themeComponent)
        {
            if (themeComponent == null) return;

            themeComponent.TerminateComponent();
            Components.Remove(themeComponent);
        }
    }
}
