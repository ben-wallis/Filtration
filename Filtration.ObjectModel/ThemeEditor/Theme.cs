using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.ThemeEditor
{
    [Serializable]
    public class Theme
    {
        private readonly ThemeComponentCollection _components; 

        public Theme()
        {
            _components = new ThemeComponentCollection { IsMasterCollection = false};
        }

        public string Name { get; set; }
        public string FilePath { get; set; }

        public ThemeComponentCollection Components
        {
            get { return _components; }
        }

        public bool ComponentExists(ThemeComponentType componentType, string componentName)
        {
            var componentCount =
                _components.Count(c => c.ComponentName == componentName && c.ComponentType == componentType);
            return componentCount > 0;
        }

        public void AddComponent(ThemeComponentType componentType, string componentName, Color componentColor)
        {
            _components.Add(new ThemeComponent(componentType, componentName, componentColor));
        }
    }
}
