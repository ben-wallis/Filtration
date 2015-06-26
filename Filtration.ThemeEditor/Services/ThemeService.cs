using System;
using System.Linq;
using System.Windows;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.ThemeEditor;

namespace Filtration.ThemeEditor.Services
{
    public interface IThemeService
    {
        void ApplyThemeToScript(Theme theme, ItemFilterScript script);
    }

    public class ThemeService : IThemeService
    {
        public void ApplyThemeToScript(Theme theme, ItemFilterScript script)
        {
            var mismatchedComponents = false;
            foreach (var component in theme.Components)
            {
                var componentMatched = false;
                Type targetType = null;
                switch (component.ComponentType)
                {
                    case ThemeComponentType.BackgroundColor:
                        targetType = typeof (BackgroundColorBlockItem);
                        break;
                    case ThemeComponentType.TextColor:
                        targetType = typeof (TextColorBlockItem);
                        break;
                    case ThemeComponentType.BorderColor:
                        targetType = typeof (BorderColorBlockItem);
                        break;
                }

                foreach (var block in script.ItemFilterBlocks)
                {
                    foreach (var blockItem in block.BlockItems.Where(i => i.GetType() == targetType))
                    {
                        var colorBlockItem = (ColorBlockItem) blockItem;
                        if (colorBlockItem.ThemeComponent != null &&
                            colorBlockItem.ThemeComponent.ComponentName == component.ComponentName)
                        {
                            colorBlockItem.Color = component.Color;
                            componentMatched = true;
                        }
                    }   
                }

                if (!componentMatched)
                {
                    mismatchedComponents = true;
                }
            }

            if (mismatchedComponents)
            {
                MessageBox.Show(
                    "Not all theme components had matches - are you sure this theme is designed for this script?",
                    "Possible Theme Mismatch", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}
