using System;
using System.Windows.Media;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.ThemeEditor
{
    [Serializable]
    public class IntegerThemeComponent : ThemeComponent
    {
        private int _value;

        private IntegerThemeComponent()
        {
        }

        public IntegerThemeComponent(ThemeComponentType componentType, string componentName, int componentValue)
        {
            if (componentName == null)
            {
                throw new ArgumentException("Null parameters not allowed in IntegerThemeComponent constructor");
            }

            ComponentType = componentType;
            Value = componentValue;
            ComponentName = componentName;
        }

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
                _themeComponentUpdatedEventHandler?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
