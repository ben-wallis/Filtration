using System;
using System.Linq;
using System.Windows;
using Filtration.Common.Services;
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
        private readonly IMessageBoxService _messageBoxService;

        public ThemeService(IMessageBoxService messageBoxService)
        {
            _messageBoxService = messageBoxService;
        }

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
                _messageBoxService.Show("Possible Theme Mismatch",
                    "Not all theme components had matches - are you sure this theme is designed for this script?",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}
