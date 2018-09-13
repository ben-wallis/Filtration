using System;
using System.Collections.Generic;

namespace Filtration.ObjectModel.Commands.ItemFilterScript
{
    public class PasteBlocksCommand : IUndoableCommand
    {
        private readonly IItemFilterScript _itemFilterScript;
        private readonly List<IItemFilterBlockBase> _pastedItemFilterBlocks;
        private readonly IItemFilterBlockBase _addAfterItemFilterBlock;

        public PasteBlocksCommand(IItemFilterScript itemFilterScript, IItemFilterBlockBase block, IItemFilterBlockBase addAfterItemFilterBlock)
        {
            _itemFilterScript = itemFilterScript;
            _pastedItemFilterBlocks = new List<IItemFilterBlockBase> { block };
            _addAfterItemFilterBlock = addAfterItemFilterBlock;
        }

        public PasteBlocksCommand(IItemFilterScript itemFilterScript, List<IItemFilterBlockBase> pastedItemFilterBlocks, IItemFilterBlockBase addAfterItemFilterBlock)
        {
            _itemFilterScript = itemFilterScript;
            _pastedItemFilterBlocks = pastedItemFilterBlocks;
            _addAfterItemFilterBlock = addAfterItemFilterBlock;
        }

        public void Execute()
        {
            if (_addAfterItemFilterBlock != null)
            {
                var lastAddedBlock = _addAfterItemFilterBlock;
                foreach (var block in _pastedItemFilterBlocks)
                {
                    _itemFilterScript.ItemFilterBlocks.Insert(_itemFilterScript.ItemFilterBlocks.IndexOf(lastAddedBlock) + 1, block);
                    lastAddedBlock = block;
                }
            }
            else
            {
                foreach (var block in _pastedItemFilterBlocks)
                {
                    _itemFilterScript.ItemFilterBlocks.Add(block);
                }
            }
        }

        public void Undo()
        {
            foreach (var block in _pastedItemFilterBlocks)
            {
                _itemFilterScript.ItemFilterBlocks.Remove(block);
            }
        }

        public void Redo() => Execute();
    }
}