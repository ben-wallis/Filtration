using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
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

        public static string PreprocessDisabledBlocks(string inputString, out List<bool> inBlock)
        {
            bool inDisabledBlock = false;
            inBlock = new List<bool>();

            var lines = Regex.Split(inputString, "\r\n|\r|\n").ToList();
            // find first show/hide and check script
            for (var i = 0; i < lines.Count; i++)
            {
                inBlock.Add(false);
                lines[i] = lines[i].Trim();
                if(!lines[i].StartsWith("#"))
                {
                    if ((lines[i].StartsWith("Show") || lines[i].StartsWith("Hide")) && (lines[i].Length == 4 || lines[i][4] == ' ')) // found
                    {
                        inBlock[i] = true;
                        break;
                    }
                    else // This means script has wrong syntax, just replace those lines with empty string
                    {
                        lines[i] = "";
                    }
                }
            }

            // find remaining boundaries
            var lastInBlock = inBlock.Count - 1;
            for (var i = inBlock.Count; i < lines.Count; i++)
            {
                inBlock.Add(false);
                lines[i] = lines[i].Trim();
                if (!lines[i].StartsWith("#") && lines[i].Length > 0)
                {
                    if (!lines[i].StartsWith("Show") && !lines[i].StartsWith("Hide")) // Continuing inline
                    {
                        for(int j = lastInBlock + 1; j < i; j++)
                        {
                            inBlock[j] = true;
                        }
                    }
                    lastInBlock = i;
                    inBlock[i] = true;
                }
            }

            for (var i = 0; i < lines.Count; i++)
            {
                if (!inDisabledBlock && lines[i].StartsWith("#"))
                {
                    string curLine = lines[i].Substring(1).Trim();
                    if ((curLine.StartsWith("Show") || curLine.StartsWith("Hide")) && (curLine.Length == 4 || curLine[4] == ' ') && !inBlock[i])
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

            if(Regex.Matches(inputString, @"#Disabled\sBlock\s(Start|End).*?\n").Count > 0)
            {
                if (MessageBox.Show(
                    "Loaded script contains special '#Disabled Block Start' lines." +
                    " These may be coming from old versions of Filtration or Greengroove's filter." +
                    "It is suggested to remove them however this may cause problems with original source" +
                    Environment.NewLine + "(There is no in game effect of those lines)" +
                    Environment.NewLine + Environment.NewLine + "Would you like to remove them?", "Special Comment Lines Found",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    //Remove old disabled tags
                    inputString = Regex.Replace(inputString, @"#Disabled\sBlock\s(Start|End).*?\n", "");
                    inputString = (inputString.EndsWith("\n#Disabled Block End")) ? inputString.Substring(0, inputString.Length - 19) : inputString;
                }
            }

            var originalLines = Regex.Split(inputString, "\r\n|\r|\n");

            inputString = inputString.Replace("\t", "");
            List<bool> inBlock;
            inputString = PreprocessDisabledBlocks(inputString, out inBlock);

            var conditionBoundaries = IdentifyBlockBoundaries(inputString, inBlock);

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

        public IItemFilterScript TranslatePastedStringToItemFilterScript(string inputString, bool blockGroupsEnabled)
        {
            //Remove old disabled tags to prevent messagebox on paste
            inputString = Regex.Replace(inputString, @"#Disabled\sBlock\s(Start|End).*?\n", "");
            inputString = (inputString.EndsWith("\n#Disabled Block End")) ? inputString.Substring(0, inputString.Length - 19) : inputString;

            inputString = (blockGroupsEnabled ? "# EnableBlockGroups" : "#") + Environment.NewLine + Environment.NewLine + inputString;

            return TranslateStringToItemFilterScript(inputString);
        }

        private static LinkedList<ItemFilterBlockBoundary> IdentifyBlockBoundaries(string inputString, List<bool> inBlock)
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

                // A line starting with a comment when we're inside a ItemFilterBlock boundary may represent the end of that block 
                // or a block item comment
                if (trimmedLine.StartsWith("#") && !inBlock[currentLine] &&
                    currentItemFilterBlockBoundary.BoundaryType == ItemFilterBlockBoundaryType.ItemFilterBlock)
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
