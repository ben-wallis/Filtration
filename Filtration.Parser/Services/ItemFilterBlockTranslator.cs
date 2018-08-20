﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Filtration.Common.Utilities;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Extensions;
using Filtration.ObjectModel.ThemeEditor;
using Filtration.Parser.Interface.Services;

namespace Filtration.Parser.Services
{
    internal class ItemFilterBlockTranslator : IItemFilterBlockTranslator
    {
        private readonly IBlockGroupHierarchyBuilder _blockGroupHierarchyBuilder;
        private const string _indent = "    ";
        private readonly string _newLine = Environment.NewLine + _indent;
        private readonly string _disabledNewLine = Environment.NewLine + "#" + _indent;
        private ThemeComponentCollection _masterComponentCollection;

        public ItemFilterBlockTranslator(IBlockGroupHierarchyBuilder blockGroupHierarchyBuilder)
        {
            _blockGroupHierarchyBuilder = blockGroupHierarchyBuilder;
        }

        // Converts a string into an ItemFilterCommentBlock maintaining newlines and spaces but removing # characters
        public IItemFilterCommentBlock TranslateStringToItemFilterCommentBlock(string inputString, IItemFilterScript parentItemFilterScript)
        {
            var itemFilterCommentBlock = new ItemFilterCommentBlock(parentItemFilterScript);

            foreach (var line in new LineReader(() => new StringReader(inputString)))
            {
                var trimmedLine = line.TrimStart(' ').TrimStart('#');
                itemFilterCommentBlock.Comment += trimmedLine + Environment.NewLine;
            }

            itemFilterCommentBlock.Comment = itemFilterCommentBlock.Comment.TrimEnd('\r', '\n');

            return itemFilterCommentBlock;
        }

