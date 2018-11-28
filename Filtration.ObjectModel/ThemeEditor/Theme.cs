using System;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.ThemeEditor
{
    [Serializable]
    [XmlInclude(typeof(ColorThemeComponent))]
    [XmlInclude(typeof(EffectColorThemeComponent))]
    [XmlInclude(typeof(IconThemeComponent))]
    [XmlInclude(typeof(IntegerThemeComponent))]
    [XmlInclude(typeof(StringThemeComponent))]
    [XmlInclude(typeof(StrIntThemeComponent))]
    public class Theme
    {
        public Theme()
        {
            Components = new ThemeComponentCollection { IsMasterCollection = false};
        }

        public string Name { get; set; }

        [XmlIgnore]
        public string FilePath { get; set; }

        public ThemeComponentCollection Components { get; set; }

        public bool ComponentExists(ThemeComponentType componentType, string componentName)
        {
            var componentCount =
                Components.Count(c => c.ComponentName == componentName && c.ComponentType == componentType);
            return componentCount > 0;
        }

        public void AddComponent(ThemeComponentType componentType, string componentName, Color componentColor)
        {
            Components.Add(new ColorThemeComponent(componentType, componentName, componentColor));
        }

        public void AddComponent(ThemeComponentType componentType, string componentName, int componentValue)
        {
            Components.Add(new IntegerThemeComponent(componentType, componentName, componentValue));
        }

        public void AddComponent(ThemeComponentType componentType, string componentName, string componentValue, int componentSecondValue)
        {
            Components.Add(new StrIntThemeComponent(componentType, componentName, componentValue, componentSecondValue));
        }
    }
}
