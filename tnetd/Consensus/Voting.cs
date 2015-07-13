
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
        SortedSet<Hash> mergedTransactions = new SortedSet<Hash>();
        
        /// <summary>
        /// Set of nodes, who sent a transaction ID
        /// key: Transaction ID
        /// value: Set of nodes
        /// </summary>
        ConcurrentDictionary<Hash, HashSet<Hash>> propagationMap;

        DoubleSpendBlacklist blacklist;
        
        public Voting(NodeConfig nodeConfig, NodeState nodeState, NetworkPacketSwitch networkPacketSwitch)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;
            this.networkPacketSwitch = networkPacketSwitch;
            this.CurrentTransactions = new ConcurrentDictionary<Hash, TransactionContent>();
            this.propagationMap = new ConcurrentDictionary<Hash, HashSet<Hash>>();
            this.blacklist = new DoubleSpendBlacklist(nodeState);
            networkPacketSwitch.VoteEvent += networkPacketSwitch_VoteEvent;
            networkPacketSwitch.VoteMergeEvent += networkPacketSwitch_VoteMergeEvent;

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
            blacklist.ClearExpired();
            MergeStateCounter++;
            SendMergeRequests();

            if (MergeStateCounter == 5)
            {
                Dictionary<Hash, long> temporaryBalances = new Dictionary<Hash, long>();
                foreach (KeyValuePair<Hash, TransactionContent> transaction in CurrentTransactions)
                {
                    List<Hash> badaccounts;
                    if (Spendable(transaction.Value, temporaryBalances, out badaccounts))
                    {
                        //update temporary balances
                        foreach (TransactionEntity te in CurrentTransactions[transaction.Key].Sources)
                        {
                            Hash account = new Hash(te.PublicKey);
                            if (temporaryBalances.ContainsKey(account))
                            {
                                temporaryBalances[account] -= te.Value;
                            }
                            else
                            {
                                AccountInfo accountInfo;
                                if (nodeState.Ledger.TryFetch(account, out accountInfo))
                                {
                                    temporaryBalances[account] = accountInfo.Money - te.Value;
                                }
                                else
                                {
                                    // this case should not happen
                                    throw new Exception("account disappeared after check");
                                }
                            }
                        }
                        //add transaction to ballot proposal
                        mergedTransactions.Add(transaction.Key);
                    }
                    else
                    {
                        //handle bad accounts
                        foreach (Hash badaccount in badaccounts)
                        {
                            //remove all bad transactions from ballot proposal
                            long time = 0;
                            foreach (Hash t in mergedTransactions)
                            {
                                foreach (TransactionEntity source in CurrentTransactions[t].Sources)
                                {
                                    if (source.PublicKey == badaccount.Hex)
                                    {
                                        mergedTransactions.Remove(t);
                                    }
                                }
                            }
                            //blacklist bad account
                            blacklist.Add(badaccount, time);
                        }
                    }
                }

                CurrentConsensusState = ConsensusStates.Vote;
                MergeStateCounter = 0;
            }
        }

        /// <summary>
        /// returns true, if transaction is spendable under the current ledger
        /// in combination with transactions from mergedTransactions
        /// adds all accounts with too little balance to badaccounts for blacklisting
        /// removes all double-spending accounts from mergedTransactions
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="temporaryBalances"></param>
        /// <param name="badaccounts"></param>
        /// <returns></returns>
        bool Spendable(TransactionContent transaction, Dictionary<Hash, long> temporaryBalances, out List<Hash> badaccounts)
        {
            badaccounts = new List<Hash>();
            bool spendable = true;
            foreach (TransactionEntity sender in transaction.Sources)
            {
                Hash account = new Hash(sender.PublicKey);

                AccountInfo accountInfo;
                bool ok = nodeState.Ledger.TryFetch(account, out accountInfo);

                // account does not exist
                if (!ok)
                {
                    spendable = false;
                    break;
                }
                // account already used in this voting round
                if (temporaryBalances.ContainsKey(account))
                {
                    if (sender.Value > temporaryBalances[account])
                    {
                        badaccounts.Add(account);
                        spendable = false;
                    }
                }
                // account not used before
                else
                {
                    if (sender.Value > accountInfo.Money)
                    {
                        badaccounts.Add(account);
                        spendable = false;
                    }
                }
            }
            return spendable;
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

        void ProcessFetchRequest(NetworkPacket packet)
        {
            FetchRequestMsg message = new FetchRequestMsg();
            message.Deserialize(packet.Data);

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

        void ProcessFetchResponse(NetworkPacket packet)
        {
            if (networkPacketSwitch.VerifyPendingPacket(packet))
            {
                FetchResponseMsg message = new FetchResponseMsg();
                message.Deserialize(packet.Data);

                foreach (KeyValuePair<Hash, TransactionContent> transaction in message.transactions)
                {
                    if (transaction.Value.VerifySignature() == TransactionProcessingResult.Accepted)
                    {
                        CurrentTransactions.AddOrUpdate(transaction.Key, transaction.Value, (ok, ov) => ov);
                    }
                    else
                    {
                        // TODO: blacklist peer 
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
