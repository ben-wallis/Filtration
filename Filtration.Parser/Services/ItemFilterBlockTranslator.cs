using System;
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
        public IItemFilterCommentBlock TranslateStringToItemFilterCommentBlock(string inputString, IItemFilterScript parentItemFilterScript, string originalString = "")
        {
            var itemFilterCommentBlock = new ItemFilterCommentBlock(parentItemFilterScript) {OriginalText = originalString};

            foreach (var line in new LineReader(() => new StringReader(inputString)))
            {
                var trimmedLine = line.TrimStart(' ').TrimStart('#');
                itemFilterCommentBlock.Comment += trimmedLine + Environment.NewLine;
            }

            itemFilterCommentBlock.Comment = itemFilterCommentBlock.Comment.TrimEnd('\r', '\n');

            itemFilterCommentBlock.IsEdited = false;
            return itemFilterCommentBlock;
        }

        // This method converts a string into a ItemFilterBlock. This is used for pasting ItemFilterBlocks
        // and reading ItemFilterScripts from a file.
        public IItemFilterBlock TranslateStringToItemFilterBlock(string inputString, IItemFilterScript parentItemFilterScript, string originalString = "", bool initialiseBlockGroupHierarchyBuilder = false)
        {
            if (initialiseBlockGroupHierarchyBuilder)
            {
                _blockGroupHierarchyBuilder.Initialise(parentItemFilterScript.ItemFilterBlockGroups.First());
            }

            _masterComponentCollection = parentItemFilterScript.ItemFilterScriptSettings.ThemeComponentCollection;
            var block = new ItemFilterBlock(parentItemFilterScript);
            var showHideFound = false;
            block.OriginalText = originalString;

            foreach (var line in new LineReader(() => new StringReader(inputString)))
            {
                if (line.StartsWith(@"#"))
                {
                    if(!showHideFound)
                    {
                        block.Description = line.TrimStart('#').TrimStart(' ');
                    }
                    else
                    {
                        if(block.BlockItems.Count > 1)
                        {
                            block.BlockItems.Last().Comment += Environment.NewLine + line.TrimStart('#');
                        }
                        else
                        {
                            block.ActionBlockItem.Comment += Environment.NewLine + line.TrimStart('#');
                        }
                    }
                    continue;
                }

                var fullLine = line.Trim();
                var trimmedLine = fullLine;
                var blockComment = "";
                var themeComponentType = -1;
                if(trimmedLine.IndexOf('#') > 0)
                {
                    blockComment = trimmedLine.Substring(trimmedLine.IndexOf('#') + 1);
                    trimmedLine = trimmedLine.Substring(0, trimmedLine.IndexOf('#')).Trim();
                }
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
                            AddBlockGroupToBlock(block, fullLine);
                        }
                        else
                        {
                            block.ActionBlockItem.Comment = GetTextAfterFirstComment(fullLine);
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
                    case "Prophecy":
                    {
                        AddStringListItemToBlockItems<ProphecyBlockItem>(block, trimmedLine);
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
                    case "SynthesisedItem":
                    {
                        AddBooleanItemToBlockItems<SynthesisedItemBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "FracturedItem":
                    {
                        AddBooleanItemToBlockItems<FracturedItemBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "AnyEnchantment":
                    {
                        AddBooleanItemToBlockItems<AnyEnchantmentBlockItem>(block, trimmedLine);
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

                        var result = Regex.Matches(trimmedLine, @"([\w\s]*)");

                        var blockItem = new TextColorBlockItem {Color = GetColorFromString(result[0].Groups[1].Value)};
                        block.BlockItems.Add(blockItem);
                        themeComponentType = (int)ThemeComponentType.TextColor;
                        break;
                    }
                    case "SetBackgroundColor":
                    {
                        // Only ever use the last SetBackgroundColor item encountered as multiples aren't valid.
                        RemoveExistingBlockItemsOfType<BackgroundColorBlockItem>(block);

                        var result = Regex.Matches(trimmedLine, @"([\w\s]*)");

                        var blockItem = new BackgroundColorBlockItem {Color = GetColorFromString(result[0].Groups[1].Value)};
                        block.BlockItems.Add(blockItem);
                        themeComponentType = (int)ThemeComponentType.BackgroundColor;
                        break;
                    }
                    case "SetBorderColor":
                    {
                        // Only ever use the last SetBorderColor item encountered as multiples aren't valid.
                        RemoveExistingBlockItemsOfType<BorderColorBlockItem>(block);

                        var result = Regex.Matches(trimmedLine, @"([\w\s]*)");

                        var blockItem = new BorderColorBlockItem {Color = GetColorFromString(result[0].Groups[1].Value)};
                        block.BlockItems.Add(blockItem);
                        themeComponentType = (int)ThemeComponentType.BorderColor;
                        break;
                    }
                    case "SetFontSize":
                    {
                        // Only ever use the last SetFontSize item encountered as multiples aren't valid.
                        RemoveExistingBlockItemsOfType<FontSizeBlockItem>(block);

                        var match = Regex.Matches(trimmedLine, @"(\s+(\d+)\s*)");
                        if (match.Count > 0)
                        {
                            var blockItem = new FontSizeBlockItem(Convert.ToInt16(match[0].Groups[2].Value));
                            block.BlockItems.Add(blockItem);
                            themeComponentType = (int)ThemeComponentType.FontSize;
                        }
                        break;
                    }
                    case "PlayAlertSound":
                    case "PlayAlertSoundPositional":
                    {
                        // Only ever use the last PlayAlertSound item encountered as multiples aren't valid.
                        RemoveExistingBlockItemsOfType<SoundBlockItem>(block);
                        RemoveExistingBlockItemsOfType<PositionalSoundBlockItem>(block);
                        RemoveExistingBlockItemsOfType<CustomSoundBlockItem>(block);

                        var match = Regex.Match(trimmedLine, @"\S+\s+(\S+)\s?(\d+)?");

                        if (match.Success)
                        {
                            string firstValue = match.Groups[1].Value;

                            var secondValue = match.Groups[2].Success ? Convert.ToInt16(match.Groups[2].Value) : 79;

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
                            themeComponentType = (int)ThemeComponentType.AlertSound;
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
                    case "HasEnchantment":
                    {
                        AddStringListItemToBlockItems<HasEnchantmentBlockItem>(block, trimmedLine);
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

                        AddNilItemToBlockItems<DisableDropSoundBlockItem>(block, trimmedLine);
                        break;
                    }
                    case "MinimapIcon":
                    {
                        // Only ever use the last Icon item encountered as multiples aren't valid.
                        RemoveExistingBlockItemsOfType<MapIconBlockItem>(block);

                        // TODO: Get size, color, shape values programmatically
                        var match = Regex.Match(trimmedLine,
                            @"\S+\s+(0|1|2)\s+(Red|Green|Blue|Brown|White|Yellow)\s+(Circle|Diamond|Hexagon|Square|Star|Triangle)\s*([#]?)(.*)",
                            RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            var blockItemValue = new MapIconBlockItem
                            {
                                Size = (IconSize)short.Parse(match.Groups[1].Value),
                                Color = EnumHelper.GetEnumValueFromDescription<IconColor>(match.Groups[2].Value),
                                Shape = EnumHelper.GetEnumValueFromDescription<IconShape>(match.Groups[3].Value)
                            };

                            block.BlockItems.Add(blockItemValue);
                            themeComponentType = (int)ThemeComponentType.Icon;
                        }
                        break;
                    }
                    case "PlayEffect":
                    {
                        // Only ever use the last BeamColor item encountered as multiples aren't valid.
                        RemoveExistingBlockItemsOfType<PlayEffectBlockItem>(block);

                        // TODO: Get colors programmatically
                        var match = Regex.Match(trimmedLine, @"\S+\s+(Red|Green|Blue|Brown|White|Yellow)\s*(Temp)?", RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            var blockItemValue = new PlayEffectBlockItem
                            {
                                Color = EnumHelper.GetEnumValueFromDescription<EffectColor>(match.Groups[1].Value),
                                Temporary = match.Groups[2].Value.Trim().ToLower() == "temp"
                            };
                            block.BlockItems.Add(blockItemValue);
                            themeComponentType = (int)ThemeComponentType.Effect;
                        }
                        break;
                    }
                    case "CustomAlertSound":
                    {
                        // Only ever use the last CustomSoundBlockItem item encountered as multiples aren't valid.
                        RemoveExistingBlockItemsOfType<CustomSoundBlockItem>(block);
                        RemoveExistingBlockItemsOfType<SoundBlockItem>(block);
                        RemoveExistingBlockItemsOfType<PositionalSoundBlockItem>(block);

                        var match = Regex.Match(trimmedLine, @"\S+\s+""([^\*\<\>\?|]+)""");

                        if (match.Success)
                        {
                            var blockItemValue = new CustomSoundBlockItem
                            {
                                Value = match.Groups[1].Value
                            };
                            block.BlockItems.Add(blockItemValue);
                            themeComponentType = (int)ThemeComponentType.CustomSound;
                        }
                        break;
                    }
                    case "MapTier":
                    {
                        AddNumericFilterPredicateItemToBlockItems<MapTierBlockItem>(block, trimmedLine);
                        break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(blockComment) && block.BlockItems.Count > 1)
                {
                    if(!(block.BlockItems.Last() is IBlockItemWithTheme blockItemWithTheme))
                    {
                        block.BlockItems.Last().Comment = blockComment;
                    }
                    else
                    {
                        switch((ThemeComponentType)themeComponentType)
                        {
                            case ThemeComponentType.AlertSound:
                            {
                                ThemeComponent themeComponent;
                                if(blockItemWithTheme is SoundBlockItem item)
                                {
                                    themeComponent = _masterComponentCollection.AddComponent(ThemeComponentType.AlertSound, blockComment.Trim(),
                                        item.Value, item.SecondValue);
                                }
                                else
                                {
                                    themeComponent = _masterComponentCollection.AddComponent(ThemeComponentType.AlertSound, blockComment.Trim(),
                                        ((PositionalSoundBlockItem)blockItemWithTheme).Value, ((PositionalSoundBlockItem)blockItemWithTheme).SecondValue);
                                }
                                blockItemWithTheme.ThemeComponent = themeComponent;
                                break;
                            }
                            case ThemeComponentType.BackgroundColor:
                            {
                                ThemeComponent themeComponent = _masterComponentCollection.AddComponent(ThemeComponentType.BackgroundColor,
                                    blockComment.Trim(), ((BackgroundColorBlockItem)blockItemWithTheme).Color);
                                blockItemWithTheme.ThemeComponent = themeComponent;
                                break;
                            }
                            case ThemeComponentType.BorderColor:
                            {
                                ThemeComponent themeComponent = _masterComponentCollection.AddComponent(ThemeComponentType.BorderColor,
                                    blockComment.Trim(), ((BorderColorBlockItem)blockItemWithTheme).Color);
                                blockItemWithTheme.ThemeComponent = themeComponent;
                                break;
                            }
                            case ThemeComponentType.CustomSound:
                            {
                                ThemeComponent themeComponent = _masterComponentCollection.AddComponent(ThemeComponentType.CustomSound,
                                    blockComment.Trim(), ((CustomSoundBlockItem)blockItemWithTheme).Value);
                                blockItemWithTheme.ThemeComponent = themeComponent;
                                    break;
                            }
                            case ThemeComponentType.Effect:
                            {
                                ThemeComponent themeComponent = _masterComponentCollection.AddComponent(ThemeComponentType.Effect,
                                    blockComment.Trim(), ((EffectColorBlockItem)blockItemWithTheme).Color, ((EffectColorBlockItem)blockItemWithTheme).Temporary);
                                blockItemWithTheme.ThemeComponent = themeComponent;
                                break;
                            }
                            case ThemeComponentType.FontSize:
                            {
                                ThemeComponent themeComponent = _masterComponentCollection.AddComponent(ThemeComponentType.FontSize,
                                    blockComment.Trim(), ((FontSizeBlockItem)blockItemWithTheme).Value);
                                blockItemWithTheme.ThemeComponent = themeComponent;
                                break;
                            }
                            case ThemeComponentType.Icon:
                            {
                                ThemeComponent themeComponent = _masterComponentCollection.AddComponent(ThemeComponentType.Icon, blockComment.Trim(),
                                    ((IconBlockItem)blockItemWithTheme).Size, ((IconBlockItem)blockItemWithTheme).Color, ((IconBlockItem)blockItemWithTheme).Shape);
                                blockItemWithTheme.ThemeComponent = themeComponent;
                                break;
                            }
                            case ThemeComponentType.TextColor:
                            {
                                ThemeComponent themeComponent = _masterComponentCollection.AddComponent(ThemeComponentType.TextColor,
                                    blockComment.Trim(), ((TextColorBlockItem)blockItemWithTheme).Color);
                                blockItemWithTheme.ThemeComponent = themeComponent;
                                break;
                            }
                        }
                    }
                }
            }
            block.IsEdited = false;
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
            inputString = Regex.Replace(inputString, @"\s+", " ");
            var blockItem = Activator.CreateInstance<T>();
            var splitString = inputString.Split(' ');
            if (splitString.Length == 2)
            {
                blockItem.BooleanValue = splitString[1].Trim().ToLowerInvariant() == "true";
                block.BlockItems.Add(blockItem);
            }
        }

        private static void AddNilItemToBlockItems<T>(IItemFilterBlock block, string inputString) where T : NilBlockItem
        {
            var blockItem = Activator.CreateInstance<T>();
            blockItem.Comment = GetTextAfterFirstComment(inputString);
            block.BlockItems.Add(blockItem);
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
                var blockComment = "";
                var trimmedLine = line.Trim();
                if (trimmedLine.IndexOf('#') > 0)
                {
                    blockComment = trimmedLine.Substring(trimmedLine.IndexOf('#') + 1).Trim();
                    trimmedLine = trimmedLine.Substring(0, trimmedLine.IndexOf('#')).Trim();
                }

                switch (matches.Value)
                {
                    case "DisableDropSound":
                    {
                        blockItems.Add(new DisableDropSoundBlockItem());
                        break;
                    }
                    case "PlayAlertSound":
                    {
                        var match = Regex.Match(trimmedLine, @"\s+(\S+) (\d+)");
                        if (!match.Success) break;
                        var blockItem = new SoundBlockItem(match.Groups[1].Value, Convert.ToInt16(match.Groups[2].Value));
                        if(_masterComponentCollection != null && !string.IsNullOrWhiteSpace(blockComment))
                        {
                            ThemeComponent themeComponent = _masterComponentCollection.AddComponent(ThemeComponentType.AlertSound,
                                blockComment, blockItem.Value, blockItem.SecondValue);
                                blockItem.ThemeComponent = themeComponent;
                        }
                        blockItems.Add(blockItem);
                        break;
                    }
                    case "PlayAlertSoundPositional":
                    {
                        var match = Regex.Match(trimmedLine, @"\s+(\S+) (\d+)");
                        if (!match.Success) break;
                        var blockItem = new PositionalSoundBlockItem(match.Groups[1].Value, Convert.ToInt16(match.Groups[2].Value));
                        if(_masterComponentCollection != null && !string.IsNullOrWhiteSpace(blockComment))
                        {
                            ThemeComponent themeComponent = _masterComponentCollection.AddComponent(ThemeComponentType.AlertSound,
                                blockComment, blockItem.Value, blockItem.SecondValue);
                                blockItem.ThemeComponent = themeComponent;
                        }
                        blockItems.Add(blockItem);
                        break;
                    }
                    case "SetTextColor":
                    {
                        var result = Regex.Matches(trimmedLine, @"([\w\s]*)");

                        var blockItem = new TextColorBlockItem {Color = GetColorFromString(result[0].Groups[1].Value)};
                        if(_masterComponentCollection != null && !string.IsNullOrWhiteSpace(blockComment))
                        {
                            ThemeComponent themeComponent = _masterComponentCollection.AddComponent(ThemeComponentType.TextColor,
                                blockComment, blockItem.Color);
                            blockItem.ThemeComponent = themeComponent;
                        }
                        blockItems.Add(blockItem);
                        break;
                    }
                    case "SetBackgroundColor":
                    {
                        var result = Regex.Matches(trimmedLine, @"([\w\s]*)");

                        var blockItem = new BackgroundColorBlockItem {Color = GetColorFromString(result[0].Groups[1].Value)};
                        if(_masterComponentCollection != null && !string.IsNullOrWhiteSpace(blockComment))
                        {
                            ThemeComponent themeComponent = _masterComponentCollection.AddComponent(ThemeComponentType.BackgroundColor,
                                blockComment, blockItem.Color);
                            blockItem.ThemeComponent = themeComponent;
                        }
                        blockItems.Add(blockItem);
                        break;
                    }
                    case "SetBorderColor":
                    {
                        var result = Regex.Matches(trimmedLine, @"([\w\s]*)");

                        var blockItem = new BorderColorBlockItem {Color = GetColorFromString(result[0].Groups[1].Value)};
                        if(_masterComponentCollection != null && !string.IsNullOrWhiteSpace(blockComment))
                        {
                            ThemeComponent themeComponent = _masterComponentCollection.AddComponent(ThemeComponentType.BorderColor,
                                blockComment, blockItem.Color);
                            blockItem.ThemeComponent = themeComponent;
                        }
                        blockItems.Add(blockItem);
                        break;
                    }
                    case "SetFontSize":
                    {
                        var match = Regex.Match(trimmedLine, @"\s+(\d+)");
                        if (!match.Success) break;
                        var blockItem = new FontSizeBlockItem(Convert.ToInt16(match.Value));
                        if (_masterComponentCollection != null && !string.IsNullOrWhiteSpace(blockComment))
                        {
                            ThemeComponent themeComponent = _masterComponentCollection.AddComponent(ThemeComponentType.FontSize,
                                blockComment, blockItem.Value);
                            blockItem.ThemeComponent = themeComponent;
                        }
                        blockItems.Add(blockItem);
                        break;
                    }
                    case "MinimapIcon":
                    {
                        // TODO: Get size, color, shape values programmatically
                        var match = Regex.Match(trimmedLine,
                            @"\S+\s+(0|1|2)\s+(Red|Green|Blue|Brown|White|Yellow)\s+(Circle|Diamond|Hexagon|Square|Star|Triangle)\s*([#]?)(.*)",
                            RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            var blockItemValue = new MapIconBlockItem
                            {
                                Size = (IconSize)short.Parse(match.Groups[1].Value),
                                Color = EnumHelper.GetEnumValueFromDescription<IconColor>(match.Groups[2].Value),
                                Shape = EnumHelper.GetEnumValueFromDescription<IconShape>(match.Groups[3].Value)
                            };
                            
                            blockItems.Add(blockItemValue);
                        }
                        break;
                    }
                    case "PlayEffect":
                    {
                        // TODO: Get colors programmatically
                        var match = Regex.Match(trimmedLine, @"\S+\s+(Red|Green|Blue|Brown|White|Yellow)\s*(Temp)?", RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            var blockItemValue = new PlayEffectBlockItem
                            {
                                Color = EnumHelper.GetEnumValueFromDescription<EffectColor>(match.Groups[1].Value),
                                Temporary = match.Groups[2].Value.Trim().ToLower() == "temp"
                            };
                            blockItems.Add(blockItemValue);
                        }
                        break;
                    }
                    case "CustomAlertSound":
                    {
                        var match = Regex.Match(trimmedLine, @"\S+\s+""([^\*\<\>\?|]+)""");

                        if (match.Success)
                        {
                            var blockItemValue = new CustomSoundBlockItem
                            {
                                Value = match.Groups[1].Value
                            };
                            blockItems.Add(blockItemValue);
                        }
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
                block.BlockGroup = _blockGroupHierarchyBuilder.IntegrateStringListIntoBlockGroupHierarchy(blockGroups,
                    block.Action == BlockAction.Show, block.Enabled);
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
                        A = 240,
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
            if (!itemFilterCommentBlock.IsEdited)
            {
                return itemFilterCommentBlock.OriginalText;
            }

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
            if(!block.IsEdited)
            {
                return block.OriginalText;
            }

            var outputString = string.Empty;

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

            //TODO: Disabled for the time being. A better solution is needed.
            // Replace 'Maelström' to prevent encoding problems in other editors
            //outputString.Replace("Maelström Staff", "Maelstr");
            //outputString.Replace("Maelström of Chaos", "Maelstr");
            //outputString.Replace("Maelström", "Maelstr");

            return outputString;
        }
    }
}
