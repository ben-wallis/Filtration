namespace Filtration.Interface
{
    public interface IEditableDocument : IDocument
    {
        bool IsDirty { get; }
        void Save();
        void SaveAs();
    }
}
