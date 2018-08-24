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

            ThemeComponent component = null;
            switch(componentType)
            {
                case ThemeComponentType.BackgroundColor:
                case ThemeComponentType.BorderColor:
                case ThemeComponentType.TextColor:
                    component = new ColorThemeComponent(componentType, componentName, componentColor);
                    break;
            }
            Items.Add(component);

            return component;
        }

        private bool ComponentExists(ThemeComponentType componentType, string componentName)
        {
            var componentCount =
                Items.Count(c => c.ComponentName == componentName && c.ComponentType == componentType);
            return componentCount > 0;
        }
    }
}
