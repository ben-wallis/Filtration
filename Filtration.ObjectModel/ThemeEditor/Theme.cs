using System;
using System.Collections.Generic;
using System.Windows.Media;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.ThemeEditor
{
    [Serializable]
    public class Theme
    {
        private readonly List<ThemeComponent> _components; 

        public Theme()
        {
            _components = new List<ThemeComponent>();
        }

        public string Name { get; set; }
        public string FilePath { get; set; }

        public List<ThemeComponent> Components
        {
            get { return _components; }
        }

        public bool ComponentExists(ThemeComponentType componentType, string componentName)
        {
            return _components.Exists(c => c.ComponentName == componentName && c.ComponentType == componentType);
        }

        public void AddComponent(ThemeComponentType componentType, string componentName, Color componentColor)
        {
            _components.Add(new ThemeComponent(componentType, componentName, componentColor));
        }
    }
}
