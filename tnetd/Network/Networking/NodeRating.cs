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

        public NodeRating(NodeState nodeState)
        {
            this.nodeState = nodeState;

            currentData = new ConcurrentDictionary<Hash, int>();
            currentMsgs = new ConcurrentDictionary<Hash, int>();
            nodeMsgHistory = new ConcurrentDictionary<Hash, ConcurrentQueue<int>>();
            nodeDataHistory = new ConcurrentDictionary<Hash, ConcurrentQueue<int>>();

            foreach (Hash node in nodeState.ConnectedValidators.Keys)
            {
                ConcurrentQueue<int> msgQ = new ConcurrentQueue<int>();
                nodeMsgHistory.AddOrUpdate(node, msgQ, (k, v) => msgQ);

                ConcurrentQueue<int> dataQ = new ConcurrentQueue<int>();
                nodeDataHistory.AddOrUpdate(node, dataQ, (k, v) => dataQ);
            }

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
            currentData[packet.PublicKeySource] += packet.Data.Length;
            currentMsgs[packet.PublicKeySource] += 1;
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

        /// <summary>
        /// process current counters and enqueue results
        /// </summary>
        private void processQueue(object sender, ElapsedEventArgs e)
        {
            foreach (Hash node in nodeState.ConnectedValidators.Keys)
            {
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
            foreach (Hash node in nodeState.ConnectedValidators.Keys)
            {
                // TODO: crop queues to length
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
