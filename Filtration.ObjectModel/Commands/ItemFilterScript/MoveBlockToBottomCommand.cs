namespace Filtration.ObjectModel.Commands.ItemFilterScript
{
    public class MoveBlockToBottomCommand : IUndoableCommand
    {
        private readonly IItemFilterScript _itemFilterScript;
        private readonly IItemFilterBlockBase _blockToMove;
        private int _indexMovedFrom;

        public MoveBlockToBottomCommand(IItemFilterScript itemFilterScript, IItemFilterBlockBase blockToMove)
        {
            _itemFilterScript = itemFilterScript;
            _blockToMove = blockToMove;
        }
        public void Execute()
        {
            _indexMovedFrom = _itemFilterScript.ItemFilterBlocks.IndexOf(_blockToMove);
            
            _itemFilterScript.ItemFilterBlocks.Remove(_blockToMove);
            _itemFilterScript.ItemFilterBlocks.Add(_blockToMove);
        }

        public void Undo()
        {
            _itemFilterScript.ItemFilterBlocks.Remove(_blockToMove);
            _itemFilterScript.ItemFilterBlocks.Insert(_indexMovedFrom, _blockToMove);
        }

        public void Redo() => Execute();
    }
}
