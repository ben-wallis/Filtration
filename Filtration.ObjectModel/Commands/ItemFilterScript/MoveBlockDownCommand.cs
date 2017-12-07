using System;

namespace Filtration.ObjectModel.Commands.ItemFilterScript
{
    public class MoveBlockDownCommand : IUndoableCommand
    {
        private readonly IItemFilterScript _itemFilterScript;
        private readonly IItemFilterBlockBase _blockToMove;
        private int _indexMovedFrom;

        public MoveBlockDownCommand(IItemFilterScript itemFilterScript, IItemFilterBlockBase blockToMove)
        {
            _itemFilterScript = itemFilterScript;
            _blockToMove = blockToMove;
        }

        public void Execute()
        {
            _indexMovedFrom = _itemFilterScript.ItemFilterBlocks.IndexOf(_blockToMove);

            if (_indexMovedFrom >= _itemFilterScript.ItemFilterBlocks.Count)
            {
                throw new InvalidOperationException("Cannot move the bottom block down");
            }

            _itemFilterScript.ItemFilterBlocks.Remove(_blockToMove);
            _itemFilterScript.ItemFilterBlocks.Insert(_indexMovedFrom + 1, _blockToMove);

        }

        public void Undo()
        {
            _itemFilterScript.ItemFilterBlocks.Remove(_blockToMove);
            _itemFilterScript.ItemFilterBlocks.Insert(_indexMovedFrom, _blockToMove);
        }

        public void Redo() => Execute();
    }
}