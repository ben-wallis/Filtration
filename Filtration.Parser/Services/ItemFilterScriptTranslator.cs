using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Filtration.Common.Utilities;
using Filtration.ObjectModel;
using Filtration.ObjectModel.Factories;
using Filtration.Parser.Interface.Services;
using Filtration.Properties;

namespace Filtration.Parser.Services
{

    internal class ItemFilterBlockBoundary
    {
        public ItemFilterBlockBoundary(int startLine, ItemFilterBlockBoundaryType itemFilterBlockBoundaryType)
        {
            StartLine = startLine;
            BoundaryType = itemFilterBlockBoundaryType;
        }

        public int StartLine { get; set; }
        public ItemFilterBlockBoundaryType BoundaryType { get; set; }

    }

    internal enum ItemFilterBlockBoundaryType
    {
        ScriptDescription,
        ItemFilterBlock,
        CommentBlock
    }

    internal class ItemFilterScriptTranslator : IItemFilterScriptTranslator
    {
        private readonly IItemFilterBlockTranslator _blockTranslator;
        private readonly IItemFilterScriptFactory _itemFilterScriptFactory;
        private readonly IBlockGroupHierarchyBuilder _blockGroupHierarchyBuilder;

        public ItemFilterScriptTranslator(IBlockGroupHierarchyBuilder blockGroupHierarchyBuilder,
                                          IItemFilterBlockTranslator blockTranslator,
                                          IItemFilterScriptFactory itemFilterScriptFactory)
        {
            _blockGroupHierarchyBuilder = blockGroupHierarchyBuilder;
            _blockTranslator = blockTranslator;
            _itemFilterScriptFactory = itemFilterScriptFactory;
        }

        public static string PreprocessDisabledBlocks(string inputString)
        {
            bool inDisabledBlock = false;

            var lines = Regex.Split(inputString, "\r\n|\r|\n").ToList();

            for (var i = 0; i < lines.Count; i++)
            {
                if (!inDisabledBlock && lines[i].StartsWith("#"))
                {
                    string curLine = Regex.Replace(lines[i].Substring(1), @"\s+", "");
                    if ((curLine.StartsWith("Show") || curLine.StartsWith("Hide")) && (curLine.Length == 4 || curLine[4] == '#'))
                    {
                        inDisabledBlock = true;
                        lines[i] = lines[i].Substring(1).TrimStart(' ');
                        lines[i] = lines[i].Substring(0, 4) + "Disabled" + lines[i].Substring(4);
                        continue;
                    }
                }
                if (inDisabledBlock)
                {
                    if (!lines[i].StartsWith("#"))
                    {
                        inDisabledBlock = false;
                    }
                    else
                    {
                        lines[i] = lines[i].Substring(1);
                    }
                }
            }

            return lines.Aggregate((c, n) => c + Environment.NewLine + n);
        }

        public IItemFilterScript TranslateStringToItemFilterScript(string inputString)
        {
            var script = _itemFilterScriptFactory.Create();
            _blockGroupHierarchyBuilder.Initialise(script.ItemFilterBlockGroups.First());

            //Remove old disabled tags
            inputString = Regex.Replace(inputString, @"#Disabled\sBlock\s(Start|End).*?\n", "");
            inputString = (inputString.EndsWith("\n#Disabled Block End")) ? inputString.Substring(0, inputString.Length - 19) : inputString;

            var originalLines = Regex.Split(inputString, "\r\n|\r|\n");

            inputString = inputString.Replace("\t", "");
            inputString = PreprocessDisabledBlocks(inputString);

            var conditionBoundaries = IdentifyBlockBoundaries(inputString);

            var lines = Regex.Split(inputString, "\r\n|\r|\n");

            // Process the script header
            for (var i = 0; i < conditionBoundaries.Skip(1).First().StartLine; i++)
            {
                if (lines[i].StartsWith("# EnableBlockGroups"))
                {
                    script.ItemFilterScriptSettings.BlockGroupsEnabled = true;
                }
                else if (lines[i].StartsWith("#"))
                {
                    script.Description += lines[i].Substring(1).Trim(' ') + Environment.NewLine;
                }
            }

            if (!string.IsNullOrEmpty(script.Description))
            {
                script.Description = script.Description.TrimEnd('\n').TrimEnd('\r');
            }

            // Extract each block from between boundaries and translate it into a ItemFilterBlock object
            // and add that object to the ItemFilterBlocks list 
            for (var boundary = conditionBoundaries.First; boundary != null; boundary = boundary.Next)
            {
                if (boundary.Value.BoundaryType == ItemFilterBlockBoundaryType.ScriptDescription)
                {
                    continue;
                }

                var begin = boundary.Value.StartLine;
                var end = boundary.Next?.Value.StartLine ?? lines.Length;
                var block = new string[end - begin];
                Array.Copy(lines, begin, block, 0, end - begin);
                var blockString = string.Join("\r\n", block);
                Array.Copy(originalLines, begin, block, 0, end - begin);
                var originalString = "";
                for (var i = block.Length - 1; i >= 0; i--)
                {
                    if(block[i].Replace(" ", "").Replace("\t", "").Length > 0)
                    {
                        originalString = string.Join("\r\n", block, 0, i + 1);
                        break;
                    }
                }

                if (boundary.Value.BoundaryType == ItemFilterBlockBoundaryType.ItemFilterBlock)
                {
                    script.ItemFilterBlocks.Add(_blockTranslator.TranslateStringToItemFilterBlock(blockString, script, originalString));
                }
                else
                {
                    script.ItemFilterBlocks.Add(_blockTranslator.TranslateStringToItemFilterCommentBlock(blockString, script, originalString));
                }
            }

            _blockGroupHierarchyBuilder.Cleanup();
            return script;
        }
        
