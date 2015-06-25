using System;
using System.Windows.Media;

namespace Filtration.ThemeEditor.Models
{
    public class ThemeComponent
    {
        public ThemeComponent(Type targetType, string componentName, Color componentColor)
        {
            if (targetType == null || componentName == null || componentColor == null)
            {
                throw new ArgumentException("Null parameters not allowed in ThemeComponent constructor");
            }

            TargetType = targetType;
            Color = componentColor;
            ComponentName = componentName;
        }

        public string ComponentName { get; set; }
        public Type TargetType { get; private set; }
        public Color Color { get; set; }
    }
}
