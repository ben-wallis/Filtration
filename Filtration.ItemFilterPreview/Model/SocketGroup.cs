using System;
using System.Collections.Generic;

namespace Filtration.ItemFilterPreview.Model
{
    public class SocketGroup : List<Socket>
    {
        public SocketGroup(List<Socket> sockets, bool linked)
        {
            if (sockets.Count < 1 || sockets.Count > 6)
            {
                throw new InvalidOperationException("A socket group must have between 2 and 6 sockets");
            }


            if (linked && sockets.Count < 2)
            {
                throw new InvalidOperationException("A linked socket group must have at least 2 sockets");
            }

            AddRange(sockets);
            Linked = linked;
        }

        public bool Linked { get; }
    }
}