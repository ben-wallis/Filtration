using System;
using System.Collections.ObjectModel;
using Filtration.ObjectModel.Enums;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;

namespace Filtration.ObjectModel.ThemeEditor
{
    [Serializable]
    public class StringThemeComponent : ThemeComponent
    {
        private string _value;
        public static ObservableCollection<string> _customSoundsAvailable;

        public StringThemeComponent(ThemeComponentType componentType, string componentName, string componentValue)
        {
            if (componentName == null || componentValue == null)
            {
                throw new ArgumentException("Null parameters not allowed in StringThemeComponent constructor");
            }

            ComponentType = componentType;
            Value = componentValue;
            ComponentName = componentName;

            if (_customSoundsAvailable == null || _customSoundsAvailable.Count < 1)
            {
                _customSoundsAvailable = new ObservableCollection<string> {
                    "1maybevaluable.mp3", "2currency.mp3", "3uniques.mp3", "4maps.mp3", "5highmaps.mp3",
                    "6veryvaluable.mp3", "7chancing.mp3", "12leveling.mp3", "placeholder.mp3"
                };
            }

            if (_customSoundsAvailable.IndexOf(Value) < 0)
            {
                _customSoundsAvailable.Add(Value);
            }
            
            CustomSoundFileDialogCommand = new RelayCommand(OnCustomSoundFileDialog);
        }

        public RelayCommand CustomSoundFileDialogCommand { get; set; }

        public ObservableCollection<string> CustomSoundsAvailable => _customSoundsAvailable;

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged();
                _themeComponentUpdatedEventHandler?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnCustomSoundFileDialog()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.DefaultExt = ".mp3";
            var poePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString() + @"\My Games\Path of Exile\";
            fileDialog.InitialDirectory = poePath;

            Nullable<bool> result = fileDialog.ShowDialog();
            if (result == true)
            {
                var fileName = fileDialog.FileName;
                if (fileName.StartsWith(poePath))
                {
                    fileName = fileName.Replace(poePath, "");
                }

                if (CustomSoundsAvailable.IndexOf(fileName) < 0)
                {
                    CustomSoundsAvailable.Add(fileName);
                    OnPropertyChanged(nameof(CustomSoundsAvailable));
                }
                Value = fileName;
            }
        }
    }
}
