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
            var showHideFound = false;

            var lines = Regex.Split(inputString, "\r\n|\r|\n").ToList();
            var linesToRemove = new List<int>();

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith("#Disabled Block Start"))
                {
                    inDisabledBlock = true;
                    linesToRemove.Add(i);
                    continue;
                }
                if (inDisabledBlock)
                {
                    if (lines[i].StartsWith("#Disabled Block End"))
                    {
                        inDisabledBlock = false;
                        showHideFound = false;
                        linesToRemove.Add(i);
                        continue;
                    }

                    lines[i] = lines[i].TrimStart('#');
                    lines[i] = lines[i].Replace("#", " # ");
                    var spaceOrEndOfLinePos = lines[i].IndexOf(" ", StringComparison.Ordinal) > 0 ? lines[i].IndexOf(" ", StringComparison.Ordinal) : lines[i].Length;
                    var lineOption = lines[i].Substring(0, spaceOrEndOfLinePos);

                    // If we haven't found a Show or Hide line yet, then this is probably the block comment.
                    // Put its # back on and skip to the next line.
                    if (lineOption != "Show" && lineOption != "Hide" && showHideFound == false)
                    {
                        lines[i] = "#" + lines[i];
                        continue;
                    }

                    if (lineOption == "Show")
                    {
                        lines[i] = lines[i].Replace("Show", "ShowDisabled");
                        showHideFound = true;
                    }
                    else if (lineOption == "Hide")
                    {
                        lines[i] = lines[i].Replace("Hide", "HideDisabled");
                        showHideFound = true;
                    }
                }
            }

            for (var i = linesToRemove.Count - 1; i >= 0; i--)
            {
                lines.RemoveAt(linesToRemove[i]);
            }

            return lines.Aggregate((c, n) => c + Environment.NewLine + n);
        }

        public IItemFilterScript TranslateStringToItemFilterScript(string inputString)
        {
            var script = _itemFilterScriptFactory.Create();
            _blockGroupHierarchyBuilder.Initialise(script.ItemFilterBlockGroups.First());

            inputString = inputString.Replace("\t", "");
            if (inputString.Contains("#Disabled Block Start"))
            {
                inputString = PreprocessDisabledBlocks(inputString);
            }

            var conditionBoundaries = IdentifyBlockBoundaries(inputString);

            var lines = Regex.Split(inputString, "\r\n|\r|\n");

            // Process the script header
            for (var i = 0; i < conditionBoundaries.Skip(1).First().StartLine; i++)
            {
                if (lines[i].StartsWith("#"))
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

                if (boundary.Value.BoundaryType == ItemFilterBlockBoundaryType.ItemFilterBlock)
                {
                    script.ItemFilterBlocks.Add(_blockTranslator.TranslateStringToItemFilterBlock(blockString, script.ItemFilterScriptSettings));
                }
                else
                {
                    script.ItemFilterBlocks.Add(_blockTranslator.TranslateStringToItemFilterCommentBlock(blockString));
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
                else if (trimmedLine.StartsWith("#") && string.IsNullOrWhiteSpace(previousLine) && currentItemFilterBlockBoundary.BoundaryType != ItemFilterBlockBoundaryType.ScriptDescription)
                {
                    if (blockBoundaries.Count > 0)
                    {
                        blockBoundaries.AddLast(currentItemFilterBlockBoundary);
                    }
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
                outputString += Environment.NewLine;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var block in script.ItemFilterBlocks)
            {
                outputString += _blockTranslator.TranslateItemFilterBlockToString(block as ItemFilterBlock) + Environment.NewLine;

                if (Settings.Default.ExtraLineBetweenBlocks)
                {
                    outputString += Environment.NewLine;
                }
            }

            return outputString;
        }
    }
}
