using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Filtration.ThemeEditor.Models;

namespace Filtration.Translators
{
    internal interface IThemeComponentListBuilder
    {
        void Initialise();
        ThemeComponent AddComponent(Type targetType, string componentName, Color componentColor);
        List<ThemeComponent> GetComponents();
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
        
        public ThemeComponent AddComponent(Type targetType, string componentName, Color componentColor)
        {
            if (ComponentExists(targetType, componentName))
            {
                return _themeComponents.FirstOrDefault(t => t.ComponentName == componentName && t.TargetType == targetType);
            }

            var component = new ThemeComponent(targetType, componentName, componentColor);
            _themeComponents.Add(component);

            return component;
        }

        public List<ThemeComponent> GetComponents()
        {
            return _themeComponents;
        }

        private bool ComponentExists(Type targetType, string componentName)
        {
            return _themeComponents.Exists(c => c.ComponentName == componentName && c.TargetType == targetType);
        }
    }
}
