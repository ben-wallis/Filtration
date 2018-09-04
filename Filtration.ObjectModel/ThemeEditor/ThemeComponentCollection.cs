using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.ThemeEditor
{
    public class ThemeComponentCollection : ObservableCollection<ThemeComponent>
    {
        public bool IsMasterCollection { get; set; }

        public ThemeComponent AddComponent(ThemeComponentType componentType, string componentName, Color componentColor)
        {
            if (ComponentExists(componentType, componentName))
            {
                return Items.FirstOrDefault(t => t.ComponentName == componentName && t.ComponentType == componentType);
            }

            var component = new ColorThemeComponent(componentType, componentName, componentColor);
            Items.Add(component);

            return component;
        }

        public ThemeComponent AddComponent(ThemeComponentType componentType, string componentName, int componentValue)
        {
            if (ComponentExists(componentType, componentName))
            {
                return Items.FirstOrDefault(t => t.ComponentName == componentName && t.ComponentType == componentType);
            }

            var component = new IntegerThemeComponent(componentType, componentName, componentValue);
            Items.Add(component);

            return component;
        }

        public ThemeComponent AddComponent(ThemeComponentType componentType, string componentName, string componentValue, int componentSecondValue)
        {
            if (ComponentExists(componentType, componentName))
            {
                return Items.FirstOrDefault(t => t.ComponentName == componentName && t.ComponentType == componentType);
            }

            var component = new StrIntThemeComponent(componentType, componentName, componentValue, componentSecondValue);
            Items.Add(component);

            return component;
        }

        public ThemeComponent AddComponent(ThemeComponentType componentType, string componentName, string componentValue)
        {
            if (ComponentExists(componentType, componentName))
            {
                return Items.FirstOrDefault(t => t.ComponentName == componentName && t.ComponentType == componentType);
            }

            var component = new StringThemeComponent(componentType, componentName, componentValue);
            Items.Add(component);

            return component;
        }

        public ThemeComponent AddComponent(ThemeComponentType componentType, string componentName, IconSize componentIconSize, IconColor componentIconColor, IconShape componentIconShape)
        {
            if (ComponentExists(componentType, componentName))
            {
                return Items.FirstOrDefault(t => t.ComponentName == componentName && t.ComponentType == componentType);
            }

            var component = new IconThemeComponent(componentType, componentName, componentIconSize, componentIconColor, componentIconShape);
            Items.Add(component);

            return component;
        }

        public ThemeComponent AddComponent(ThemeComponentType componentType, string componentName, EffectColor componentEffectColor, bool componentTemporary)
        {
            if (ComponentExists(componentType, componentName))
            {
                return Items.FirstOrDefault(t => t.ComponentName == componentName && t.ComponentType == componentType);
            }

            var component = new EffectColorThemeComponent(componentType, componentName, componentEffectColor, componentTemporary);
            Items.Add(component);

            return component;
        }

        private bool ComponentExists(ThemeComponentType componentType, string componentName)
        {
            var componentCount =
                Items.Count(c => c.ComponentName == componentName && c.ComponentType == componentType);
            return componentCount > 0;
        }

        public bool ComponentExists(ThemeComponent themeComponent)
        {
            return ComponentExists(themeComponent.ComponentType, themeComponent.ComponentName);
        }
    }
}
