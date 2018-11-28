using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;
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

        private StringThemeComponent()
        {
        }

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

                _customSoundsAvailable = new ObservableCollection<string>();

                var poeFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString() + @"\My Games\Path of Exile\";
                if(System.IO.Directory.Exists(poeFolderPath))
                {
                    var poeFolderFiles = System.IO.Directory.GetFiles(poeFolderPath).Where(
                        s => s.EndsWith(".mp3")
                        || s.EndsWith(".wav")
                        || s.EndsWith(".wma")
                        || s.EndsWith(".3gp")
                        || s.EndsWith(".aag")
                        || s.EndsWith(".m4a")
                        || s.EndsWith(".ogg")
                    ).OrderBy(f => f);

                    foreach (var file in poeFolderFiles)
                    {
                        _customSoundsAvailable.Add(file.Replace(poeFolderPath, ""));
                    }
                }
            }

            if(string.IsNullOrWhiteSpace(Value))
            {
                Value = _customSoundsAvailable.Count > 0 ? _customSoundsAvailable[0] : "";
            }
            else if (_customSoundsAvailable.IndexOf(Value) < 0)
            {
                _customSoundsAvailable.Add(Value);
            }
            
            CustomSoundFileDialogCommand = new RelayCommand(OnCustomSoundFileDialog);
        }

        [XmlIgnore]
        public RelayCommand CustomSoundFileDialogCommand { get; set; }

        public ObservableCollection<string> CustomSoundsAvailable => _customSoundsAvailable;

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
                _themeComponentUpdatedEventHandler?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnCustomSoundFileDialog()
        {
            OpenFileDialog fileDialog = new OpenFileDialog {DefaultExt = ".mp3"};
            var poePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\Path of Exile\";
            fileDialog.InitialDirectory = poePath;

            bool? result = fileDialog.ShowDialog();
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
