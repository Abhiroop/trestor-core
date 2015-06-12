﻿using System;
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

            // maybe replace by better rng, but no crypo here
            rng = new Random();
        }


        private void Print(String message)
        {
            DisplayUtils.Display(" Node " + nodeConfig.NodeID + " | PrDscvry: " + message);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        public void Start(int interval)
        {
            timer = new Timer();
            timer.Interval = interval;
            timer.Elapsed += initiatePeerDiscovery;
            timer.Enabled = true;
            timer.Start();
            Print("timer started");
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
            Print("init: count " + count + " select " + select);
            Hash peer = nodeState.ConnectedValidators.First<Hash>();// ToArray()[select];
            Hash token = TNetUtils.GenerateNewToken();

            //save locally
            requestRecipient = peer;
            requestToken = token;

            //send message
            PeerDiscoveryMsg request = new PeerDiscoveryMsg();
            request.knownPeers = KnownPeers;
            byte[] message = request.Serialize();
            NetworkPacket packet = new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_TIMESYNC_REQUEST, message, token);
            //networkPacketSwitch.AddToQueue(peer, packet);
        }

        public void PeerDiscoveryMsgHandler(NetworkPacket packet)
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
            Print("request arrived");
            Hash token = packet.Token;
            PeerDiscoveryMsg request = new PeerDiscoveryMsg();
            request.Deserialize(packet.Data);

            processNewPeerList(request.knownPeers);
        }

        private void processPeerDiscovery(NetworkPacket packet)
        {
            if (packet.Token == requestToken)
            {
                PeerDiscoveryMsg response = new PeerDiscoveryMsg();
                response.Deserialize(packet.Data);
                processNewPeerList(response.knownPeers);
            }
        }

        private void processNewPeerList(ConcurrentDictionary<Hash, byte[]> knownPeers)
        {
            int oldcount = knownPeers.Count;
            foreach (KeyValuePair<Hash, byte[]> peer in knownPeers)
            {
                KnownPeers.AddOrUpdate(peer.Key, peer.Value, (ok, ov) => peer.Value);
            }
            int newcount = knownPeers.Count;
            Print("processing peer list: " + oldcount + " => " + newcount); 
        }
    }
}
