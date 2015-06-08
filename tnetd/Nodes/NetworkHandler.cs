﻿
// @Author : Arpan Jati
// @Date: 13th Feb 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Network;
using TNetD.Network.Networking;
using TNetD.Time;

namespace TNetD.Nodes
{
    class NetworkHandler
    {
        public delegate void TimeSyncEventHandler(NetworkPacket packet);
        public event TimeSyncEventHandler TimeSyncEvent;

        NodeConfig nodeConfig;
        NodeState nodeState;
        GlobalConfiguration globalConfiguration;

        SecureNetwork network = default(SecureNetwork);

        TimeSync timeSync = default(TimeSync);

        public NetworkHandler(NodeConfig nodeConfig, NodeState nodeState, GlobalConfiguration globalConfiguration)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;
            this.globalConfiguration = globalConfiguration;
            
            network = new SecureNetwork(nodeConfig);
            network.PacketReceived += network_PacketReceived;

            network.Initialize();

        }

        async public Task InitialConnectAsync()
        {
            await Task.Run(async () =>
            {
                // Connect to TrustedNodes
                List<Task> tasks = new List<Task>();

                foreach (KeyValuePair<Hash, NodeSocketData> kvp in globalConfiguration.TrustedNodes)
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
            DisplayUtils.Display(" Packet: " + packet.Type + " | From: " + packet.PublicKeySource + " | Data Length : " + packet.Data.Length);
                       
            switch (packet.Type)
            {
                case PacketType.TPT_TRANS_REQUEST:
                case PacketType.TPT_TRANS_FORWARDING:
                    
                    break;

                case PacketType.TPT_CONS_STATE:
                case PacketType.TPT_CONS_CURRENT_SET:
                case PacketType.TPT_CONS_REQUEST_TC_TX:
                case PacketType.TPT_CONS_RESP_TC_TX:
                case PacketType.TPT_CONS_VOTES:
                case PacketType.TPT_CONS_DOUBLESPENDERS:
                case PacketType.TPT_LSYNC_FETCH_ROOT:
                case PacketType.TPT_LSYNC_FETCH_LAYER_DATA:
                case PacketType.TPT_LSYNC_REPLY_ROOT:
                case PacketType.TPT_LSYNC_REPLY_LAYER_DATA:

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

                    if (TimeSyncEvent != null) TimeSyncEvent(packet);

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



    }
}
