using System;
using System.Collections.Generic;
using System.Linq;

namespace Filtration.ObjectModel.Commands.ItemFilterScript
{
    public class MoveBlocksToIndexCommand : IUndoableCommand
    {
        private readonly IItemFilterScript _itemFilterScript;
        private List<int> _sourceIndexes;
        private int _targetIndex;

        public MoveBlocksToIndexCommand(IItemFilterScript itemFilterScript, IItemFilterBlockBase block, int targetIndex)
        {
            _itemFilterScript = itemFilterScript;
            _sourceIndexes = new List<int> { _itemFilterScript.ItemFilterBlocks.IndexOf(block) };
            _targetIndex = targetIndex;
        }

        public MoveBlocksToIndexCommand(IItemFilterScript itemFilterScript, List<int> sourceIndexes, int targetIndex)
        {
            _itemFilterScript = itemFilterScript;
            _sourceIndexes = sourceIndexes;
            _sourceIndexes.Sort();
            _targetIndex = targetIndex;
        }

        public void Execute()
        {
            List<IItemFilterBlockBase> blocksToMove = new List<IItemFilterBlockBase>();
            for (var i = 0; i < _sourceIndexes.Count; i++)
            {
                blocksToMove.Add(_itemFilterScript.ItemFilterBlocks[_sourceIndexes[i]]);
            }

            for (var i = _sourceIndexes.Count - 1; i >= 0; i--)
            {
                _itemFilterScript.ItemFilterBlocks.RemoveAt(_sourceIndexes[i]);
            }

            for (var i = 0; i < blocksToMove.Count; i++)
            {
                _itemFilterScript.ItemFilterBlocks.Insert(_targetIndex + i, blocksToMove[i]);
            }
        }

        public void Undo()
        {
            List<IItemFilterBlockBase> blocksToMove = new List<IItemFilterBlockBase>();
            for (var i = 0; i < _sourceIndexes.Count; i++)
            {
                blocksToMove.Add(_itemFilterScript.ItemFilterBlocks[_targetIndex]);
                _itemFilterScript.ItemFilterBlocks.RemoveAt(_targetIndex);
            }

            for (var i = 0; i < _sourceIndexes.Count; i++)
            {
                _itemFilterScript.ItemFilterBlocks.Insert(_sourceIndexes[i], blocksToMove[i]);
            }
        }

        public void Redo() => Execute();
    }
}