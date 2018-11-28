using System;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.ThemeEditor
{
    [Serializable]
    public class IconThemeComponent : ThemeComponent
    {
        private IconSize _iconSize;
        private IconColor _iconColor;
        private IconShape _iconShape;

        private IconThemeComponent()
        {
        }

        public IconThemeComponent(ThemeComponentType componentType, string componentName, IconSize componentIconSize, IconColor componentIconColor, IconShape componentIconShape)
        {
            if (componentName == null)
            {
                throw new ArgumentException("Null parameters not allowed in IconThemeComponent constructor");
            }

            ComponentType = componentType;
            ComponentName = componentName;
            IconSize = componentIconSize;
            IconColor = componentIconColor;
            IconShape = componentIconShape;
        }

        public IconSize IconSize
        {
            get => _iconSize;
            set
            {
                _iconSize = value;
                OnPropertyChanged();
                _themeComponentUpdatedEventHandler?.Invoke(this, EventArgs.Empty);
            }
        }

        public IconColor IconColor
        {
            get => _iconColor;
            set
            {
                _iconColor = value;
                OnPropertyChanged();
                _themeComponentUpdatedEventHandler?.Invoke(this, EventArgs.Empty);
            }
        }

        public IconShape IconShape
        {
            get => _iconShape;
            set
            {
                _iconShape = value;
                OnPropertyChanged();
                _themeComponentUpdatedEventHandler?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
