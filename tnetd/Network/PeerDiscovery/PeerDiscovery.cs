using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Network;
using TNetD.Network.Networking;

namespace TNetD.Network.PeerDiscovery
{
    class PeerDiscovery
    {

        public PeerDiscovery()
        {

        }

        public void InitiateGossip(Hash node)
        {

        }

        public void GossipMsgHandler(NetworkPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.TPT_GOSSIP_INIT:
                    respondToGossip(packet);
                    return;
                case PacketType.TPT_GOSSIP_RESPONSE:
                    processGossipResponse(packet);
                    return;
            }
        }

        private void respondToGossip(NetworkPacket packet)
        {

        }

        private void processGossipResponse(NetworkPacket packet)
        {

        }
    }
}
