
// @Author : Arpan Jati
// @Date: 25th Dec 2014 + 8 June 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Network
{
    class NetworkPacketQueueEntry
    {
        public Hash PublicKeyDestination;
        public NetworkPacket Packet;
        public int TransmitAttempts;

        public NetworkPacketQueueEntry(Hash publicKeyDestination, NetworkPacket packet)
        {
            PublicKeyDestination = publicKeyDestination;
            Packet = packet;
            TransmitAttempts = 0;
        }
    };
}
