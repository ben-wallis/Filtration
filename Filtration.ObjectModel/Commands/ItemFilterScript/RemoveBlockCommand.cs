using System;

namespace Filtration.ObjectModel.Commands.ItemFilterScript
{
    public class RemoveBlockCommand : IUndoableCommand
    {
        private readonly IItemFilterScript _itemFilterScript;
        private IItemFilterBlockBase _removedItemFilterBlock;
        private int _indexRemovedFrom;

        public RemoveBlockCommand(IItemFilterScript itemFilterScript, IItemFilterBlockBase itemFilterBlockBase)
        {
            _itemFilterScript = itemFilterScript;
            _removedItemFilterBlock = itemFilterBlockBase;
        }
        public void Execute()
        {
            _indexRemovedFrom = _itemFilterScript.ItemFilterBlocks.IndexOf(_removedItemFilterBlock);
            _itemFilterScript.ItemFilterBlocks.Remove(_removedItemFilterBlock);
        }

        public void Undo()
        {
            _itemFilterScript.ItemFilterBlocks.Insert(_indexRemovedFrom, _removedItemFilterBlock);
        }

        public void Redo() => Execute();
    }
}
