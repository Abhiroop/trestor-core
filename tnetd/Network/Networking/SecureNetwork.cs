﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TNetD.Nodes;

namespace TNetD.Network.Networking
{
    class SecureNetwork
    {
        public event PacketReceivedHandler PacketReceived;

        Timer updateTimer;
        NodeConfig nodeConfig;

        ConcurrentQueue<NetworkPacketQueueEntry> outgoingPacketQueue = new ConcurrentQueue<NetworkPacketQueueEntry>();

        IncomingConnectionHander incomingConnectionHander = default(IncomingConnectionHander);

        /// <summary>
        /// Collection of OutgoingConnections, Key is the PublicKey.
        /// </summary>
        Dictionary<Hash, OutgoingConnection> outgoingConnections = new Dictionary<Hash, OutgoingConnection>();

        public SecureNetwork(NodeConfig nodeConfig)
        {
            this.nodeConfig = nodeConfig;
        }

        public void Initialize()
        {
            incomingConnectionHander = new IncomingConnectionHander(nodeConfig.NetworkConfig.ListenPort, nodeConfig);

            updateTimer = new Timer(TimerCallback, null, 0, nodeConfig.NetworkConfig.UpdateFrequencyMS);
        }

        public bool IsConnected(Hash PublicKey)
        {
            if (incomingConnectionHander.IsConnected(PublicKey))
            {
                return true;
            }

            if (outgoingConnections.ContainsKey(PublicKey))
                return true;

            return false;
        }

        public void Stop()
        {
            incomingConnectionHander.StopAndExit();
        }

        private void TimerCallback(Object o)
        {
            try
            {
                // Check outgoing queue for new packet requests.

                while (outgoingPacketQueue.Count > 0)
                {
                    NetworkPacketQueueEntry npqe;
                    if (outgoingPacketQueue.TryDequeue(out npqe))
                    {
                        if (outgoingConnections.ContainsKey(npqe.PublicKey_Dest)) // Already Connected (outgoing), Just send a packet.
                        {
                            outgoingConnections[npqe.PublicKey_Dest].EnqueuePacket(npqe.Packet);
                        }

                        else if (incomingConnectionHander.IsConnected(npqe.PublicKey_Dest))  // Already Connected (incoming), Just send a packet.
                        {
                            incomingConnectionHander.EnqueuePacket(npqe);
                        }

                        else // Create a new outgoing connection and queue a packet.
                        {
                            if (nodeConfig.GlobalConfiguration.TrustedNodes.ContainsKey(npqe.PublicKey_Dest))
                            {
                                var socketInfo = nodeConfig.GlobalConfiguration.TrustedNodes[npqe.PublicKey_Dest];

                                OutgoingConnection oc = new OutgoingConnection(socketInfo);
                                oc.PacketReceived += oc_PacketReceived;

                                oc.EnqueuePacket(npqe.Packet);

                                outgoingConnections.Add(npqe.PublicKey_Dest, oc);
                            }
                            else
                            {
                                // The public key is not described / no-connection information. 
                                // Fetch information from other nodes and try again.

                                DisplayUtils.Display("Could not find " + npqe.PublicKey_Dest.ToString());
                            }
                        }
                    }
                }

                // Maintain / Fix Outgoing connection lists.

                HashSet<Hash> toRemove = new HashSet<Hash>();

                foreach (KeyValuePair<Hash, OutgoingConnection> kvp in outgoingConnections)
                {
                    if (kvp.Value.ThreadKilled) // Remove killed connections
                    {
                        toRemove.Add(kvp.Key);
                    }
                }

                foreach (Hash hsh in toRemove)
                {
                    outgoingConnections.Remove(hsh);
                }

            }
            catch (System.Exception ex)
            {
                DisplayUtils.Display("SecureNetwork.TimerCallback()", ex);
            }
        }

        void oc_PacketReceived(Hash PublicKey, byte[] Data)
        {
            if (PacketReceived != null) // Relay the packet to the outer event.
                PacketReceived(PublicKey, Data);
        }

        public NetworkResult AddToQueue(NetworkPacketQueueEntry npqe)
        {
            outgoingPacketQueue.Enqueue(npqe);

            return NetworkResult.Queued;
        }


    }
}