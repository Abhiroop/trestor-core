using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Network;
using TNetD.Network.Networking;
using TNetD.Transactions;

namespace TNetD.Consensus
{
    partial class Voting
    {

        public void ProcessPendingTransactions()
        {
            lock (VotingTransactionLock)
            {
                try
                {
                    if ((nodeState.IncomingTransactionMap.IncomingTransactions.Count > 0) &&
                        (Common.NodeOperationType == NodeOperationType.Distributed))
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
                catch
                {

                }
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
            packet.Type = PacketType.TPT_CONS_MERGE_TX_FETCH_REQUEST;
            networkPacketSwitch.AddToQueue(node, packet);
            Print("fetch request sent");
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
            rpacket.Type = PacketType.TPT_CONS_MERGE_TX_FETCH_RESPONSE;
            networkPacketSwitch.AddToQueue(packet.PublicKeySource, rpacket);
            Print("fetch request processed");
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
                        if (transactionChecker.Spendable(transaction.Value, new Dictionary<Hash, long>(), out badaccounts))
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
            Print("Fetch Response Processed");
        }
        
        /// <summary>
        /// request a list of all known transactions from each connected validator
        /// note that message has no content
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
            Print("merge requests sent");
        }

        /// <summary>
        /// respond to a merge request by sending a list of all hashes of known transactions
        /// </summary>
        /// <param name="packet"></param>
        void ProcessMergeRequest(NetworkPacket packet)
        {
            Hash sender = packet.PublicKeySource;
            Hash token = packet.Token;

            //add all transaction IDs from CurrentTransactions
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
            Print("merge request processed");
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
            Print("merge response processed");
        }


        void SendBallotRequests()
        {
            LedgerCloseData lcd;
            bool ok = nodeState.PersistentCloseHistory.GetLastRowData(out lcd);

            // TODO: OF okay = false make sure that the ledger is synced first before voting.

            foreach (var node in nodeState.ConnectedValidators)
            {
                // Create BallotRequestMessage
                BallotRequestMessage brp = new BallotRequestMessage();
                brp.LedgerCloseSequence = lcd.SequenceNumber;

                // Create NetworkPacket and send
                NetworkPacket request = new NetworkPacket();
                request.PublicKeySource = nodeConfig.PublicKey;
                request.Token = TNetUtils.GenerateNewToken();
                request.Data = brp.Serialize();
                request.Type = PacketType.TPT_CONS_BALLOT_REQUEST;

                networkPacketSwitch.AddToQueue(node.Key, request);
            }

            Print("Ballot requests sent to " + nodeState.ConnectedValidators.Count + " Nodes");
        }


        void ProcessBallotRequest(NetworkPacket packet)
        {
            BallotResponseMessage brm = new BallotResponseMessage();

            if (isBallotValid)
            {
                brm.ballot = ballot;
                brm.goodBallot = true;
            }

            Hash sender = packet.PublicKeySource;
            Hash token = packet.Token;

            NetworkPacket response = new NetworkPacket();
            response.Token = token;
            response.PublicKeySource = nodeConfig.PublicKey;
            response.Data = brm.Serialize();
            response.Type = PacketType.TPT_CONS_BALLOT_RESPONSE;
            networkPacketSwitch.AddToQueue(sender, response);

            Print("ProcessBallotRequest Replied to " + sender);
        }

        void ProcessBallotResponse(NetworkPacket packet)
        {
            if (networkPacketSwitch.VerifyPendingPacket(packet))
            {
                BallotResponseMessage message = new BallotResponseMessage();
                message.Deserialize(packet.Data);
                
                                 
            }

            Print("Ballot Response Processed");
        }

    }
}
