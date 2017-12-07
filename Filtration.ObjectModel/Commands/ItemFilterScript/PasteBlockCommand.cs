using System;

namespace Filtration.ObjectModel.Commands.ItemFilterScript
{
    public class PasteBlockCommand : IUndoableCommand
    {
        private readonly IItemFilterScript _itemFilterScript;
        private readonly IItemFilterBlockBase _pastedItemFilterBlock;
        private readonly IItemFilterBlockBase _addAfterItemFilterBlock;

        public PasteBlockCommand(IItemFilterScript itemFilterScript, IItemFilterBlockBase pastedItemFilterBlock, IItemFilterBlockBase addAfterItemFilterBlock)
        {
            _itemFilterScript = itemFilterScript;
            _pastedItemFilterBlock = pastedItemFilterBlock;
            _addAfterItemFilterBlock = addAfterItemFilterBlock;
        }

        public void Execute()
        {
            if (_addAfterItemFilterBlock != null)
            {
                _itemFilterScript.ItemFilterBlocks.Insert(_itemFilterScript.ItemFilterBlocks.IndexOf(_addAfterItemFilterBlock) + 1, _pastedItemFilterBlock);
            }
            else
            {
                _itemFilterScript.ItemFilterBlocks.Add(_pastedItemFilterBlock);
            }
        }

        public void Undo()
        {
            _itemFilterScript.ItemFilterBlocks.Remove(_pastedItemFilterBlock);
        }

        public void Redo() => Execute();
    }
}
