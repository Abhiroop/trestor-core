
// @Author : Arpan Jati
// @Date: Jan 2015 | September 2015

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TNetD.Nodes;

namespace TNetD.Network.Networking
{
    public enum ConnectionDirection { Outgoing, Incoming };

    class SecureNetwork
    {
        private object SecureNetworkTimerLock = new object();

        public event PacketReceivedHandler PacketReceived;

        //Timer updateTimer;
        Timer connectionUpdateTimer;
        NodeConfig nodeConfig = default(NodeConfig);
        NodeState nodeState = default(NodeState);

        ConcurrentQueue<NetworkPacketQueueEntry> outgoingPacketQueue = new ConcurrentQueue<NetworkPacketQueueEntry>();

        IncomingConnectionHander incomingConnectionHander = default(IncomingConnectionHander);

        NetworkMessagePairs messageTypePairs;

        /// <summary>
        /// Collection of OutgoingConnections, Key is the PublicKey.
        /// </summary>
        Dictionary<Hash, OutgoingConnection> outgoingConnections = new Dictionary<Hash, OutgoingConnection>();

        public SecureNetwork(NodeConfig nodeConfig, NodeState nodeState)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;

            messageTypePairs = new NetworkMessagePairs();
        }

        public void Initialize()
        {
            incomingConnectionHander = new IncomingConnectionHander(nodeConfig.NetworkConfig.ListenPort, nodeConfig);

            incomingConnectionHander.PacketReceived += process_PacketReceived;

            Observable.Interval(TimeSpan.FromMilliseconds(Constants.Network_UpdateFrequencyMS))
               .Subscribe(async x => await TimerCallback(x));

            //updateTimer = new Timer(TimerCallback, null, 0, nodeConfig.NetworkConfig.UpdateFrequencyMS);

            connectionUpdateTimer = new Timer(ConnectionTimerCallback, null, 0, Constants.Network_ConnectionUpdateFrequencyMS);
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

        private SemaphoreSlim syncLock = new SemaphoreSlim(1);

        private async Task TimerCallback(object o)
        {
            try
            {
                await syncLock.WaitAsync();

                // Check outgoing queue for new packet requests.
                while (outgoingPacketQueue.Count > 0)
                {
                    NetworkPacketQueueEntry npqe;
                    if (outgoingPacketQueue.TryDequeue(out npqe))
                    {
                        await SendAsync(npqe);
                    }
                }
            }
            catch (System.Exception ex)
            {
                DisplayUtils.Display("SecureNetwork.TimerCallback()", ex);
            }
            finally
            {
                syncLock.Release();
            }
        }
        
        public async Task SendAsync(NetworkPacketQueueEntry npqe)
        {
            bool success = true;

            if (outgoingConnections.ContainsKey(npqe.PublicKeyDestination)) // Already Connected (outgoing), Just send a packet.
            {
                OutgoingConnection conn = outgoingConnections[npqe.PublicKeyDestination];

                if (conn.KeyExchanged)
                {
                    await outgoingConnections[npqe.PublicKeyDestination].SendAsync(npqe.Packet);
                }
                else
                {
                    outgoingConnections[npqe.PublicKeyDestination].EnqueuePacket(npqe.Packet);
                }
            }

            else if (incomingConnectionHander.IsConnected(npqe.PublicKeyDestination))  // Already Connected (incoming), Just send a packet.
            {
                await incomingConnectionHander.SendAsync(npqe);
            }

            else // Create a new outgoing connection and queue a packet.
            {
                if ((nodeConfig.TrustedNodes.ContainsKey(npqe.PublicKeyDestination)) &&
                    (npqe.PublicKeyDestination != nodeConfig.PublicKey))
                {
                    var socketInfo = nodeConfig.TrustedNodes[npqe.PublicKeyDestination];

                    OutgoingConnection oc = new OutgoingConnection(socketInfo, nodeConfig);
                    oc.PacketReceived += process_PacketReceived;

                    oc.EnqueuePacket(npqe.Packet);

                    outgoingConnections.Add(npqe.PublicKeyDestination, oc);
                }
                else
                {
                    // The public key is not described / no-connection information. 
                    // Fetch information from other nodes and try again.

                    success = false;

                    DisplayUtils.Display("Could not find " + npqe.PublicKeyDestination.ToString());
                }
            }

            if (success)
                NPQEPostTxOperations(npqe);
        }
        
        private void NPQEPostTxOperations(NetworkPacketQueueEntry npqe)
        {
            Interlocked.Increment(ref nodeState.NodeInfo.NodeDetails.NetworkPacketsOut);

            if (npqe.Packet.Token.Hex.Length == Common.NETWORK_TOKEN_LENGTH)
            {
                if (messageTypePairs.Pairs.ContainsKey(npqe.Packet.Type))
                {
                    PendingNetworkRequest pnr = new PendingNetworkRequest(nodeState.SystemTime, npqe.PublicKeyDestination,
                        messageTypePairs.Pairs[npqe.Packet.Type]);

                    nodeState.PendingNetworkRequests.AddOrUpdate(npqe.Packet.Token, pnr, (k, v) => pnr);
                }
                else
                {
                    if (!messageTypePairs.Responses.Contains(npqe.Packet.Type))
                    {
                        DisplayUtils.Display("Non-Pair packet with valid Token length.", DisplayType.CodeAssertionFailed);
                    }
                }
            }
            else
            {
                if (npqe.Packet.Token.Hex.Length != 0)
                {
                    DisplayUtils.Display("Bad token Length.", DisplayType.CodeAssertionFailed);
                    nodeState.logger.Log(LogType.Network, "Invalid packet from " + npqe.Packet.PublicKeySource + ": Bad token Length");
                }
            }
        }
        
        public NetworkResult AddToQueue(NetworkPacketQueueEntry npqe)
        {
            outgoingPacketQueue.Enqueue(npqe);

            return NetworkResult.Queued;
        }      

        private void ConnectionTimerCallback(Object o)
        {
            try
            {
                // Maintain / Fix Outgoing connection lists.

                HashSet<Hash> toRemove = new HashSet<Hash>();

                foreach (KeyValuePair<Hash, OutgoingConnection> kvp in outgoingConnections)
                {
                    if (kvp.Value.Ended) // Remove killed connections
                    {
                        toRemove.Add(kvp.Key);
                    }
                }

                foreach (Hash hsh in toRemove)
                {
                    outgoingConnections.Remove(hsh);
                }

                foreach (KeyValuePair<Hash, OutgoingConnection> kvp in outgoingConnections)
                {
                    if (kvp.Value.KeyExchanged)
                    {
                        if (!nodeState.ConnectedValidators.ContainsKey(kvp.Value.nodeSocketData.PublicKey))
                        {
                            nodeState.ConnectedValidators.TryAdd(kvp.Value.nodeSocketData.PublicKey, new ConnectionProperties(ConnectionDirection.Outgoing,
                                nodeConfig.TrustedNodes.ContainsKey(kvp.Value.nodeSocketData.PublicKey)));
                        }
                    }
                }

                foreach (KeyValuePair<Hash, IncomingClient> kvp in incomingConnectionHander.IncomingConnections)
                {
                    if (kvp.Value.KeyExchanged)
                    {
                        if (!nodeState.ConnectedValidators.ContainsKey(kvp.Value.PublicKey))
                        {
                            nodeState.ConnectedValidators.TryAdd(kvp.Value.PublicKey, new ConnectionProperties(ConnectionDirection.Incoming,
                                nodeConfig.TrustedNodes.ContainsKey(kvp.Value.PublicKey)));
                        }
                    }
                }

                toRemove.Clear();

                foreach (var conn in nodeState.ConnectedValidators)
                {
                    bool exists = false;

                    if (incomingConnectionHander.IsConnected(conn.Key)) exists = true;

                    if (outgoingConnections.ContainsKey(conn.Key)) exists = true;

                    if (!exists) toRemove.Add(conn.Key);
                }

                foreach (Hash pk in toRemove)
                {
                    ConnectionProperties cp;
                    nodeState.ConnectedValidators.TryRemove(pk, out cp);
                }
            }
            catch (System.Exception ex)
            {
                DisplayUtils.Display("SecureNetwork.ConnectionTimerCallback()", ex);
            }
        }

        async Task process_PacketReceived(NetworkPacket packet)
        {
            if (PacketReceived != null) // Relay the packet to the outer event.
            {
                Interlocked.Increment(ref nodeState.NodeInfo.NodeDetails.NetworkPacketsIn);
                await PacketReceived(packet);
            }
        }
    }
}
