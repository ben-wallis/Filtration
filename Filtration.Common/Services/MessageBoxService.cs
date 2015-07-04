using System.Windows;

namespace Filtration.Common.Services
{
    public interface IMessageBoxService
    {
        MessageBoxResult Show(string caption, string message, MessageBoxButton buttons, MessageBoxImage image);
    }

    public class MessageBoxService : IMessageBoxService
    {
        public MessageBoxResult Show(string caption, string message, MessageBoxButton buttons, MessageBoxImage image)
        {
            return MessageBox.Show(message, caption, buttons, image);
        }
    }
}
