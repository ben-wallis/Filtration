using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Castle.Core.Internal;
using Filtration.ObjectModel;
using Filtration.Properties;
using Filtration.Utilities;

namespace Filtration.Translators
{
    internal interface IItemFilterScriptTranslator
    {
        ItemFilterScript TranslateStringToItemFilterScript(string inputString);
        string TranslateItemFilterScriptToString(ItemFilterScript script);
    }

    internal class ItemFilterScriptTranslator : IItemFilterScriptTranslator
    {
        private readonly IItemFilterBlockTranslator _blockTranslator;
        private readonly IBlockGroupHierarchyBuilder _blockGroupHierarchyBuilder;
        private readonly IThemeComponentListBuilder _themeComponentListBuilder;

        public ItemFilterScriptTranslator(IItemFilterBlockTranslator blockTranslator,
                                          IBlockGroupHierarchyBuilder blockGroupHierarchyBuilder,
                                          IThemeComponentListBuilder themeComponentListBuilder)
        {
            _blockTranslator = blockTranslator;
            _blockGroupHierarchyBuilder = blockGroupHierarchyBuilder;
            _themeComponentListBuilder = themeComponentListBuilder;
        }

        public ItemFilterScript TranslateStringToItemFilterScript(string inputString)
        {
            var script = new ItemFilterScript();
            _blockGroupHierarchyBuilder.Initialise(script.ItemFilterBlockGroups.First());

            inputString = inputString.Replace("\t", "");
            var conditionBoundaries = IdentifyBlockBoundaries(inputString);

            var lines = Regex.Split(inputString, "\r\n|\r|\n");

            // Process the script header
            for (var i = 0; i < conditionBoundaries.First.Value; i++)
            {
                if (lines[i].StartsWith("#"))
                {
                    script.Description += lines[i].Substring(1).Trim(' ') + Environment.NewLine;
                }    
            }

            if (!script.Description.IsNullOrEmpty())
            {
                script.Description = script.Description.TrimEnd('\n').TrimEnd('\r');
            }

            _themeComponentListBuilder.Initialise();

            // Extract each block from between boundaries and translate it into a ItemFilterBlock object
            // and add that object to the ItemFilterBlocks list 
            for (var boundary = conditionBoundaries.First; boundary != null; boundary = boundary.Next)
            {
                var begin = boundary.Value;
                var end = boundary.Next != null ? boundary.Next.Value : lines.Length;
                var block = new string[end - begin];
                Array.Copy(lines, begin, block, 0, end - begin);
                var blockString = string.Join("\r\n", block);
                script.ItemFilterBlocks.Add(_blockTranslator.TranslateStringToItemFilterBlock(blockString));
            }

            script.ThemeComponents = _themeComponentListBuilder.GetComponents();
            _blockGroupHierarchyBuilder.Cleanup();
            _themeComponentListBuilder.Cleanup();
            return script;
        }

        private static LinkedList<int> IdentifyBlockBoundaries(string inputString)
        {
            var blockBoundaries = new LinkedList<int>();
            var previousLine = string.Empty;
            var currentLine = 0;

            foreach (var line in new LineReader(() => new StringReader(inputString)))
            {
                currentLine++;
                var trimmedLine = line.TrimStart(' ').TrimEnd(' ');
                if (trimmedLine.StartsWith("Show") || trimmedLine.StartsWith("Hide") ||
                    trimmedLine.StartsWith("# Section:"))
                {
                    // If the line previous to the Show or Hide line is a comment then we should include that in the block
                    // as it represents the block description.
                    // currentLine > 2 caters for an edge case where the script description is a single line and the first
                    // block has no description. This prevents the script description from being assigned to the first block's description.
                    blockBoundaries.AddLast(previousLine.StartsWith("#") && !previousLine.StartsWith("# Section:") && currentLine > 2 ? currentLine - 2 : currentLine - 1);
                }
                previousLine = line;
            }

            return blockBoundaries;
        }

        public string TranslateItemFilterScriptToString(ItemFilterScript script)
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
                outputString += _blockTranslator.TranslateItemFilterBlockToString(block) + Environment.NewLine;

                if (Settings.Default.ExtraLineBetweenBlocks)
                {
                    outputString += Environment.NewLine;
                }
            }

            return outputString;
        }
    }
}
