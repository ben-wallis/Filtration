using System;
using System.IO;
using System.Threading.Tasks;
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

        void InitialiseForNewTheme(ThemeComponentCollection themeComponentCollection);
        void InitialiseForMasterTheme(IItemFilterScript script);
        bool IsMasterTheme { get; }
        IItemFilterScript IsMasterThemeForScript { get; }
        string Title { get; }
        string FilePath { get; set; }
        string Filename { get; }
        string Name { get; set; }
        ThemeComponentCollection Components { get; set; }
        ThemeComponent SelectedThemeComponent { get; }
    }

    public class ThemeEditorViewModel : PaneViewModel, IThemeEditorViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
            CloseCommand = new RelayCommand(async () => await OnCloseCommand());

            var icon = new BitmapImage();
            icon.BeginInit();
            icon.UriSource = new Uri("pack://application:,,,/Filtration;component/Resources/Icons/Theme.ico");
            icon.EndInit();
            IconSource = icon;
            
        }

        public RelayCommand<ThemeComponentType> AddThemeComponentCommand { get; }
        public RelayCommand<ThemeComponent> DeleteThemeComponentCommand { get; }
        public RelayCommand CloseCommand { get; }

        public bool IsMasterTheme => Components.IsMasterCollection;

        public IItemFilterScript IsMasterThemeForScript { get; private set; }

        public void InitialiseForNewTheme(ThemeComponentCollection themeComponentCollection)
        {
            Components = themeComponentCollection;
            _filenameIsFake = true;
        }

        public void InitialiseForMasterTheme(IItemFilterScript script)
        {
            Components = script.ThemeComponents;
            IsMasterThemeForScript = script;
            _filenameIsFake = true;

        }

        public bool IsScript => false;
        public bool IsTheme => true;
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

        public string Filename => _filenameIsFake ? FilePath : Path.GetFileName(FilePath);

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

        public async Task SaveAsync()
        {
            if (IsMasterTheme) return;

            if (_filenameIsFake)
            {
                await SaveAsAsync();
                return;
            }

            try
            {
                await _themeProvider.SaveThemeAsync(this, FilePath);
                //RemoveDirtyFlag();
            }
            catch (Exception e)
            {
                if (Logger.IsErrorEnabled)
                {
                    Logger.Error(e);
                }

                _messageBoxService.Show("Save Error", "Error saving filter theme - " + e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task SaveAsAsync()
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
                await _themeProvider.SaveThemeAsync(this, FilePath);
                _filenameIsFake = false;
                Title = Filename;
                //RemoveDirtyFlag();
            }
            catch (Exception e)
            {
                if (Logger.IsErrorEnabled)
                {
                    Logger.Error(e);
                }

                _messageBoxService.Show("Save Error", "Error saving theme file - " + e.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
                FilePath = previousFilePath;
            }
        }

        public async Task OnCloseCommand()
        {
            await Close();
        }

#pragma warning disable 1998
        public async Task<bool> Close()
#pragma warning restore 1998
        {
           Messenger.Default.Send(new ThemeClosedMessage {ClosedViewModel = this});
            return true;
        }
        
        private void OnAddThemeComponentCommand(ThemeComponentType themeComponentType)
        {
            switch (themeComponentType)
            {
                case ThemeComponentType.BackgroundColor:
                case ThemeComponentType.BorderColor:
                case ThemeComponentType.TextColor:
                    Components.Add(new ColorThemeComponent(themeComponentType, "Untitled Component",
                        new Color { A = 240, R = 255, G = 255, B = 255 }));
                    break;
                case ThemeComponentType.FontSize:
                    Components.Add(new IntegerThemeComponent(themeComponentType, "Untitled Component", 35));
                    break;
                case ThemeComponentType.AlertSound:
                    Components.Add(new StrIntThemeComponent(themeComponentType, "Untitled Component", "1", 100));
                    break;
                case ThemeComponentType.CustomSound:
                    Components.Add(new StringThemeComponent(themeComponentType, "Untitled Component", ""));
                    break;
                case ThemeComponentType.Icon:
                    Components.Add(new IconThemeComponent(themeComponentType, "Untitled Component", IconSize.Largest, IconColor.Red, IconShape.Circle));
                    break;
                case ThemeComponentType.Effect:
                    Components.Add(new EffectColorThemeComponent(themeComponentType, "Untitled Component", EffectColor.Red, false));
                    break;
            }
        }

        private void OnDeleteThemeComponentCommand(ThemeComponent themeComponent)
        {
            if (themeComponent == null) return;

            themeComponent.TerminateComponent();
            Components.Remove(themeComponent);
        }
    }
}
