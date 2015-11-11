using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Nodes;
using System.Timers;


namespace TNetD.Network.Networking
{
    class NodeRating
    {

        private ConcurrentDictionary<Hash, ConcurrentQueue<int>> nodeMsgHistory;
        private ConcurrentDictionary<Hash, ConcurrentQueue<int>> nodeDataHistory;

        private ConcurrentDictionary<Hash, int> currentMsgs;
        private ConcurrentDictionary<Hash, int> currentData;

        NodeState nodeState;

        Timer updateTimer;

        //thresholds of packets/bytes per second
        int msgthreshold = 1000;
        int datathreshold = 4096;
        int queuelength = 10;

        public NodeRating(NodeState nodeState)
        {
            this.nodeState = nodeState;

            currentData = new ConcurrentDictionary<Hash, int>();
            currentMsgs = new ConcurrentDictionary<Hash, int>();
            nodeMsgHistory = new ConcurrentDictionary<Hash, ConcurrentQueue<int>>();
            nodeDataHistory = new ConcurrentDictionary<Hash, ConcurrentQueue<int>>();

            updateTimer = new Timer();
            updateTimer.Interval = 1000;
            updateTimer.Elapsed += processQueue;
            updateTimer.Enabled = true;
            updateTimer.Start();
        }

        /// <summary>
        /// count incoming packet and its data length
        /// </summary>
        /// <param name="packet"></param>
        public void TrackPacket(NetworkPacket packet)
        {
            currentData.AddOrUpdate(packet.PublicKeySource, packet.Data.Length, (k, v) => v + packet.Data.Length);
            currentMsgs.AddOrUpdate(packet.PublicKeySource, 1, (k, v) => v + 1);
        }

        /// <summary>
        /// returns the number of messages sent by "node"
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int GetNodeMsgCount(Hash node)
        {
            ConcurrentQueue<int> queue = nodeMsgHistory[node];
            return queue.Sum();
        }

        /// <summary>
        /// returns the amount of data sent by "node" (in bytes)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int GetNodeData(Hash node)
        {
            ConcurrentQueue<int> queue = nodeDataHistory[node];
            return queue.Sum();
        }



        private void processQueue(object sender, ElapsedEventArgs e)
        {
            foreach (Hash node in nodeState.ConnectedValidators.Keys)
            {
                if (!nodeMsgHistory.ContainsKey(node))
                {
                    ConcurrentQueue<int> msgQ = new ConcurrentQueue<int>();
                    nodeMsgHistory.AddOrUpdate(node, msgQ, (k, v) => msgQ);
                }
                if (!nodeDataHistory.ContainsKey(node))
                {
                    ConcurrentQueue<int> dataQ = new ConcurrentQueue<int>();
                    nodeDataHistory.AddOrUpdate(node, dataQ, (k, v) => dataQ);
                }

                if (currentMsgs.ContainsKey(node))
                {
                    nodeMsgHistory[node].Enqueue(currentMsgs[node]);
                }
                else
                {
                    nodeMsgHistory[node].Enqueue(0);
                }

                if (currentData.ContainsKey(node))
                {
                    nodeDataHistory[node].Enqueue(currentData[node]);
                }
                else
                {
                    nodeDataHistory[node].Enqueue(0);
                }
            }

            foreach (Hash node in nodeMsgHistory.Keys)
            {
                if (!nodeState.ConnectedValidators.Keys.Contains<Hash>(node))
                {
                    ConcurrentQueue<int> q;
                    nodeMsgHistory.TryRemove(node, out q);
                }
            }

            foreach (Hash node in nodeDataHistory.Keys)
            {
                if (!nodeState.ConnectedValidators.Keys.Contains<Hash>(node))
                {
                    ConcurrentQueue<int> q;
                    nodeDataHistory.TryRemove(node, out q);
                }
            }

            foreach (Hash node in nodeState.ConnectedValidators.Keys)
            {
                // crop queue to length of 10
                while (nodeMsgHistory.ContainsKey(node) && nodeMsgHistory[node].Count > queuelength)
                {
                    int i;
                    nodeMsgHistory[node].TryDequeue(out i);
                }
                while (nodeDataHistory.ContainsKey(node) && nodeDataHistory[node].Count > queuelength)
                {
                    int i;
                    nodeDataHistory[node].TryDequeue(out i);
                }

                //compute  results and compare with thresholds
                int sum;
                sum = nodeMsgHistory[node].Sum();
                if (sum > msgthreshold)
                {
                    nodeState.logger.Log(LogType.Network, "WARNING: More than " + msgthreshold
                        + " packets from " + node + " received in " + queuelength + " s.");
                }
                sum = nodeDataHistory[node].Sum();
                if (sum > datathreshold)
                {
                    nodeState.logger.Log(LogType.Network, "WARNING: More than " + datathreshold
                        + " bytes from " + node + " received in " + queuelength + " s.");
                }
            }
        }


        private bool isRequest(NetworkPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.TPT_CONS_CONFIRM_REQUEST:
                case PacketType.TPT_CONS_MERGE_REQUEST:
                case PacketType.TPT_CONS_MERGE_TX_FETCH_REQUEST:
                case PacketType.TPT_CONS_SYNC_REQUEST:
                case PacketType.TPT_CONS_VOTE_REQUEST:
                case PacketType.TPT_LSYNC_LEAF_REQUEST:
                case PacketType.TPT_LSYNC_LEAF_REQUEST_ALL:
                case PacketType.TPT_LSYNC_NODE_REQUEST:
                case PacketType.TPT_LSYNC_ROOT_REQUEST:
                case PacketType.TPT_TIMESYNC_REQUEST:
                case PacketType.TPT_TX_SYNC_CLOSEHISTORY_REQUEST:
                case PacketType.TPT_TX_SYNC_FETCH_REQUEST:
                case PacketType.TPT_TX_SYNC_ID_REQUEST:
                case PacketType.TPT_TX_SYNC_QUERY_REQUEST:
                case PacketType.TPT_PEER_DISCOVERY_REQUEST:
                    return true;
            }
            return false;
        }
    }
}
