
// Arpan Jati
// 12th July 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Network.Networking
{
    public struct ConnectionProperties
    {
        public ConnectionDirection Direction;
        public bool IsTrusted;
        public ConnectionProperties(ConnectionDirection Direction, bool IsTrusted)
        {
            this.Direction = Direction;
            this.IsTrusted = IsTrusted;
        }
    }
}
