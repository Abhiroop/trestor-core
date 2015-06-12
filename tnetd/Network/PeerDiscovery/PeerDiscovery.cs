using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Timers;
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
        private Timer timer;

        public PeerDiscovery(NodeState nodeState, NodeConfig nodeConfig, NetworkPacketSwitch networkPacketSwitch)
        {
            this.nodeState = nodeState;
            this.nodeConfig = nodeConfig;
            this.networkPacketSwitch = networkPacketSwitch;
            requestRecipient = null;
            requestToken = null;
            KnownPeers = new ConcurrentDictionary<Hash, byte[]>();
            networkPacketSwitch.PeerDiscoveryEvent += networkHandler_PeerDiscoveryEvent;

            // maybe replace by better rng, but no crypo here anyway
            rng = new Random();
        }


        private void Print(String message)
        {
            DisplayUtils.Display(" Node " + nodeConfig.NodeID + " | PrDscvry: " + message);
        }




        public void Start(int interval)
        {
            timer = new Timer();
            timer.Interval = interval;
            timer.Elapsed += initiatePeerDiscovery;
            timer.Enabled = true;
            timer.Start();
        }

        /// <summary>
        /// Initiate gossip-style peer-discovery protocol with a node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void initiatePeerDiscovery(object sender, ElapsedEventArgs e)
        {
            int count = nodeState.ConnectedValidators.Count;
            if (count <= 0)
                return;
            int select = rng.Next(count);
            Print("init: selecting " + select + " of " + count);
            Hash peer = nodeState.ConnectedValidators.ToArray()[select];
            Hash token = TNetUtils.GenerateNewToken();

            //save locally
            requestRecipient = peer;
            requestToken = token;

            //send message
            PeerDiscoveryMsg request = new PeerDiscoveryMsg();
            request.knownPeers = KnownPeers;
            byte[] message = request.Serialize();
            NetworkPacket packet = new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_PEER_DISCOVERY_INIT, message, token);
            networkPacketSwitch.AddToQueue(peer, packet);
        }

        public void networkHandler_PeerDiscoveryEvent(NetworkPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.TPT_PEER_DISCOVERY_INIT:
                    respondPeerDiscovery(packet);
                    return;
                case PacketType.TPT_PEER_DISCOVERY_RESPONSE:
                    processPeerDiscovery(packet);
                    return;
            }
        }

        private void respondPeerDiscovery(NetworkPacket packet)
        {
            PeerDiscoveryMsg request = new PeerDiscoveryMsg();
            request.Deserialize(packet.Data);

            // process incoming peer list
            processNewPeerList(request.knownPeers);

            // send message with own peer list
            Hash token = packet.Token;
            Hash peer = packet.PublicKeySource;
            PeerDiscoveryMsg response = new PeerDiscoveryMsg();
            response.knownPeers = KnownPeers;
            byte[] message = response.Serialize();
            NetworkPacket newpacket = new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_PEER_DISCOVERY_RESPONSE, message, token);
            networkPacketSwitch.AddToQueue(peer, newpacket);
        }

        private void processPeerDiscovery(NetworkPacket packet)
        {
            if (packet.Token == requestToken)
            {
                PeerDiscoveryMsg response = new PeerDiscoveryMsg();
                response.Deserialize(packet.Data);
                processNewPeerList(response.knownPeers);
            }
            else
            {
                Print("toke mismatch");
            }
        }

        /// <summary>
        /// Will merge a new peer list into the existing one
        /// </summary>
        /// <param name="knownPeers"></param>
        private void processNewPeerList(ConcurrentDictionary<Hash, byte[]> knownPeers)
        {
            int oldcount = KnownPeers.Count;
            foreach (KeyValuePair<Hash, byte[]> peer in knownPeers)
            {
                KnownPeers.AddOrUpdate(peer.Key, peer.Value, (ok, ov) => peer.Value);
            }
            int newcount = KnownPeers.Count;
            Print("processing peer list: " + oldcount + " => " + newcount); 
        }
    }
}
