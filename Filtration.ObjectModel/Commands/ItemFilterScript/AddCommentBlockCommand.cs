using System;

namespace Filtration.ObjectModel.Commands.ItemFilterScript
{
    public class AddCommentBlockCommand : IUndoableCommand
    {
        private readonly IItemFilterScript _itemFilterScript;
        private readonly IItemFilterBlockBase _addAfterItemFilterBlock;
        private IItemFilterCommentBlock _newItemFilterBlock;

        public AddCommentBlockCommand(IItemFilterScript itemFilterScript, IItemFilterBlockBase addAfterItemFilterBlock)
        {
            _itemFilterScript = itemFilterScript;
            _addAfterItemFilterBlock = addAfterItemFilterBlock;
        }
        public void Execute()
        {
            _newItemFilterBlock = new ItemFilterCommentBlock(_itemFilterScript)
            {
                Comment = string.Empty
            };
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
