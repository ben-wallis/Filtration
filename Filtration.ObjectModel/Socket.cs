using System;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel
{
    [Serializable]
    public class Socket
    {
        private Socket()
        {
        }

        public Socket(SocketColor color)
        {
            Color = color;
        }

        public SocketColor Color { get; }
    }
}