using System;

namespace Filtration.ObjectModel.Commands.ItemFilterScript
{
    public class AddBlockCommand : IUndoableCommand
    {
        private readonly IItemFilterScript _itemFilterScript;
        private readonly IItemFilterBlockBase _addAfterItemFilterBlock;
        private IItemFilterBlock _newItemFilterBlock;

        public AddBlockCommand(IItemFilterScript itemFilterScript, IItemFilterBlockBase addAfterItemFilterBlock)
        {
            _itemFilterScript = itemFilterScript;
            _addAfterItemFilterBlock = addAfterItemFilterBlock;
        }
        public void Execute()
        {
            _newItemFilterBlock = new ItemFilterBlock(_itemFilterScript);
            if (_addAfterItemFilterBlock != null)
            {
                _itemFilterScript.ItemFilterBlocks.Insert(_itemFilterScript.ItemFilterBlocks.IndexOf(_addAfterItemFilterBlock) + 1, _newItemFilterBlock);
            }
            else
            {
                _itemFilterScript.ItemFilterBlocks.Add(_newItemFilterBlock);
            }

        }

        public void Undo()
        {
            _itemFilterScript.ItemFilterBlocks.Remove(_newItemFilterBlock);
        }

        public void Redo() => Execute();
    }
}
