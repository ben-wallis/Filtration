using System.Collections.Generic;

namespace Filtration.ObjectModel.Commands
{
    public interface ICommandManager
    {
        void ExecuteCommand(ICommand command);
        void Undo(int undoLevels = 1);
        void Redo(int redoLevels = 1);
    }

    public interface ICommandManagerInternal : ICommandManager
    {
        void SetScript(IItemFilterScriptInternal layout);
    }

    internal class CommandManager : ICommandManagerInternal
    {
        private readonly Stack<IUndoableCommand> _undoCommandStack = new Stack<IUndoableCommand>();
        private readonly Stack<IUndoableCommand> _redoCommandStack = new Stack<IUndoableCommand>();
        private IItemFilterScriptInternal _itemFilterScript;

        public void SetScript(IItemFilterScriptInternal itemFilterScript)
        {
            _itemFilterScript = itemFilterScript;
        }

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            if (command is IUndoableCommand undoableCommand)
            {
                _undoCommandStack.Push(undoableCommand);
                _redoCommandStack.Clear();
            }
            _itemFilterScript.SetIsDirty(true);
        }

        public void Undo(int undoLevels = 1)
        {
            for (var index = undoLevels; _undoCommandStack.Count > 0 && index > 0; --index)
            {
                var undoableCommand = _undoCommandStack.Pop();
                undoableCommand.Undo();
                _redoCommandStack.Push(undoableCommand);
            }
            _itemFilterScript.SetIsDirty(true);
        }

        public void Redo(int redoLevels = 1)
        {
            for (int index = redoLevels; _redoCommandStack.Count > 0 && index > 0; --index)
            {
                var undoableCommand = _redoCommandStack.Pop();
                undoableCommand.Redo();
                _undoCommandStack.Push(undoableCommand);
            }
            _itemFilterScript.SetIsDirty(true);
        }
    }
}
