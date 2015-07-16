﻿
//  @Author: Arpan Jati | Stephan Verbuecheln
//  @Date: June 2015 

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
using System.Reactive.Linq;
using System.Collections.Concurrent;

namespace TNetD.Consensus
{
    public enum ConsensusStates { Collect, Merge, Vote, Confirm, Apply };

    class Voting
    {
        bool Enabled { get; set; }

        private object VotingTransactionLock = new object();
        private object ConsensusLock = new object();

        public ConsensusStates CurrentConsensusState { get; private set; }

        NodeConfig nodeConfig;
        NodeState nodeState;
        NetworkPacketSwitch networkPacketSwitch;

        /// <summary>
        /// ID and content of current transactions
        /// </summary>
        ConcurrentDictionary<Hash, TransactionContent> CurrentTransactions;
        Ballot ballot;
        TransactionChecker tchecker;

        /// <summary>
        /// Set of nodes, who sent a transaction ID
        /// key: Transaction ID
        /// value: Set of nodes
        /// </summary>
        ConcurrentDictionary<Hash, HashSet<Hash>> propagationMap;


        public Voting(NodeConfig nodeConfig, NodeState nodeState, NetworkPacketSwitch networkPacketSwitch)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;
            this.networkPacketSwitch = networkPacketSwitch;
            this.CurrentTransactions = new ConcurrentDictionary<Hash, TransactionContent>();
            this.propagationMap = new ConcurrentDictionary<Hash, HashSet<Hash>>();
            networkPacketSwitch.VoteEvent += networkPacketSwitch_VoteEvent;
            networkPacketSwitch.VoteMergeEvent += networkPacketSwitch_VoteMergeEvent;
            TransactionChecker tcheck = new TransactionChecker(nodeState);

            CurrentConsensusState = ConsensusStates.Collect;

            Observable.Interval(TimeSpan.FromMilliseconds(1000))
                .Subscribe(async x => await TimerCallback_Voting(x));

        }

        private async Task TimerCallback_Voting(Object o)
        {
            if (Enabled)
            {
                await Task.Run(() =>
                {
                    lock (ConsensusLock)
                    {
                        switch (CurrentConsensusState)
                        {
                            case ConsensusStates.Collect:
                                ProcessPendingTransactions();
                                CurrentConsensusState = ConsensusStates.Merge;
                                break;

                            case ConsensusStates.Merge:
                                HandleMerge();
                                break;

                            case ConsensusStates.Vote:
                                HandleVoting();
                                break;

                            case ConsensusStates.Confirm:
                                HandleConfirmation();
                                break;

                            case ConsensusStates.Apply:
                                HandleVoting();
                                break;

                        }
                    }

                });
            }
        }

        int MergeStateCounter = 0;
        int VotingStateCounter = 0;
        int ConfirmationStateCounter = 0;

        void HandleMerge()
        {
            MergeStateCounter++;
            SendMergeRequests();

            // after 5 rounds: assemble ballot
            if (MergeStateCounter >= 5)
            {
                ballot = tchecker.CreateBallot(CurrentTransactions);
                CurrentConsensusState = ConsensusStates.Vote;
                MergeStateCounter = 0;
            }
        }



        void HandleVoting()
        {
            VotingStateCounter++;


        }

        void HandleConfirmation()
        {
            ConfirmationStateCounter++;


        }

        void HandleApply()
        {


            CurrentConsensusState = ConsensusStates.Apply;
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

        /// <summary>
        /// sends a request to get transaction data for all transaction IDs in the sorted hash
        /// </summary>
        /// <param name="node"></param>
        /// <param name="transactions"></param>
        void sendFetchRequest(Hash node, SortedSet<Hash> transactions)
        {
            FetchRequestMsg message = new FetchRequestMsg();
            message.IDs = transactions;
            Hash token = TNetUtils.GenerateNewToken();
            NetworkPacket packet = new NetworkPacket();
            packet.Data = message.Serialize();
            packet.Token = token;
            packet.PublicKeySource = nodeConfig.PublicKey;
            packet.Type = PacketType.TPT_CONS_TX_FETCH_REQUEST;
            networkPacketSwitch.AddToQueue(node, packet);
        }

        /// <summary>
        /// responds to a fetch request, sending all requested transaction data
        /// </summary>
        /// <param name="packet"></param>
        void ProcessFetchRequest(NetworkPacket packet)
        {
            FetchRequestMsg message = new FetchRequestMsg();
            message.Deserialize(packet.Data);

            // collect transaction data
            FetchResponseMsg response = new FetchResponseMsg();
            foreach (Hash id in message.IDs)
            {
                response.transactions.Add(id, CurrentTransactions[id]);
            }

            NetworkPacket rpacket = new NetworkPacket();
            rpacket.Data = response.Serialize();
            rpacket.Token = packet.Token;
            rpacket.PublicKeySource = nodeConfig.PublicKey;
            rpacket.Type = PacketType.TPT_CONS_TX_FETCH_RESPONSE;
            networkPacketSwitch.AddToQueue(packet.PublicKeySource, rpacket);
        }

        /// <summary>
        /// process the response to a fetch request, i.e. add the transactions to CurrentTransactions
        /// </summary>
        /// <param name="packet"></param>
        void ProcessFetchResponse(NetworkPacket packet)
        {
            if (networkPacketSwitch.VerifyPendingPacket(packet))
            {
                FetchResponseMsg message = new FetchResponseMsg();
                message.Deserialize(packet.Data);

                //check each transaction for signature validity and basic spendability
                foreach (KeyValuePair<Hash, TransactionContent> transaction in message.transactions)
                {
                    //check signature
                    if (transaction.Value.VerifySignature() == TransactionProcessingResult.Accepted) 
                    {
                        //check spendability
                        List<Hash> badaccounts = new List<Hash>();
                        if (tchecker.Spendable(transaction.Value, new Dictionary<Hash, long>(), out badaccounts))
                        {
                            CurrentTransactions.AddOrUpdate(transaction.Key, transaction.Value, (ok, ov) => ov);
                        }
                        else 
                        { 
                            //could blacklist accounts here although not necessary
                        }
                    }
                    else
                    {
                        //could blacklist peer for sending transaction with invalid signature
                    }
                }
            }
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

        /// <summary>
        /// respond to merge request by sending a list of all hashes of known (not expired) transactions
        /// </summary>
        /// <param name="packet"></param>
        void ProcessMergeResponse(NetworkPacket packet)
        {
            if (networkPacketSwitch.VerifyPendingPacket(packet))
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
                sendFetchRequest(packet.PublicKeySource, newTransactions);
            }
        }

        /// <summary>
        /// request a list of all known transactions from each connected validator
        /// </summary>
        void SendMergeRequests()
        {
            foreach (var node in nodeState.ConnectedValidators)
            {
                Hash token = TNetUtils.GenerateNewToken();
                NetworkPacket request = new NetworkPacket();
                request.PublicKeySource = nodeConfig.PublicKey;
                request.Token = token;
                request.Type = PacketType.TPT_CONS_MERGE_REQUEST;
                networkPacketSwitch.AddToQueue(node.Key, request);
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
                                    CurrentTransactions.TryAdd(kvp.Key, kvp.Value);
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
