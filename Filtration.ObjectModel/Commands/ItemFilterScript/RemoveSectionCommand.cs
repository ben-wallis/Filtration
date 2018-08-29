using System;
using System.Collections.Generic;

namespace Filtration.ObjectModel.Commands.ItemFilterScript
{
    public class RemoveSectionCommand : IUndoableCommand
    {
        private readonly IItemFilterScript _itemFilterScript;
        private List<IItemFilterBlockBase> _removedItemFilterBlocks;
        private int _sectionStart;
        private int _sectionSize;

        public RemoveSectionCommand(IItemFilterScript itemFilterScript, int sectionStart, int sectionSize)
        {
            _itemFilterScript = itemFilterScript;
            _sectionStart = sectionStart;
            _sectionSize = sectionSize;
            _removedItemFilterBlocks = new List<IItemFilterBlockBase>();
            for(var i = 0; i < _sectionSize; i++)
            {
                _removedItemFilterBlocks.Add(_itemFilterScript.ItemFilterBlocks[_sectionStart + i]);
            }
        }
        public void Execute()
        {
            for (var i = 0; i < _sectionSize; i++)
            {
                _itemFilterScript.ItemFilterBlocks.RemoveAt(_sectionStart);
            }
        }

        public void Undo()
        {
            for (var i = 0; i < _sectionSize; i++)
            {
                _itemFilterScript.ItemFilterBlocks.Insert(_sectionStart + i, _removedItemFilterBlocks[i]);
            }
        }

        public void Redo() => Execute();
    }
}
