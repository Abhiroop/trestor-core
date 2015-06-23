using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TNetD.Nodes;
using TNetD.Transactions;

namespace TNetD.Consensus
{
    class Voting
    {
         private object VotingTransactionLock = new object();

        NodeConfig nodeConfig;
        NodeState nodeState;
        NetworkPacketSwitch networkPacketSwitch;

        Dictionary<Hash, TransactionContent> CurrentTransactions = new Dictionary<Hash, TransactionContent>();

        public Voting(NodeConfig nodeConfig, NodeState nodeState, NetworkPacketSwitch networkPacketSwitch)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;
            this.networkPacketSwitch = networkPacketSwitch;
            networkPacketSwitch.VoteEvent += networkPacketSwitch_VoteEvent;
            networkPacketSwitch.VoteMergeEvent += networkPacketSwitch_VoteMergeEvent;
        }
        
        void networkPacketSwitch_VoteMergeEvent(Network.NetworkPacket packet)
        {

        }

        void networkPacketSwitch_VoteEvent(Network.NetworkPacket packet)
        {

        }

        public void ProcessPendingTransactions()
        {
            lock (VotingTransactionLock)
            {

                try
                {
                    if (nodeState.IncomingTransactionMap.IncomingTransactions.Count > 0)
                    {

                        lock (nodeState.IncomingTransactionMap.transactionLock)
                        {
                            foreach (KeyValuePair<Hash, TransactionContent> kvp in nodeState.IncomingTransactionMap.IncomingTransactions)
                            {
                                //transactionContentStack.Enqueue(kvp.Value);
                                if (!CurrentTransactions.ContainsKey(kvp.Key))
                                {
                                    CurrentTransactions.Add(kvp.Key, kvp.Value);
                                }
                                //Interlocked.Increment(ref nodeState.NodeInfo.NodeDetails.TransactionsVerified);
                            }

                            nodeState.IncomingTransactionMap.IncomingTransactions.Clear();
                            nodeState.IncomingTransactionMap.IncomingPropagations_ALL.Clear();
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }




    }
}
