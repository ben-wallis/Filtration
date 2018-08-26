using System;
using System.Collections.Generic;
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
        void ApplyThemeToScript(Theme theme, IItemFilterScript script);
    }

    public class ThemeService : IThemeService
    {
        private readonly IMessageBoxService _messageBoxService;

        public ThemeService(IMessageBoxService messageBoxService)
        {
            _messageBoxService = messageBoxService;
        }

        public void ApplyThemeToScript(Theme theme, IItemFilterScript script)
        {
            var mismatchedComponents = false;
            foreach (var component in theme.Components)
            {
                var blocks = script.ItemFilterBlocks.OfType<ItemFilterBlock>();
                switch (component.ComponentType)
                {
                    case ThemeComponentType.BackgroundColor:
                        mismatchedComponents = ApplyColorTheme(blocks, typeof(BackgroundColorBlockItem), component);
                        break;
                    case ThemeComponentType.TextColor:
                        mismatchedComponents = ApplyColorTheme(blocks, typeof(TextColorBlockItem), component);
                        break;
                    case ThemeComponentType.BorderColor:
                        mismatchedComponents = ApplyColorTheme(blocks, typeof(BorderColorBlockItem), component);
                        break;
                    case ThemeComponentType.FontSize:
                        mismatchedComponents = ApplyIntegerTheme(blocks, typeof(FontSizeBlockItem), component);
                        break;
                    case ThemeComponentType.AlertSound:
                        mismatchedComponents = ApplyStrIntTheme(blocks, typeof(SoundBlockItem), component);
                        mismatchedComponents = ApplyStrIntTheme(blocks, typeof(PositionalSoundBlockItem), component);
                        break;
                }
            }

            if (mismatchedComponents)
            {
                _messageBoxService.Show("Possible Theme Mismatch",
                    "Not all theme components had matches - are you sure this theme is designed for this script?",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private bool ApplyColorTheme(IEnumerable<ItemFilterBlock> blocks, Type type, ThemeComponent component)
        {
            var componentMatched = false;
            foreach (var block in blocks)
            {
                foreach (var blockItem in block.BlockItems.Where(i => i.GetType() == type))
                {
                    var colorBlockItem = (ColorBlockItem)blockItem;
                    if (colorBlockItem.ThemeComponent != null &&
                        colorBlockItem.ThemeComponent.ComponentName == component.ComponentName)
                    {
                        colorBlockItem.Color = ((ColorThemeComponent)component).Color;
                        componentMatched = true;
                    }
                }
            }

            return !componentMatched;
        }

        private bool ApplyIntegerTheme(IEnumerable<ItemFilterBlock> blocks, Type type, ThemeComponent component)
        {
            var componentMatched = false;
            foreach (var block in blocks)
            {
                foreach (var blockItem in block.BlockItems.Where(i => i.GetType() == type))
                {
                    var colorBlockItem = (IntegerBlockItem)blockItem;
                    if (colorBlockItem.ThemeComponent != null &&
                        colorBlockItem.ThemeComponent.ComponentName == component.ComponentName)
                    {
                        colorBlockItem.Value = ((IntegerThemeComponent)component).Value;
                        componentMatched = true;
                    }
                }
            }

            return !componentMatched;
        }

        private bool ApplyStrIntTheme(IEnumerable<ItemFilterBlock> blocks, Type type, ThemeComponent component)
        {
            var componentMatched = false;
            foreach (var block in blocks)
            {
                foreach (var blockItem in block.BlockItems.Where(i => i.GetType() == type))
                {
                    var colorBlockItem = (StrIntBlockItem)blockItem;
                    if (colorBlockItem.ThemeComponent != null &&
                        colorBlockItem.ThemeComponent.ComponentName == component.ComponentName)
                    {
                        colorBlockItem.Value = ((StrIntThemeComponent)component).Value;
                        colorBlockItem.SecondValue = ((StrIntThemeComponent)component).SecondValue;
                        componentMatched = true;
                    }
                }
            }

            return !componentMatched;
        }
    }
}
