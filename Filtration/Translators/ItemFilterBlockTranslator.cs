using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Extensions;
using Filtration.Utilities;

namespace Filtration.Translators
{
    internal interface IItemFilterBlockTranslator
    {
        ItemFilterBlock TranslateStringToItemFilterBlock(string inputString);
        string TranslateItemFilterBlockToString(ItemFilterBlock block);
        void ReplaceColorBlockItemsFromString(ObservableCollection<IItemFilterBlockItem> blockItems, string inputString);
    }

    internal class ItemFilterBlockTranslator : IItemFilterBlockTranslator
    {
        private readonly IBlockGroupHierarchyBuilder _blockGroupHierarchyBuilder;
        private readonly IThemeComponentListBuilder _themeComponentListBuilder;
        private const string Indent = "    ";
        private readonly string _newLine = Environment.NewLine + Indent;

        public ItemFilterBlockTranslator(IBlockGroupHierarchyBuilder blockGroupHierarchyBuilder, IThemeComponentListBuilder themeComponentListBuilder)
        {
            _blockGroupHierarchyBuilder = blockGroupHierarchyBuilder;
            _themeComponentListBuilder = themeComponentListBuilder;
        }

        // This method converts a string into a ItemFilterBlock. This is used for pasting ItemFilterBlocks 
        // and reading ItemFilterScripts from a file.
        public ItemFilterBlock TranslateStringToItemFilterBlock(string inputString)
        {
            var block = new ItemFilterBlock();
            var showHideFound = false;
            foreach (var line in new LineReader(() => new StringReader(inputString)))
            {

                if (line.StartsWith(@"# Section:"))
                {
                    var section = new ItemFilterSection
                    {
                        Description = line.Substring(line.IndexOf(":", StringComparison.Ordinal) + 1).Trim()
                    };
                    return section;
                }

                if (line.StartsWith(@"#") && !showHideFound)
                {
                    block.Description = line.TrimStart('#').TrimStart(' ');
                    continue;
                }

                var adjustedLine = line.Replace("#", " # ");
                var trimmedLine = adjustedLine.TrimStart(' ');
                var spaceOrEndOfLinePos = trimmedLine.IndexOf(" ", StringComparison.Ordinal) > 0 ? trimmedLine.IndexOf(" ", StringComparison.Ordinal) : trimmedLine.Length;

                var lineOption = trimmedLine.Substring(0, spaceOrEndOfLinePos);
                switch (lineOption)
                {
                    case "Show":
                        showHideFound = true;
                        block.Action = BlockAction.Show;
                        AddBlockGroupToBlock(block, trimmedLine);
                        break;
                    case "Hide":
                        showHideFound = true;
                        block.Action = BlockAction.Hide;
                        AddBlockGroupToBlock(block, trimmedLine);
                        break;
                    case "ItemLevel":
                    {
                        AddNumericFilterPredicateItemToBlockItems<ItemLevelBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "DropLevel":
                    {
                        AddNumericFilterPredicateItemToBlockItems<DropLevelBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "Quality":
                    {
                        AddNumericFilterPredicateItemToBlockItems<QualityBlockItem>(block,trimmedLine);
                        break;
                    }
                    case "Rarity":
                    {
                        var blockItemValue = new RarityBlockItem();
                        var result = Regex.Match(trimmedLine, @"^\w+\s+([><!=]{0,2})\s*(\w+)$");
                        if (result.Groups.Count == 3)
                        {
                            blockItemValue.FilterPredicate.PredicateOperator =
                                EnumHelper.GetEnumValueFromDescription<FilterPredicateOperator>(string.IsNullOrEmpty(result.Groups[1].Value) ? "=" : result.Groups[1].Value);
                            blockItemValue.FilterPredicate.PredicateOperand =
                                (int)(EnumHelper.GetEnumValueFromDescription<ItemRarity>(result.Groups[2].Value));
                        }
                        block.BlockItems.Add(blockItemValue);
                        break;
                    }
                    case "Class":
                    {
                        AddStringListItemToBlockItems<ClassBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "BaseType":
                    {
                        AddStringListItemToBlockItems<BaseTypeBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "Sockets":
                    {
                        AddNumericFilterPredicateItemToBlockItems<SocketsBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "LinkedSockets":
                    {
                        AddNumericFilterPredicateItemToBlockItems<LinkedSocketsBlockItem>(block,trimmedLine);
                        break;
                    }
                    case "Width":
                    {
                        AddNumericFilterPredicateItemToBlockItems<WidthBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "Height":
                    {
                        AddNumericFilterPredicateItemToBlockItems<HeightBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "SocketGroup":
                    {
                        AddStringListItemToBlockItems<SocketGroupBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "SetTextColor":
                    {
                        // Only ever use the last SetTextColor item encountered as multiples aren't valid.
                        RemoveExistingBlockItemsOfType<TextColorBlockItem>(block);

                        AddColorItemToBlockItems<TextColorBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "SetBackgroundColor":
                    {
                        // Only ever use the last SetBackgroundColor item encountered as multiples aren't valid.
                        RemoveExistingBlockItemsOfType<BackgroundColorBlockItem>(block);

                        AddColorItemToBlockItems<BackgroundColorBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "SetBorderColor":
                    {
                        // Only ever use the last SetBorderColor item encountered as multiples aren't valid.
                        RemoveExistingBlockItemsOfType<BorderColorBlockItem>(block);

                        AddColorItemToBlockItems<BorderColorBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "SetFontSize":
                    {
                        // Only ever use the last SetFontSize item encountered as multiples aren't valid.
                        RemoveExistingBlockItemsOfType<FontSizeBlockItem>(block);

                        var match = Regex.Match(trimmedLine, @"\s+(\d+)");
                        if (match.Success)
                        {
                            var blockItemValue = new FontSizeBlockItem(Convert.ToInt16(match.Value));
                            block.BlockItems.Add(blockItemValue);
                        }
                        break;
                    }
                    case "PlayAlertSound":
                    {
                        // Only ever use the last PlayAlertSound item encountered as multiples aren't valid.
                        RemoveExistingBlockItemsOfType<SoundBlockItem>(block);

                        var matches = Regex.Matches(trimmedLine, @"\s+(\d+)");
                        switch (matches.Count)
                        {
                            case 1:
                                if (matches[0].Success)
                                {
                                    var blockItemValue = new SoundBlockItem
                                    {
                                        Value = Convert.ToInt16(matches[0].Value),
                                        SecondValue = 79
                                    };
                                    block.BlockItems.Add(blockItemValue);
                                }
                                break;
                            case 2:
                                if (matches[0].Success && matches[1].Success)
                                {
                                    var blockItemValue = new SoundBlockItem
                                    {
                                        Value = Convert.ToInt16(matches[0].Value),
                                        SecondValue = Convert.ToInt16(matches[1].Value)
                                    };
                                    block.BlockItems.Add(blockItemValue);
                                }
                                break;
                        }
                        break;
                    }
                }
            }

            return block;
        }

        private static void RemoveExistingBlockItemsOfType<T>(ItemFilterBlock block)
        {
            var existingBlockItemCount = block.BlockItems.Count(b => b.GetType() == typeof(T));
            if (existingBlockItemCount > 0)
            {
                var existingBlockItem = block.BlockItems.First(b => b.GetType() == typeof(T));
                block.BlockItems.Remove(existingBlockItem);
            }
        }

        private static void AddNumericFilterPredicateItemToBlockItems<T>(ItemFilterBlock block, string inputString) where T : NumericFilterPredicateBlockItem
        {
            var blockItem = Activator.CreateInstance<T>();
            
            SetNumericFilterPredicateFromString(blockItem.FilterPredicate, inputString);
            block.BlockItems.Add(blockItem);
        }

        private static void SetNumericFilterPredicateFromString(NumericFilterPredicate predicate, string inputString)
        {
            var result = Regex.Match(inputString, @"^\w+\s+([><!=]{0,2})\s*(\d{0,3})$");
            if (result.Groups.Count != 3) return;

            predicate.PredicateOperator =
                EnumHelper.GetEnumValueFromDescription<FilterPredicateOperator>(string.IsNullOrEmpty(result.Groups[1].Value) ? "=" : result.Groups[1].Value);
            predicate.PredicateOperand = Convert.ToInt16(result.Groups[2].Value);
        }

        private static void AddStringListItemToBlockItems<T>(ItemFilterBlock block, string inputString) where T : StringListBlockItem
        {
            var blockItem = Activator.CreateInstance<T>();
            PopulateListFromString(blockItem.Items, inputString.Substring(inputString.IndexOf(" ", StringComparison.Ordinal) + 1).Trim());
            block.BlockItems.Add(blockItem);
        }

        private static void PopulateListFromString(ICollection<string> list, string inputString)
        {
            var result = Regex.Matches(inputString, @"[^\s""]+|""([^""]*)""");
            foreach (Match match in result)
            {
                list.Add(match.Groups[1].Success
                    ? match.Groups[1].Value
                    : match.Groups[0].Value);
            }
        }

        private void AddColorItemToBlockItems<T>(ItemFilterBlock block, string inputString) where T : ColorBlockItem
        {
            block.BlockItems.Add(GetColorBlockItemFromString<T>(inputString));
        }

        private T GetColorBlockItemFromString<T>(string inputString) where T: ColorBlockItem
        {
            var blockItem = Activator.CreateInstance<T>();
            var result = Regex.Matches(inputString, @"([\w\s]*)[#]?(.*)");

            blockItem.Color = GetColorFromString(result[0].Groups[1].Value);

            var componentName = result[0].Groups[2].Value.Trim();
            if (!string.IsNullOrEmpty(componentName))
            {
                ThemeComponentType componentType;
                if (typeof(T) == typeof(TextColorBlockItem))
                {
                    componentType = ThemeComponentType.TextColor;
                }
                else if (typeof(T) == typeof(BackgroundColorBlockItem))
                {
                    componentType = ThemeComponentType.BackgroundColor;
                }
                else if (typeof(T) == typeof(BorderColorBlockItem))
                {
                    componentType = ThemeComponentType.BorderColor;
                }
                else
                {
                    throw new Exception("Parsing error - unknown theme component type");
                }
                if (_themeComponentListBuilder.IsInitialised)
                {
                    blockItem.ThemeComponent = _themeComponentListBuilder.AddComponent(componentType, componentName,
                        blockItem.Color);
                }
            }

            return blockItem;
        }

        public void ReplaceColorBlockItemsFromString(ObservableCollection<IItemFilterBlockItem> blockItems, string inputString)
        {
            // Reverse iterate to remove existing IAudioVisualBlockItems
            for (var idx = blockItems.Count - 1; idx >= 0; idx--)
            {
                if (blockItems[idx] is IAudioVisualBlockItem)
                {
                    blockItems.RemoveAt(idx);
                }
            }

            foreach (var line in new LineReader(() => new StringReader(inputString)))
            {
                var matches = Regex.Match(line, @"(\w+)");
                
                switch (matches.Value)
                {
                    case "SetTextColor":
                    {
                        blockItems.Add(GetColorBlockItemFromString<TextColorBlockItem>(line));
                        break;
                    }
                    case "SetBackgroundColor":
                    {
                        blockItems.Add(GetColorBlockItemFromString<BackgroundColorBlockItem>(line));
                        break;
                    }
                    case "SetBorderColor":
                    {
                        blockItems.Add(GetColorBlockItemFromString<BorderColorBlockItem>(line));
                        break;
                    }
                    case "SetFontSize":
                    {
                        var match = Regex.Match(line, @"\s+(\d+)");
                        if (!match.Success) break;
                        blockItems.Add(new FontSizeBlockItem(Convert.ToInt16(match.Value)));
                        break;
                    }
                } 
            }
        }
        
        private void AddBlockGroupToBlock(ItemFilterBlock block, string inputString)
        {
            var blockGroupStart = inputString.IndexOf("#", StringComparison.Ordinal);
            if (blockGroupStart <= 0) return;

            var blockGroupText = inputString.Substring(blockGroupStart + 1);
            var blockGroups = blockGroupText.Split('-').ToList();
            if (blockGroups.Count(b => !string.IsNullOrEmpty(b.Trim())) > 0)
            {
                block.BlockGroup = _blockGroupHierarchyBuilder.IntegrateStringListIntoBlockGroupHierarchy(blockGroups);
                block.BlockGroup.IsChecked = block.Action == BlockAction.Show;
            }
        }

        private static Color GetColorFromString(string inputString)
        {
            var argbValues = Regex.Matches(inputString, @"\s+(\d+)");

            switch (argbValues.Count)
            {
                case 3:
                    return new Color
                    {
                        A = byte.MaxValue,
                        R = Convert.ToByte(argbValues[0].Value),
                        G = Convert.ToByte(argbValues[1].Value),
                        B = Convert.ToByte(argbValues[2].Value)
                    };
                case 4:
                    return new Color
                    {
                        R = Convert.ToByte(argbValues[0].Value),
                        G = Convert.ToByte(argbValues[1].Value),
                        B = Convert.ToByte(argbValues[2].Value),
                        A = Convert.ToByte(argbValues[3].Value)
                    };
            }
            return new Color();
        }

        // This method converts an ItemFilterBlock object into a string. This is used for copying a ItemFilterBlock
        // to the clipboard, and when saving a ItemFilterScript.
        public string TranslateItemFilterBlockToString(ItemFilterBlock block)
        {
            if (block.GetType() == typeof (ItemFilterSection))
            {
                return "# Section: " + block.Description;
            }

            var outputString = string.Empty;

            if (!string.IsNullOrEmpty(block.Description))
            {
                outputString += "# " + block.Description + Environment.NewLine;
            }

            outputString += block.Action.GetAttributeDescription();

            if (block.BlockGroup != null)
            {
                outputString += " # " + block.BlockGroup;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var blockItem in block.BlockItems.Where(b => b.GetType() != typeof(ActionBlockItem)).OrderBy(b => b.SortOrder))
            {
                if (blockItem.OutputText != string.Empty)
                {
                    outputString += _newLine + blockItem.OutputText;
                }
            }
            
            return outputString;
        }
    }
}
