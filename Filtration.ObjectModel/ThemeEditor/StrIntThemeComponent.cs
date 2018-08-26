﻿using System;
using System.Windows.Media;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.ThemeEditor
{
    [Serializable]
    public class StrIntThemeComponent : ThemeComponent
    {
        private string _value;
        private int _secondValue;

        public StrIntThemeComponent(ThemeComponentType componentType, string componentName, string componentValue, int componentSecondValue)
        {
            if (componentName == null || componentValue == null)
            {
                throw new ArgumentException("Null parameters not allowed in StrIntThemeComponent constructor");
            }

            ComponentType = componentType;
            Value = componentValue;
            SecondValue = componentSecondValue;
            ComponentName = componentName;
        }

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

        public int SecondValue
        {
            get { return _secondValue; }
            set
            {
                _secondValue = value;
                OnPropertyChanged();
                _themeComponentUpdatedEventHandler?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
