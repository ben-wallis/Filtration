using System;
using System.Windows.Media;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.ThemeEditor
{
    [Serializable]
    public class ThemeComponent
    {
        public ThemeComponent()
        {
            
        }

        public ThemeComponent(ThemeComponentType componentType, string componentName, Color componentColor)
        {
            if (componentName == null || componentColor == null)
            {
                throw new ArgumentException("Null parameters not allowed in ThemeComponent constructor");
            }

            ComponentType = componentType;
            Color = componentColor;
            ComponentName = componentName;
        }

        public string ComponentName { get; set; }
        public ThemeComponentType ComponentType{ get; set; }
        public Color Color { get; set; }
    }
}