        // This method converts a string into a ItemFilterBlock. This is used for pasting ItemFilterBlocks 
        // and reading ItemFilterScripts from a file.
        public IItemFilterBlock TranslateStringToItemFilterBlock(string inputString, IItemFilterScript parentItemFilterScript, bool initialiseBlockGroupHierarchyBuilder = false)
        {
            if (initialiseBlockGroupHierarchyBuilder)
            {
                _blockGroupHierarchyBuilder.Initialise(parentItemFilterScript.ItemFilterBlockGroups.First());
            }

            _masterComponentCollection = parentItemFilterScript.ItemFilterScriptSettings.ThemeComponentCollection;
            var block = new ItemFilterBlock(parentItemFilterScript);
            var showHideFound = false;

            foreach (var line in new LineReader(() => new StringReader(inputString)))
            {
                if (line.StartsWith(@"#") && !showHideFound)
                {
                    block.Description = line.TrimStart('#').TrimStart(' ');
                    continue;
                }

                var trimmedLine = line.Trim();
                var spaceOrEndOfLinePos = trimmedLine.IndexOf(" ", StringComparison.Ordinal) > 0 ? trimmedLine.IndexOf(" ", StringComparison.Ordinal) : trimmedLine.Length;

                var lineOption = trimmedLine.Substring(0, spaceOrEndOfLinePos);
                switch (lineOption)
                {
                    case "Show":
                    case "Hide":
                    case "ShowDisabled":
                    case "HideDisabled":
                    {
                        showHideFound = true;
                        block.Action = lineOption.StartsWith("Show") ? BlockAction.Show : BlockAction.Hide;
                        block.Enabled = !lineOption.EndsWith("Disabled");

                        // If block groups are enabled for this script, the comment after Show/Hide is parsed as a block
                        // group hierarchy, if block groups are disabled it is preserved as a simple text comment.
                        if (parentItemFilterScript.ItemFilterScriptSettings.BlockGroupsEnabled)
                        {
                            AddBlockGroupToBlock(block, trimmedLine);
                        }
                        else
                        {
                            block.ActionBlockItem.Comment = GetTextAfterFirstComment(trimmedLine);
                        }
                        break;
                    }
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
                        RemoveExistingBlockItemsOfType<RarityBlockItem>(block);

                        var blockItemValue = new RarityBlockItem();
                        var result = Regex.Match(trimmedLine, @"^\w+\s+([><!=]{0,2})\s*(\w+)$");
                        if (result.Groups.Count == 3)
                        {
                            blockItemValue.FilterPredicate.PredicateOperator =
                                EnumHelper.GetEnumValueFromDescription<FilterPredicateOperator>(string.IsNullOrEmpty(result.Groups[1].Value) ? "=" : result.Groups[1].Value);
                            blockItemValue.FilterPredicate.PredicateOperand =
                                (int)EnumHelper.GetEnumValueFromDescription<ItemRarity>(result.Groups[2].Value);
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
                    case "Corrupted":
                    {
                        AddBooleanItemToBlockItems<CorruptedBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "Identified":
                    {
                        AddBooleanItemToBlockItems<IdentifiedBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "ElderItem":
                    {
                        AddBooleanItemToBlockItems<ElderItemBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "ShaperItem":
                    {
                        AddBooleanItemToBlockItems<ShaperItemBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "ShapedMap":
                    {
                        AddBooleanItemToBlockItems<ShapedMapBlockItem>(block, trimmedLine);
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
                    case "PlayAlertSoundPositional":
                    {
                        // Only ever use the last PlayAlertSound item encountered as multiples aren't valid.
                        RemoveExistingBlockItemsOfType<SoundBlockItem>(block);
                        RemoveExistingBlockItemsOfType<PositionalSoundBlockItem>(block);

                        var match = Regex.Match(trimmedLine, @"\S+\s+(\S+)\s?(\d+)?");
                        
                        if (match.Success)
                        {
                            string firstValue = match.Groups[1].Value;
                            int secondValue;

                            if (match.Groups[2].Success)
                            {
                                secondValue = Convert.ToInt16(match.Groups[2].Value);
                            }
                            else
                            {
                                secondValue = 79;
                            }

                            if (lineOption == "PlayAlertSound")
                            {
                                var blockItemValue = new SoundBlockItem
                                {
                                    Value = firstValue,
                                    SecondValue = secondValue
                                };
                                block.BlockItems.Add(blockItemValue);
                            }
                            else
                            {
                                var blockItemValue = new PositionalSoundBlockItem
                                {
                                    Value = firstValue,
                                    SecondValue = secondValue
                                };
                                block.BlockItems.Add(blockItemValue);
                            }
                        }
                        break;
                    }
                    case "GemLevel":
                    {
                        AddNumericFilterPredicateItemToBlockItems<GemLevelBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "StackSize":
                    {
                        AddNumericFilterPredicateItemToBlockItems<StackSizeBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "HasExplicitMod":
                    {
                        AddStringListItemToBlockItems<HasExplicitModBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "ElderMap":
                    {
                        AddBooleanItemToBlockItems<ElderMapBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "DisableDropSound":
                    {
                        // Only ever use the last DisableDropSound item encountered as multiples aren't valid.
                        RemoveExistingBlockItemsOfType<DisableDropSoundBlockItem>(block);

                        AddBooleanItemToBlockItems<DisableDropSoundBlockItem>(block, trimmedLine);
                        break;
                    }
                }
            }

            return block;
        }

        private static void RemoveExistingBlockItemsOfType<T>(IItemFilterBlock block)
        {
            var existingBlockItemCount = block.BlockItems.Count(b => b.GetType() == typeof(T));
            if (existingBlockItemCount > 0)
            {
                var existingBlockItem = block.BlockItems.First(b => b.GetType() == typeof(T));
                block.BlockItems.Remove(existingBlockItem);
            }
        }

        private static void AddBooleanItemToBlockItems<T>(IItemFilterBlock block, string inputString) where T : BooleanBlockItem
        {
            var blockItem = Activator.CreateInstance<T>();
            var splitString = inputString.Split(' ');
            if (splitString.Length == 2)
            {
                blockItem.BooleanValue = splitString[1].Trim().ToLowerInvariant() == "true";
                block.BlockItems.Add(blockItem);
            }
        }

        private static void AddNumericFilterPredicateItemToBlockItems<T>(IItemFilterBlock block, string inputString) where T : NumericFilterPredicateBlockItem
        {
            var blockItem = Activator.CreateInstance<T>();
            
            SetNumericFilterPredicateFromString(blockItem.FilterPredicate, inputString);
            block.BlockItems.Add(blockItem);
        }

        private static void SetNumericFilterPredicateFromString(NumericFilterPredicate predicate, string inputString)
        {
            var result = Regex.Match(inputString, @"^\w+\s+([><=]{0,2})\s*(\d{0,3})$");
            if (result.Groups.Count != 3) return;

            predicate.PredicateOperator =
                EnumHelper.GetEnumValueFromDescription<FilterPredicateOperator>(string.IsNullOrEmpty(result.Groups[1].Value) ? "=" : result.Groups[1].Value);
            predicate.PredicateOperand = Convert.ToInt16(result.Groups[2].Value);
        }

        private static void AddStringListItemToBlockItems<T>(IItemFilterBlock block, string inputString) where T : StringListBlockItem
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

        private void AddColorItemToBlockItems<T>(IItemFilterBlock block, string inputString) where T : ColorBlockItem
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
                if (_masterComponentCollection != null)
                {
                    blockItem.ThemeComponent = _masterComponentCollection.AddComponent(componentType, componentName,
                        blockItem.Color);
                }
            }

            return blockItem;
        }

        public void ReplaceAudioVisualBlockItemsFromString(ObservableCollection<IItemFilterBlockItem> blockItems, string inputString)
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
                    case "PlayAlertSound":
                    {
                        var match = Regex.Match(line, @"\s+(\S+) (\d+)");
                        if (!match.Success) break;
                        blockItems.Add(new SoundBlockItem(match.Groups[1].Value, Convert.ToInt16(match.Groups[2].Value)));
                        break;
                    }
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
        
        private void AddBlockGroupToBlock(IItemFilterBlock block, string inputString)
        {
            var blockGroupText = GetTextAfterFirstComment(inputString);
            var blockGroups = blockGroupText.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => s.Trim())
                                            .ToList();

            if (blockGroups.Count(b => !string.IsNullOrEmpty(b.Trim())) > 0)
            {
                block.BlockGroup = _blockGroupHierarchyBuilder.IntegrateStringListIntoBlockGroupHierarchy(blockGroups);
                block.BlockGroup.IsChecked = block.Action == BlockAction.Show;
            }
        }

        private static string GetTextAfterFirstComment(string inputString)
        {
            var blockGroupStart = inputString.IndexOf("#", StringComparison.Ordinal);
            if (blockGroupStart <= 0) return string.Empty;

            return inputString.Substring(blockGroupStart + 1);
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

        public string TranslateItemFilterBlockBaseToString(IItemFilterBlockBase itemFilterBlockBase)
        {
            if (itemFilterBlockBase is IItemFilterBlock itemFilterBlock)
            {
                return TranslateItemFilterBlockToString(itemFilterBlock);
            }

            if (itemFilterBlockBase is IItemFilterCommentBlock itemFilterCommentBlock)
            {
                return TranslateItemFilterCommentBlockToString(itemFilterCommentBlock);
            }

            throw new InvalidOperationException("Unable to translate unknown ItemFilterBlock type");
        }

        // TODO: Private
        public string TranslateItemFilterCommentBlockToString(IItemFilterCommentBlock itemFilterCommentBlock)
        {
            // TODO: Tests
            // TODO: # Section: text?
            var commentWithHashes = string.Empty;

            // Add "# " to the beginning of each line of the comment before saving it
            foreach (var line in new LineReader(() => new StringReader(itemFilterCommentBlock.Comment)))
            {
                commentWithHashes += $"# {line.TrimStart(' ')}{Environment.NewLine}";
            }

            // Remove trailing newline
            return commentWithHashes.TrimEnd('\r', '\n');
        }
        
        // This method converts an ItemFilterBlock object into a string. This is used for copying a ItemFilterBlock
        // to the clipboard, and when saving a ItemFilterScript.
        // TODO: Private
        public string TranslateItemFilterBlockToString(IItemFilterBlock block)
        {
            var outputString = string.Empty;

            if (!block.Enabled)
            {
                outputString += "#Disabled Block Start" + Environment.NewLine;
            }

            if (!string.IsNullOrEmpty(block.Description))
            {
                outputString += "# " + block.Description + Environment.NewLine;
            }

            outputString += (!block.Enabled ? "#" : string.Empty) + block.Action.GetAttributeDescription();

            if (block.BlockGroup != null)
            {
                outputString += " # " + block.BlockGroup;
            }
            else if (!string.IsNullOrEmpty(block.ActionBlockItem?.Comment))
            {
                outputString += " #" + block.ActionBlockItem.Comment;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var blockItem in block.BlockItems.Where(b => b.GetType() != typeof(ActionBlockItem)).OrderBy(b => b.SortOrder))
            {
                if (blockItem.OutputText != string.Empty)
                {
                    outputString += (!block.Enabled ? _disabledNewLine : _newLine) + blockItem.OutputText;
                }
            }

            if (!block.Enabled)
            {
                outputString += Environment.NewLine +  "#Disabled Block End";
            }
            
            return outputString;
        }
    }
}
