using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.ThemeEditor;

namespace Filtration.Translators
{
    internal interface IThemeComponentListBuilder
    {
        void Initialise();
        ThemeComponent AddComponent(ThemeComponentType componentType, string componentName, Color componentColor);
        List<ThemeComponent> GetComponents();
        void Cleanup();
    }

    internal class ThemeComponentListBuilder : IThemeComponentListBuilder
    {
        private List<ThemeComponent> _themeComponents;

        public ThemeComponentListBuilder()
        {
            _themeComponents = new List<ThemeComponent>();
        }

        public void Initialise()
        {
            _themeComponents = new List<ThemeComponent>();
        }

        public void Cleanup()
        {
            _themeComponents = null;
        }

        public ThemeComponent AddComponent(ThemeComponentType componentType, string componentName, Color componentColor)
        {
            if (ComponentExists(componentType, componentName))
            {
                return _themeComponents.FirstOrDefault(t => t.ComponentName == componentName && t.ComponentType == componentType);
            }

            var component = new ThemeComponent(componentType, componentName, componentColor);
            _themeComponents.Add(component);

            return component;
        }

        public List<ThemeComponent> GetComponents()
        {
            return _themeComponents;
        }

        private bool ComponentExists(ThemeComponentType componentType, string componentName)
        {
            return _themeComponents.Exists(c => c.ComponentName == componentName && c.ComponentType == componentType);
        }
    }
}
