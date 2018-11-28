using System;
using System.Windows.Media;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.ThemeEditor
{
    [Serializable]
    public class ColorThemeComponent : ThemeComponent
    {
        private Color _color;

        private ColorThemeComponent()
        {
        }

        public ColorThemeComponent(ThemeComponentType componentType, string componentName, Color componentColor)
        {
            if (componentName == null || componentColor == null)
            {
                throw new ArgumentException("Null parameters not allowed in ColorThemeComponent constructor");
            }

            ComponentType = componentType;
            Color = componentColor;
            ComponentName = componentName;
        }

        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged();
                _themeComponentUpdatedEventHandler?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
