namespace Filtration.ObjectModel.Commands
{
    internal interface IUndoableCommand : ICommand
    {
        void Undo();
        void Redo();
    }
}
