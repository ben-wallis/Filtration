using System.Threading.Tasks;

namespace Filtration.Interface
{
    public interface IEditableDocument : IDocument
    {
        bool IsDirty { get; }
        Task SaveAsync();
        Task SaveAsAsync();
    }
}