        private static LinkedList<ItemFilterBlockBoundary> IdentifyBlockBoundaries(string inputString)
        {
            var blockBoundaries = new LinkedList<ItemFilterBlockBoundary>();
            var previousLine = string.Empty;
            var currentLine = -1;
            
            var currentItemFilterBlockBoundary = new ItemFilterBlockBoundary(0, ItemFilterBlockBoundaryType.ScriptDescription);

            foreach (var line in new LineReader(() => new StringReader(inputString)))
            {
                currentLine++;
                var trimmedLine = line.Trim(' ');
                
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    previousLine = line;
                    continue;
                }

                // A line starting with a comment when we're inside a ItemFilterBlock boundary represents the end of that block 
                // as ItemFilterBlocks cannot have comment lines after the block description
                if (trimmedLine.StartsWith("#") && currentItemFilterBlockBoundary.BoundaryType == ItemFilterBlockBoundaryType.ItemFilterBlock)
                {
                    blockBoundaries.AddLast(currentItemFilterBlockBoundary);
                    currentItemFilterBlockBoundary = new ItemFilterBlockBoundary(currentLine, ItemFilterBlockBoundaryType.CommentBlock);
                }
                // A line starting with a comment where the previous line was null represents the start of a new comment (unless we're on the first
                // line in which case it's not a new comment).
                else if (trimmedLine.StartsWith("#") && string.IsNullOrWhiteSpace(previousLine) && currentLine > 0)
                {
                    blockBoundaries.AddLast(currentItemFilterBlockBoundary);
                    currentItemFilterBlockBoundary = new ItemFilterBlockBoundary(currentLine, ItemFilterBlockBoundaryType.CommentBlock);
                }

                else if (trimmedLine.StartsWith("Show") || trimmedLine.StartsWith("Hide"))
                {
                    // If the line previous to the Show or Hide line is a comment then we should include that in the block
                    // as it represents the block description.
                    // currentLine > 2 caters for an edge case where the script description is a single line and the first
                    // block has no description. This prevents the script description from being assigned to the first block's description.
                    if (!(currentItemFilterBlockBoundary.StartLine == currentLine - 1 && currentItemFilterBlockBoundary.BoundaryType == ItemFilterBlockBoundaryType.CommentBlock))
                    {
                        blockBoundaries.AddLast(currentItemFilterBlockBoundary);
                    }
                    currentItemFilterBlockBoundary = new ItemFilterBlockBoundary(previousLine.StartsWith("#") && currentLine > 2 ? currentLine - 1 : currentLine,
                        ItemFilterBlockBoundaryType.ItemFilterBlock);
                }

                previousLine = line;
            }

            if (blockBoundaries.Last.Value != currentItemFilterBlockBoundary)
            {
                blockBoundaries.AddLast(currentItemFilterBlockBoundary);
            }

            return blockBoundaries;
        }

        public string TranslateItemFilterScriptToString(IItemFilterScript script)
        {
            var outputString = string.Empty;

            outputString += "# Script edited with Filtration - https://github.com/ben-wallis/Filtration" +
                            Environment.NewLine;

            if (script.ItemFilterScriptSettings.BlockGroupsEnabled)
            {
                outputString += "# EnableBlockGroups" + Environment.NewLine;
            }

            if (!string.IsNullOrEmpty(script.Description))
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var line in new LineReader(() => new StringReader(script.Description)))
                {
                    if (!line.Contains("Script edited with Filtration"))
                    {
                        outputString += "# " + line + Environment.NewLine;
                    }
                }
            }
            outputString += Environment.NewLine;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var block in script.ItemFilterBlocks)
            {
                // Do not save temporary block until the new feature is fully implemented
                if(block is ObjectModel.BlockItemTypes.IconBlockItem)
                {
                    continue;
                }

                outputString += _blockTranslator.TranslateItemFilterBlockBaseToString(block) + Environment.NewLine;

                if (Settings.Default.ExtraLineBetweenBlocks)
                {
                    outputString += Environment.NewLine;
                }
            }

            return outputString;
        }
    }
}
