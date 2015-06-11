using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Network;
using TNetD.Network.Networking;
using TNetD.Nodes;

namespace TNetD.Network.PeerDiscovery
{
    class PeerDiscovery
    {
        // TODO: add more properties like IP address, byte[] can be changed
        public ConcurrentDictionary<Hash, byte[]> KnownPeers { get; set; }

        private Hash requestRecipient;
        private Hash requestToken;

        private NodeState nodeState;
        private Random rng;

        public PeerDiscovery(NodeState nodeState)
        {
            this.nodeState = nodeState;
            requestRecipient = null;
            requestToken = null;
            KnownPeers = new ConcurrentDictionary<Hash, byte[]>();
            // maybe replace by better rng
            rng = new Random();
        }

        /// <summary>
        /// Initiate gossip with a node
        /// </summary>
        /// <param name="node"></param>
        public void InitiateGossip(Hash node)
        {
            int count = nodeState.ConnectedValidators.Count;
            int i = rng.Next(count);

            PDRespondGossip message = new PDRespondGossip(KnownPeers);

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
