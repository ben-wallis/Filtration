using System.Collections.Generic;

namespace Filtration.ObjectModel.Commands.ItemFilterScript
{
    public class MoveBlocksToTopCommand : IUndoableCommand
    {
        private readonly IItemFilterScript _itemFilterScript;
        private readonly List<int> _sourceIndexes;

        public MoveBlocksToTopCommand(IItemFilterScript itemFilterScript, IItemFilterBlockBase block)
        {
            _itemFilterScript = itemFilterScript;
            _sourceIndexes = new List<int> { _itemFilterScript.ItemFilterBlocks.IndexOf(block) };
        }

        public MoveBlocksToTopCommand(IItemFilterScript itemFilterScript, List<int> sourceIndexes)
        {
            _itemFilterScript = itemFilterScript;
            _sourceIndexes = sourceIndexes;
            _sourceIndexes.Sort();
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

            for (var i = 0; i < _sourceIndexes.Count; i++)
            {
                _itemFilterScript.ItemFilterBlocks.Insert(i, blocksToMove[i]);
            }
        }

        public void Undo()
        {
            List<IItemFilterBlockBase> blocksToMove = new List<IItemFilterBlockBase>();
            for (var i = 0; i < _sourceIndexes.Count; i++)
            {
                blocksToMove.Add(_itemFilterScript.ItemFilterBlocks[0]);
                _itemFilterScript.ItemFilterBlocks.RemoveAt(0);
            }

            for (var i = 0; i < _sourceIndexes.Count; i++)
            {
                _itemFilterScript.ItemFilterBlocks.Insert(_sourceIndexes[i], blocksToMove[i]);
            }
        }

        public void Redo() => Execute();
    }
}