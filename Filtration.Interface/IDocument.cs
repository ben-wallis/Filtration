using System.Threading.Tasks;

namespace Filtration.Interface
{
    public interface IDocument
    {
        bool IsScript { get; }
        bool IsTheme { get; }
        Task Close();
    }
}
