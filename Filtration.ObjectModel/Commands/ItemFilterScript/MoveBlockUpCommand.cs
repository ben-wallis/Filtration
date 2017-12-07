using System;

namespace Filtration.ObjectModel.Commands.ItemFilterScript
{
    public class MoveBlockUpCommand : IUndoableCommand
    {
        private readonly IItemFilterScript _itemFilterScript;
        private readonly IItemFilterBlockBase _blockToMove;
        private int _indexMovedFrom;

        public MoveBlockUpCommand(IItemFilterScript itemFilterScript, IItemFilterBlockBase blockToMove)
        {
            _itemFilterScript = itemFilterScript;
            _blockToMove = blockToMove;
        }

        public void Execute()
        {
            _indexMovedFrom = _itemFilterScript.ItemFilterBlocks.IndexOf(_blockToMove);

            if (_indexMovedFrom <= 0)
            {
                throw new InvalidOperationException("Cannot move the top block up");
            }

            _itemFilterScript.ItemFilterBlocks.Remove(_blockToMove);
            _itemFilterScript.ItemFilterBlocks.Insert(_indexMovedFrom-1, _blockToMove);

        }

        public void Undo()
        {
            _itemFilterScript.ItemFilterBlocks.Remove(_blockToMove);
            _itemFilterScript.ItemFilterBlocks.Insert(_indexMovedFrom, _blockToMove);
        }

        public void Redo() => Execute();
    }
}