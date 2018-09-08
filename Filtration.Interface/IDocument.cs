using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.Interface
{
    public interface IDocument
    {
        bool IsScript { get; }
        bool IsTheme { get; }
        Task Close();
        RelayCommand CloseCommand { get; }
    }
}
