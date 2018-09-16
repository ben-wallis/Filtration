using System;
using System.Collections.Generic;
using System.Linq;

namespace Filtration.ObjectModel.Commands.ItemFilterScript
{
    public class RemoveBlocksCommand : IUndoableCommand
    {
        private readonly IItemFilterScript _itemFilterScript;
        private List<IItemFilterBlockBase> _removedItemFilterBlocks;
        private List<int> _sourceIndexes;

        public RemoveBlocksCommand(IItemFilterScript itemFilterScript, IItemFilterBlockBase block)
        {
            _itemFilterScript = itemFilterScript;
            _sourceIndexes = new List<int> { _itemFilterScript.ItemFilterBlocks.IndexOf(block) };
            _removedItemFilterBlocks = new List<IItemFilterBlockBase> { block };
        }

        public RemoveBlocksCommand(IItemFilterScript itemFilterScript, List<int> sourceIndexes)
        {
            _itemFilterScript = itemFilterScript;
            _sourceIndexes = sourceIndexes;
            _sourceIndexes.Sort();
            _removedItemFilterBlocks = new List<IItemFilterBlockBase>();
            for (var i = 0; i < _sourceIndexes.Count; i++)
            {
                _removedItemFilterBlocks.Add(_itemFilterScript.ItemFilterBlocks[_sourceIndexes[i]]);
            }
        }

        public void Execute()
        {
            for (var i = _sourceIndexes.Count - 1; i >= 0; i--)
            {
                _itemFilterScript.ItemFilterBlocks.RemoveAt(_sourceIndexes[i]);
            }
        }

        public void Undo()
        {
            for (var i = 0; i < _sourceIndexes.Count; i++)
            {
                _itemFilterScript.ItemFilterBlocks.Insert(_sourceIndexes[i], _removedItemFilterBlocks[i]);
            }
        }

        public void Redo() => Execute();
    }
}
