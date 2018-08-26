﻿using System;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
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

        [XmlIgnore]
        public string FilePath { get; set; }

        public ThemeComponentCollection Components => _components;

        public bool ComponentExists(ThemeComponentType componentType, string componentName)
        {
            var componentCount =
                _components.Count(c => c.ComponentName == componentName && c.ComponentType == componentType);
            return componentCount > 0;
        }

        public void AddComponent(ThemeComponentType componentType, string componentName, Color componentColor)
        {
            _components.Add(new ColorThemeComponent(componentType, componentName, componentColor));
        }

        public void AddComponent(ThemeComponentType componentType, string componentName, int componentValue)
        {
            _components.Add(new IntegerThemeComponent(componentType, componentName, componentValue));
        }

        public void AddComponent(ThemeComponentType componentType, string componentName, string componentValue, int componentSecondValue)
        {
            _components.Add(new StrIntThemeComponent(componentType, componentName, componentValue, componentSecondValue));
        }
    }
}
