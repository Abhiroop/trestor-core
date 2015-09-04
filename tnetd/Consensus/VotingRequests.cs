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
        public void processPendingTransactions()
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
        /// Sends a request to get transaction data for all transaction IDs in the sorted hash
        /// </summary>
        /// <param name="node"></param>
        /// <param name="transactions"></param>
        void sendFetchRequests(Hash node, HashSet<Hash> transactions)
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
            Print("Fetch Request Sent to " + node);
        }

        /// <summary>
        /// The key of the dictionary is the PK of the destination, and the value is the list of associated txID's.
        /// </summary>
        /// <param name="requests"></param>
        void sendFetchRequests(Dictionary<Hash, HashSet<Hash>> requests)
        {
            foreach (var request in requests)
            {
                sendFetchRequests(request.Key, request.Value);
            }
        }

        /// <summary>
        /// responds to a fetch request, sending all requested transaction data
        /// </summary>
        /// <param name="packet"></param>
        void processFetchRequest(NetworkPacket packet)
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
            Print("Fetch Request From " + packet.PublicKeySource + " Processed");
        }

        /// <summary>
        /// process the response to a fetch request, i.e. add the transactions to CurrentTransactions
        /// </summary>
        /// <param name="packet"></param>
        void processFetchResponse(NetworkPacket packet)
        {
            if (CurrentConsensusState == ConsensusStates.Merge || CurrentConsensusState == ConsensusStates.Vote)
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
            }

            if (CurrentConsensusState == ConsensusStates.Vote)
            {
                // Update Ballot Accordingly.

                CreateBallot();
            }

            Print("Fetch Response From " + packet.PublicKeySource + " Processed");
        }

        /// <summary>
        /// request a list of all known transactions from each connected validator
        /// note that message has no content
        /// </summary>
        void sendMergeRequests()
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
            Print("Merge Requests Sent to " + nodeState.ConnectedValidators.Count + " Validators");
        }

        /// <summary>
        /// respond to a merge request by sending a list of all hashes of known transactions
        /// </summary>
        /// <param name="packet"></param>
        void processMergeRequest(NetworkPacket packet)
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
            Print("Merge Request from " + packet.PublicKeySource + " Processed");
        }

        /// <summary>
        /// respond to merge request by sending a list of all hashes of known (not expired) transactions
        /// </summary>
        /// <param name="packet"></param>
        void processMergeResponse(NetworkPacket packet)
        {
            if (networkPacketSwitch.VerifyPendingPacket(packet))
            {
                MergeResponseMsg message = new MergeResponseMsg();
                message.Deserialize(packet.Data);
                HashSet<Hash> newTransactions = new HashSet<Hash>();

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

                sendFetchRequests(packet.PublicKeySource, newTransactions);
            }
            Print("Merge Response from " + packet.PublicKeySource + " Processed");
        }


        void sendVoteRequests()
        {
            LedgerCloseData lcd;
            bool ok = nodeState.PersistentCloseHistory.GetLastRowData(out lcd);

            // TODO: OF okay = false make sure that the ledger is synced first before voting.

            foreach (var node in nodeState.ConnectedValidators)
            {
                // Create BallotRequestMessage
                VoteRequestMessage brp = new VoteRequestMessage();
                brp.LedgerCloseSequence = lcd.SequenceNumber;

                // Create NetworkPacket and send
                NetworkPacket request = new NetworkPacket();
                request.PublicKeySource = nodeConfig.PublicKey;
                request.Token = TNetUtils.GenerateNewToken();
                request.Data = brp.Serialize();
                request.Type = PacketType.TPT_CONS_VOTE_REQUEST;

                networkPacketSwitch.AddToQueue(node.Key, request);
            }

            Print("Vote requests sent to " + nodeState.ConnectedValidators.Count + " Nodes");
        }


        void processVoteRequest(NetworkPacket packet)
        {
            VoteResponseMessage brm = new VoteResponseMessage();

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
            response.Type = PacketType.TPT_CONS_VOTE_RESPONSE;
            networkPacketSwitch.AddToQueue(sender, response);

            Print("Vote Request Replied to " + sender);
        }

        void processVoteResponse(NetworkPacket packet)
        {
            if (networkPacketSwitch.VerifyPendingPacket(packet))
            {
                if (CurrentConsensusState == ConsensusStates.Vote)
                {
                    VoteResponseMessage message = new VoteResponseMessage();
                    message.Deserialize(packet.Data);

                    if (message.goodBallot)
                    {
                        // We should be voting for the next ballot.
                        if (message.ballot.LedgerCloseSequence == ledgerCloseSequence)
                        {
                            // Sender and ballot keys must be same
                            if (message.ballot.PublicKey == packet.PublicKeySource)
                            {
                                // Signature should be valid.
                                if (message.ballot.VerifySignature(packet.PublicKeySource))
                                {
                                    voteMap.AddBallot(message.ballot);

                                }
                            }
                        }
                    }
                }
            }

            Print("Vote Response from " + packet.PublicKeySource + " Processed");
        }

    }
}
