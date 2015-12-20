using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel
{
    public class Socket
    {
        public Socket(SocketColor color)
        {
            Color = color;
        }

        public SocketColor Color { get; }
    }
}