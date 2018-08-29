using System;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.ThemeEditor
{
    [Serializable]
    public class EffectColorThemeComponent : ThemeComponent
    {
        private EffectColor _effectColor;
        private bool _temporary;

        public EffectColorThemeComponent(ThemeComponentType componentType, string componentName, EffectColor componentEffectColor, bool componentTemporary)
        {
            if (componentName == null)
            {
                throw new ArgumentException("Null parameters not allowed in EffectColorThemeComponent constructor");
            }

            ComponentType = componentType;
            ComponentName = componentName;
            EffectColor = componentEffectColor;
            Temporary = componentTemporary;
        }

        public EffectColor EffectColor
        {
            get { return _effectColor; }
            set
            {
                _effectColor = value;
                OnPropertyChanged();
                _themeComponentUpdatedEventHandler?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool Temporary
        {
            get { return _temporary; }
            set
            {
                _temporary = value;
                OnPropertyChanged();
                _themeComponentUpdatedEventHandler?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
