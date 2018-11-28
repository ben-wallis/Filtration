using System;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.ThemeEditor
{
    [Serializable]
    public class EffectColorThemeComponent : ThemeComponent
    {
        private EffectColor _effectColor;
        private bool _temporary;

        private EffectColorThemeComponent()
        {
        }

        public EffectColorThemeComponent(ThemeComponentType componentType, string componentName, EffectColor componentEffectColor, bool componentTemporary)
        {
            ComponentType = componentType;
            ComponentName = componentName ?? throw new ArgumentException("Null parameters not allowed in EffectColorThemeComponent constructor");
            EffectColor = componentEffectColor;
            Temporary = componentTemporary;
        }

        public EffectColor EffectColor
        {
            get => _effectColor;
            set
            {
                _effectColor = value;
                OnPropertyChanged();
                _themeComponentUpdatedEventHandler?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool Temporary
        {
            get => _temporary;
            set
            {
                _temporary = value;
                OnPropertyChanged();
                _themeComponentUpdatedEventHandler?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
