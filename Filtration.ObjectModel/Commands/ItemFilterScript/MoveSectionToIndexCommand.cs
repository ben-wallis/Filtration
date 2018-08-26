using System.Collections.Generic;

namespace Filtration.ObjectModel.Commands.ItemFilterScript
{
    public class MoveSectionToIndexCommand : IUndoableCommand
    {
        private readonly IItemFilterScript _itemFilterScript;
        private int _sectionStart;
        private int _sectionSize;
        private int _index;

        public MoveSectionToIndexCommand(IItemFilterScript itemFilterScript, int sectionStart, int sectionSize, int index)
        {
            _itemFilterScript = itemFilterScript;
            _sectionStart = sectionStart;
            _sectionSize = sectionSize;
            _index = index;
        }
        public void Execute()
        {
            List<IItemFilterBlockBase> blocksToMove = new List<IItemFilterBlockBase>();
            for(var i = 0; i < _sectionSize; i++)
            {
                blocksToMove.Add(_itemFilterScript.ItemFilterBlocks[_sectionStart]);
                _itemFilterScript.ItemFilterBlocks.RemoveAt(_sectionStart);
            }
            for (var i = 0; i < _sectionSize; i++)
            {
                _itemFilterScript.ItemFilterBlocks.Insert(_index + i, blocksToMove[i]);
            }
        }

        public void Undo()
        {
            List<IItemFilterBlockBase> blocksToMove = new List<IItemFilterBlockBase>();
            for (var i = 0; i < _sectionSize; i++)
            {
                blocksToMove.Add(_itemFilterScript.ItemFilterBlocks[_index]);
                _itemFilterScript.ItemFilterBlocks.RemoveAt(_index);
            }
            for (var i = 0; i < _sectionSize; i++)
            {
                _itemFilterScript.ItemFilterBlocks.Insert(_sectionStart + i, blocksToMove[i]);
            }
        }

        public void Redo() => Execute();
    }
}