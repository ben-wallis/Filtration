using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Filtration.ThemeEditor.Models
{
    public class Theme
    {
        private readonly List<ThemeComponent> _components; 

        public Theme()
        {
            _components = new List<ThemeComponent>();
        }

        public string Name { get; set; }

        public IEnumerable<ThemeComponent> Components
        {
            get { return _components; }
        }

        public bool ComponentExists(Type targetType, string componentName)
        {
            return _components.Exists(c => c.ComponentName == componentName && c.TargetType == targetType);
        }

        public void AddComponent(Type targetType, string componentName, Color componentColor)
        {
            _components.Add(new ThemeComponent(targetType, componentName, componentColor));
        }
    }
}
