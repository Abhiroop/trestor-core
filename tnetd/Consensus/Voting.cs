using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TNetD.Network.Networking;
using TNetD.Nodes;
using TNetD.Transactions;
using TNetD.Network;

namespace TNetD.Consensus
{
    class Voting
    {
        private object VotingTransactionLock = new object();

        NodeConfig nodeConfig;
        NodeState nodeState;
        NetworkPacketSwitch networkPacketSwitch;

        /// <summary>
        /// ID and content of current transactions
        /// </summary>
        Dictionary<Hash, TransactionContent> CurrentTransactions;

        /// <summary>
        /// Maps nodes on tokes used for merge requests
        /// key: Node
        /// value: Token
        /// </summary>
        Dictionary<Hash, Hash> mergeTokens;

        /// <summary>
        /// Set of nodes, who sent a transaction ID
        /// key: Transaction ID
        /// value: Set of nodes
        /// </summary>
        Dictionary<Hash, HashSet<Hash>> propagationMap;



        public Voting(NodeConfig nodeConfig, NodeState nodeState, NetworkPacketSwitch networkPacketSwitch)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;
            this.networkPacketSwitch = networkPacketSwitch;
            this.CurrentTransactions = new Dictionary<Hash, TransactionContent>();
            this.mergeTokens = new Dictionary<Hash, Hash>();
            propagationMap = new Dictionary<Hash, HashSet<Hash>>();
            networkPacketSwitch.VoteEvent += networkPacketSwitch_VoteEvent;
            networkPacketSwitch.VoteMergeEvent += networkPacketSwitch_VoteMergeEvent;
        }

        void networkPacketSwitch_VoteMergeEvent(NetworkPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.TPT_CONS_MERGE_REQUEST:
                    ProcessMergeRequest(packet);
                    break;
                case PacketType.TPT_CONS_MERGE_RESPONSE:
                    ProcessMergeResponse(packet);
                    break;
                case PacketType.TPT_CONS_TX_FETCH_REQUEST:
                    ProcessFetchRequest(packet);
                    break;
                case PacketType.TPT_CONS_TX_FETCH_RESPONSE:
                    ProcessFetchResponse(packet);
                    break;
            }
        }

        void sendFetchRequest(Hash node, Hash transaction)
        {

        }

        void ProcessFetchRequest(NetworkPacket packet)
        {

        }

        void ProcessFetchResponse(NetworkPacket packet)
        {

        }

        void ProcessMergeRequest(NetworkPacket packet)
        {
            Hash sender = packet.PublicKeySource;
            Hash token = packet.Token;

            MergeResponseMsg message = new MergeResponseMsg();
            foreach (KeyValuePair<Hash, TransactionContent> transaction in CurrentTransactions)
            {
                message.transactions.Add(transaction.Key);
            }

            NetworkPacket response = new NetworkPacket();
            response.Token = token;
            response.PublicKeySource = nodeConfig.PublicKey;
            response.Data = message.Serialize();
            response.Type = PacketType.TPT_CONS_MERGE_RESPONSE;
            networkPacketSwitch.AddToQueue(sender, response);
        }

        void ProcessMergeResponse(NetworkPacket packet)
        {
            MergeResponseMsg message = new MergeResponseMsg();
            message.Deserialize(packet.Data);
            SortedSet<Hash> newTransactions = new SortedSet<Hash>();

            foreach (Hash transaction in message.transactions)
            {
                //check whether transaction for the given ID is already known
                if (!CurrentTransactions.ContainsKey(transaction))
                {
                    newTransactions.Add(transaction);
                }
                //add sender to propagationMap
                propagationMap[transaction].Add(packet.PublicKeySource);
            }


            //TODO: request content for new transactions, check and add
        }

        void SendMergeRequests()
        {
            foreach (Hash node in nodeState.ConnectedValidators)
            {
                Hash token = TNetUtils.GenerateNewToken();
                NetworkPacket request = new NetworkPacket();
                request.PublicKeySource = nodeConfig.PublicKey;
                request.Token = token;
                request.Type = PacketType.TPT_CONS_MERGE_REQUEST;
                networkPacketSwitch.AddToQueue(node, request);
            }
        }

        void networkPacketSwitch_VoteEvent(Network.NetworkPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.TPT_CONS_STATE:
                    break;

                case PacketType.TPT_CONS_BALLOT_REQUEST:
                    break;

                case PacketType.TPT_CONS_BALLOT_RESPONSE:
                    break;

                case PacketType.TPT_CONS_BALLOT_AGREE_REQUEST:
                    break;

                case PacketType.TPT_CONS_BALLOT_AGREE_RESPONSE:
                    break;
            }
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
