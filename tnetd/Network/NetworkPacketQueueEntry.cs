
// @Author : Arpan Jati
// @Date: 25th Dec 2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Network
{
    class NetworkPacketQueueEntry
    {
        public Hash PublicKey_Dest;
        public NetworkPacket Packet;
        public int TransmitAttempts;

        public NetworkPacketQueueEntry(Hash publicKey_Dest, NetworkPacket packet)
        {
            PublicKey_Dest = publicKey_Dest;
            Packet = packet;
            TransmitAttempts = 0;
        }
    };
}
