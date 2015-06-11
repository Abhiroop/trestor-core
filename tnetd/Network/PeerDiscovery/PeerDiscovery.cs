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
        private NodeConfig nodeConfig;
        private NetworkPacketSwitch networkPacketSwitch;
        private Random rng;

        public PeerDiscovery(NodeState nodeState, NodeConfig nodeConfig, NetworkPacketSwitch networkPacketSwitch)
        {
            this.nodeState = nodeState;
            this.nodeConfig = nodeConfig;
            this.networkPacketSwitch = networkPacketSwitch;
            requestRecipient = null;
            requestToken = null;
            KnownPeers = new ConcurrentDictionary<Hash, byte[]>();

            // maybe replace by better rng, but no crypo here
            rng = new Random();
        }

        public void Start(int interval)
        {

        }

        /// <summary>
        /// Initiate gossip with a node
        /// </summary>
        /// <param name="node"></param>
        private void InitiateGossip(Hash node)
        {
            int count = nodeState.ConnectedValidators.Count;
            Hash peer = nodeState.ConnectedValidators.ToArray()[rng.Next(count)];
            Hash token = TNetUtils.GenerateNewToken();

            //save locally
            requestRecipient = peer;
            requestToken = token;

            //send message
            PDRespondGossip request = new PDRespondGossip();
            request.knownPeers = KnownPeers;
            byte[] message = request.Serialize();
            NetworkPacket packet = new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_TIMESYNC_REQUEST, message, token);
            networkPacketSwitch.AddToQueue(peer, packet);
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
            Hash token = packet.Token;
            PDRespondGossip request = new PDRespondGossip();
            request.Deserialize(packet.Data);

            processNewPeerList(request.knownPeers);
        }

        private void processGossipResponse(NetworkPacket packet)
        {
            if (packet.Token == requestToken)
            {
                PDRespondGossip response = new PDRespondGossip();
                response.Deserialize(packet.Data);
                processNewPeerList(response.knownPeers);
            }
        }

        private void processNewPeerList(ConcurrentDictionary<Hash, byte[]> knownPeers)
        {
            foreach (KeyValuePair<Hash, byte[]> peer in knownPeers)
            {
                KnownPeers.AddOrUpdate(peer.Key, peer.Value, (ok, ov) => peer.Value);
            }
        }
    }
}
