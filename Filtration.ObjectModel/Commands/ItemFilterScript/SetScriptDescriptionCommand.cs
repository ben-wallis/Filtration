using System;

namespace Filtration.ObjectModel.Commands.ItemFilterScript
{
    public class SetScriptDescriptionCommand : IUndoableCommand
    {
        private readonly IItemFilterScript _itemFilterScript;
        private readonly string _newDescription;
        private string _oldDescription;

        public SetScriptDescriptionCommand(IItemFilterScript itemFilterScript, string newDescription)
        {
            _itemFilterScript = itemFilterScript;
            _newDescription = newDescription;
        }

        public void Execute()
        {
            _oldDescription = _itemFilterScript.Description;
            _itemFilterScript.Description = _newDescription;
        }

        public void Undo()
        {
            _itemFilterScript.Description = _oldDescription;
        }

        public void Redo() => Execute();
    }
}
