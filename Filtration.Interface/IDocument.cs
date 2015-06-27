namespace Filtration.Interface
{
    public interface IDocument
    {
        bool IsScript { get; }
        bool IsTheme { get; }
        void Close();
    }
}
