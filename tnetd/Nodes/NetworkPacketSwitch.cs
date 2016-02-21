
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
using TNetD.Tests;
using TNetD.Time;

namespace TNetD.Nodes
{
    class NetworkPacketSwitch
    {
        public delegate void NetworkPacketEventHandler(NetworkPacket packet);
        public delegate Task AsyncNetworkPacketEventHandler(NetworkPacket packet);

        public event NetworkPacketEventHandler LedgerSyncEvent;
        public event NetworkPacketEventHandler PeerDiscoveryEvent;

        public event AsyncNetworkPacketEventHandler ConsensusEvent;
        public event AsyncNetworkPacketEventHandler TimeSyncEvent;
        
        //public PacketLogger packetLogger = default(PacketLogger);

        NodeConfig nodeConfig;
        NodeState nodeState;

        SecureNetwork network = default(SecureNetwork);

        // Cound number of packages per sender in a particular interval (ms)
        private NodeRating noderating;

        public NetworkPacketSwitch(NodeConfig nodeConfig, NodeState nodeState)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;

            network = new SecureNetwork(nodeConfig, nodeState);
            network.PacketReceived += network_PacketReceived;

            network.Initialize();

            noderating = new NodeRating(nodeState);

            //packetLogger = new PacketLogger(nodeConfig, nodeState);
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
            await Task.Delay(Common.NORMAL_RNG.Next(500, 1000)); // Wait a random delay before connecting.
            NetworkPacketQueueEntry npqe = new NetworkPacketQueueEntry(publicKey,
                new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_HELLO, new byte[0]));

            network.AddToQueue(npqe);
        }

        /// <summary>
        /// This will switch packets whichever it should go.
        /// </summary>
        /// <param name="packet"></param>
        async Task network_PacketReceived(NetworkPacket packet)
        {
            //DisplayUtils.Display(" Packet: " + packet.Type + " | From: " + packet.PublicKeySource + " | Data Length : " + packet.Data.Length);

            noderating.TrackPacket(packet);
            //packetLogger.LogReceive(packet);

            switch (packet.Type)
            {
                case PacketType.TPT_CONS_MERGE_REQUEST:
                case PacketType.TPT_CONS_MERGE_RESPONSE:
                case PacketType.TPT_CONS_MERGE_TX_FETCH_REQUEST:
                case PacketType.TPT_CONS_MERGE_TX_FETCH_RESPONSE:
                case PacketType.TPT_CONS_SYNC_REQUEST:
                case PacketType.TPT_CONS_SYNC_RESPONSE:
                case PacketType.TPT_CONS_VOTE_REQUEST:
                case PacketType.TPT_CONS_VOTE_RESPONSE:
                case PacketType.TPT_CONS_CONFIRM_REQUEST:
                case PacketType.TPT_CONS_CONFIRM_RESPONSE:

                    if (ConsensusEvent != null)
                        await ConsensusEvent.Invoke(packet).ConfigureAwait(false);

                    break;

                case PacketType.TPT_LSYNC_ROOT_REQUEST:
                case PacketType.TPT_LSYNC_ROOT_RESPONSE:
                case PacketType.TPT_LSYNC_NODE_REQUEST:
                case PacketType.TPT_LSYNC_NODE_RESPONSE:
                case PacketType.TPT_LSYNC_LEAF_REQUEST:
                case PacketType.TPT_LSYNC_LEAF_REQUEST_ALL:
                case PacketType.TPT_LSYNC_LEAF_RESPONSE:

                    LedgerSyncEvent?.Invoke(packet);

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

                    TimeSyncEvent?.Invoke(packet);

                    break;

                case PacketType.TPT_PEER_DISCOVERY_REQUEST:
                case PacketType.TPT_PEER_DISCOVERY_RESPONSE:

                    PeerDiscoveryEvent?.Invoke(packet);

                    break;
            }
        }

        public NetworkResult AddToQueue(Hash publicKeyDestination, NetworkPacket packet)
        {
            return network.AddToQueue(new NetworkPacketQueueEntry(publicKeyDestination, packet));
        }

        public async Task SendAsync(Hash publicKeyDestination, NetworkPacket packet)
        {
            //packetLogger.LogSend(publicKeyDestination, packet,1);

            await network.SendAsync(new NetworkPacketQueueEntry(publicKeyDestination, packet));
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

            if (!good)
            {
                nodeState.logger.Log(LogType.Network, "Dropped packet from " + packet.PublicKeySource + ": Unrequested response.");
            }

            return good;
        }








    }
}
