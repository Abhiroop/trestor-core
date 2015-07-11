
// @Author : Arpan Jati
// @Date: 8th June 2015 | 25 June 2015

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Network;
using TNetD.Network.Networking;
using TNetD.Time;

namespace TNetD.Nodes
{
    class NetworkPacketSwitch
    {
        public delegate void NetworkPacketEventHandler(NetworkPacket packet);
        public event NetworkPacketEventHandler TimeSyncEvent;
        public event NetworkPacketEventHandler LedgerSyncEvent;
        public event NetworkPacketEventHandler PeerDiscoveryEvent;
        public event NetworkPacketEventHandler VoteMergeEvent;
        public event NetworkPacketEventHandler VoteEvent;

        NodeConfig nodeConfig;
        NodeState nodeState;

        SecureNetwork network = default(SecureNetwork);

        public NetworkPacketSwitch(NodeConfig nodeConfig, NodeState nodeState)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;

            network = new SecureNetwork(nodeConfig, nodeState);
            network.PacketReceived += network_PacketReceived;

            network.Initialize();
        }

        public Hash[] GetConnectedNodes(ConnectionListType type)
        {
            return network.GetConnectedNodes(type);
        }

        async public Task InitialConnectAsync()
        {
            await Task.Run(async () =>
            {
                // Connect to TrustedNodes
                List<Task> tasks = new List<Task>();

                foreach (KeyValuePair<Hash, NodeSocketData> kvp in nodeConfig.TrustedNodes)
                {
                    // Make sure we are not connecting to self !!
                    if (kvp.Key != nodeConfig.PublicKey)
                    {
                        if (!network.IsConnected(kvp.Key))
                        {
                            tasks.Add(SendInitialize(kvp.Key));
                        }
                    }
                }

                await Task.WhenAll(tasks);

            });
        }

        async Task SendInitialize(Hash publicKey)
        {
            await Task.Delay(Common.random.Next(500, 1000)); // Wait a random delay before connecting.
            NetworkPacketQueueEntry npqe = new NetworkPacketQueueEntry(publicKey,
                new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_HELLO, new byte[0]));

            network.AddToQueue(npqe);
        }

        /// <summary>
        /// This will switch packets whichever it should go.
        /// </summary>
        /// <param name="packet"></param>
        void network_PacketReceived(NetworkPacket packet)
        {
            //DisplayUtils.Display(" Packet: " + packet.Type + " | From: " + packet.PublicKeySource + " | Data Length : " + packet.Data.Length);

            switch (packet.Type)
            {

                case PacketType.TPT_CONS_MERGE_REQUEST:
                case PacketType.TPT_CONS_MERGE_RESPONSE:

                    if (VoteMergeEvent != null) VoteMergeEvent(packet);

                    break;

                case PacketType.TPT_CONS_STATE:
                case PacketType.TPT_CONS_BALLOT_REQUEST:
                case PacketType.TPT_CONS_BALLOT_RESPONSE:
                case PacketType.TPT_CONS_BALLOT_AGREE_REQUEST:
                case PacketType.TPT_CONS_BALLOT_AGREE_RESPONSE:

                    if (VoteEvent != null) VoteEvent(packet);

                    break;

                case PacketType.TPT_LSYNC_ROOT_REQUEST:
                case PacketType.TPT_LSYNC_ROOT_RESPONSE:
                case PacketType.TPT_LSYNC_NODE_REQUEST:
                case PacketType.TPT_LSYNC_NODE_RESPONSE:
                case PacketType.TPT_LSYNC_LEAF_REQUEST:
                case PacketType.TPT_LSYNC_LEAF_REQUEST_ALL:
                case PacketType.TPT_LSYNC_LEAF_RESPONSE:

                    if (LedgerSyncEvent != null) LedgerSyncEvent(packet);

                    break;

                case PacketType.TPT_TX_SYNC_FETCH_REQUEST:
                case PacketType.TPT_TX_SYNC_FETCH_RESPONSE:
                case PacketType.TPT_TX_SYNC_ID_REQUEST:
                case PacketType.TPT_TX_SYNC_ID_RESPONSE:
                case PacketType.TPT_TX_SYNC_QUERY_REQUEST:
                case PacketType.TPT_TX_SYNC_QUERY_RESPONSE:
                case PacketType.TPT_TX_SYNC_CLOSEHISTORY_REQUEST:
                case PacketType.TPT_TX_SYNC_CLOSEHISTORY_RESPONSE:

                    break;

                case PacketType.TPT_TIMESYNC_REQUEST:
                case PacketType.TPT_TIMESYNC_RESPONSE:

                    if (TimeSyncEvent != null)
                        TimeSyncEvent(packet);

                    break;

                case PacketType.TPT_PEER_DISCOVERY_INIT:
                case PacketType.TPT_PEER_DISCOVERY_RESPONSE:

                    if (PeerDiscoveryEvent != null)
                        PeerDiscoveryEvent(packet);

                    break;

            }

        }

        public NetworkResult AddToQueue(NetworkPacketQueueEntry npqe)
        {
            return network.AddToQueue(npqe);
        }

        public NetworkResult AddToQueue(Hash publicKeyDestination, NetworkPacket packet)
        {
            return network.AddToQueue(publicKeyDestination, packet);
        }

        public void Stop()
        {
            network.Stop();
        }

        /// <summary>
        /// Checks if a packet reply was indeed requested. If not, the packet should be dropped.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool VerifyPendingPacket(NetworkPacket packet)
        {
            bool good = false;

            if (nodeState.PendingNetworkRequests.ContainsKey(packet.Token))
            {
                PendingNetworkRequest pnr = nodeState.PendingNetworkRequests[packet.Token];

                if ((pnr.PublicKey == packet.PublicKeySource) && (pnr.ResponseType == packet.Type))
                {
                    good = true;
                }

                PendingNetworkRequest tmp;
                nodeState.PendingNetworkRequests.TryRemove(packet.Token, out tmp);
            }

            return good;
        }



    }
}
