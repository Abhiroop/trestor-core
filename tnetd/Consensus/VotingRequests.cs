﻿
//
//  @Author: Arpan Jati
//  @Date: September 2015 
//

using System;
using System.Collections.Concurrent;
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
                        (Common.NODE_OPERATION_TYPE == NodeOperationType.Distributed))
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

            networkPacketSwitch.AddToQueue(node, new NetworkPacket()
            {
                Data = message.Serialize(),
                Token = token,
                PublicKeySource = nodeConfig.PublicKey,
                Type = PacketType.TPT_CONS_MERGE_TX_FETCH_REQUEST
            });

            if (VerboseDebugging) Print("Fetch Request Sent to " + node);
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

            networkPacketSwitch.AddToQueue(packet.PublicKeySource, new NetworkPacket()
            {
                Data = response.Serialize(),
                Token = packet.Token,
                PublicKeySource = nodeConfig.PublicKey,
                Type = PacketType.TPT_CONS_MERGE_TX_FETCH_RESPONSE
            });

            if (VerboseDebugging) Print("Fetch Request From " + packet.PublicKeySource + " Processed");
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

            if (VerboseDebugging) Print("Fetch Response From " + packet.PublicKeySource + " Processed");
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

                networkPacketSwitch.AddToQueue(node.Key, new NetworkPacket()
                {
                    PublicKeySource = nodeConfig.PublicKey,
                    Token = token,
                    Type = PacketType.TPT_CONS_MERGE_REQUEST
                });
            }

            if (VerboseDebugging) Print("Merge Requests Sent to " + nodeState.ConnectedValidators.Count + " Validators");
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
                message.AddTransaction(transaction.Key);
            }

            networkPacketSwitch.AddToQueue(sender, new NetworkPacket()
            {
                Token = token,
                PublicKeySource = nodeConfig.PublicKey,
                Data = message.Serialize(),
                Type = PacketType.TPT_CONS_MERGE_RESPONSE
            });

            if (VerboseDebugging) Print("Merge Request from " + packet.PublicKeySource + " Processed");
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

                voteMessageCounter.UpdateVoters(packet.PublicKeySource);

                foreach (Hash transaction in message)
                {
                    //check whether transaction for the given ID is already known
                    if (!CurrentTransactions.ContainsKey(transaction))
                    {
                        newTransactions.Add(transaction);
                    }
                    //add sender to propagationMap

                    if (propagationMap.ContainsKey(transaction))
                    {
                        propagationMap[transaction].Add(packet.PublicKeySource);
                    }
                    else
                    {
                        HashSet<Hash> val = new HashSet<Hash>();
                        val.Add(packet.PublicKeySource);

                        propagationMap.AddOrUpdate(transaction, val, (k, v) => val);
                    }
                }

                sendFetchRequests(packet.PublicKeySource, newTransactions);
            }

            if (VerboseDebugging) Print("Merge Response from " + packet.PublicKeySource + " Processed");
        }

        /// <summary>
        /// Send sync requests to trusted nodes.
        /// </summary>
        void sendSyncRequests()
        {
            foreach (var node in nodeConfig.TrustedNodes)
            {
                // Create BallotRequestMessage
                SyncMessage sm = new SyncMessage();
                sm.LedgerCloseSequence = LedgerCloseSequence;
                sm.ConsensusState = CurrentConsensusState;

                // Create NetworkPacket and send

                networkPacketSwitch.AddToQueue(node.Key, new NetworkPacket()
                {
                    PublicKeySource = nodeConfig.PublicKey,
                    Token = TNetUtils.GenerateNewToken(),
                    Data = sm.Serialize(),
                    Type = PacketType.TPT_CONS_SYNC_REQUEST
                });
            }

            if (VerboseDebugging) Print("Sync requests sent to " + nodeState.ConnectedValidators.Count + " Nodes");
        }

        void processSyncRequest(NetworkPacket packet)
        {
            SyncMessage syncRequest = new SyncMessage();
            syncRequest.Deserialize(packet.Data);

            SyncMessage syncResponse = new SyncMessage();

            syncResponse.ConsensusState = CurrentConsensusState;
            syncResponse.LedgerCloseSequence = LedgerCloseSequence;

            networkPacketSwitch.AddToQueue(packet.PublicKeySource, new NetworkPacket()
            {
                Token = packet.Token,
                PublicKeySource = nodeConfig.PublicKey,
                Data = syncResponse.Serialize(),
                Type = PacketType.TPT_CONS_SYNC_RESPONSE
            });

            if (VerboseDebugging) Print("Sync Request Replied to " + packet.PublicKeySource);
        }
        
        void processSyncResponse(NetworkPacket packet)
        {
            if (networkPacketSwitch.VerifyPendingPacket(packet))
            {
                if (CurrentConsensusState == ConsensusStates.Sync)
                {
                    // Is trusted !
                    if (nodeConfig.TrustedNodes.ContainsKey(packet.PublicKeySource))
                    {
                        SyncMessage message = new SyncMessage();
                        message.Deserialize(packet.Data);

                        if (!syncMap.ContainsKey(packet.PublicKeySource))
                        {
                            syncMap.AddOrUpdate(packet.PublicKeySource, message.ConsensusState, (k,v) => message.ConsensusState);
                        }
                        else
                        {
                            syncMap[packet.PublicKeySource] = message.ConsensusState;
                        }
                    }
                }
            }

            if (VerboseDebugging) Print("Sync Response from " + packet.PublicKeySource + " Processed");
        }

        static int ConsensusStatesMemberCount = Enum.GetNames(typeof(ConsensusStates)).Length;

        /// <summary>
        /// Tuple: True means than at least 3 nodes have replied with the same state.
        /// </summary>
        /// <returns></returns>
        Tuple<ConsensusStates, bool> MedianTrustedState()
        {
            int[] syncers = new int[ConsensusStatesMemberCount];

            foreach (var state in syncMap)
            {
                syncers[(int)state.Value]++;
            }

            int maxIndex = 0, maxCount = 0;

            for (int i = 0; i < ConsensusStatesMemberCount; i++)
            {
                if (syncers[i] > maxCount)
                {
                    maxCount = syncers[i];
                    maxIndex = i;
                }
            }

            return new Tuple<ConsensusStates, bool>((ConsensusStates)maxIndex, maxCount >= 3);
        }
        
        void sendVoteRequests()
        {
            foreach (var node in nodeState.ConnectedValidators)
            {
                // Create BallotRequestMessage
                VoteRequestMessage brp = new VoteRequestMessage();
                brp.LedgerCloseSequence = LedgerCloseSequence;
                brp.VotingState = CurrentVotingState;

                // Create NetworkPacket and send

                networkPacketSwitch.AddToQueue(node.Key, new NetworkPacket()
                {
                    PublicKeySource = nodeConfig.PublicKey,
                    Token = TNetUtils.GenerateNewToken(),
                    Data = brp.Serialize(),
                    Type = PacketType.TPT_CONS_VOTE_REQUEST
                });
            }

            if (VerboseDebugging) Print("Vote requests sent to " + nodeState.ConnectedValidators.Count + " Nodes");
        }

        void processVoteRequest(NetworkPacket packet)
        {
            VoteRequestMessage voteRequest = new VoteRequestMessage();
            voteRequest.Deserialize(packet.Data);

            VoteResponseMessage voteResponse = new VoteResponseMessage();

            if (voteRequest.LedgerCloseSequence == LedgerCloseSequence &&
                CheckAcceptableVotingState(voteResponse.VotingState))
            {
                voteResponse.IsSynced = true;
                voteMessageCounter.IncrementVotes();

                if (isBallotValid)
                {
                    voteResponse.Ballot = ballot;
                    voteResponse.GoodBallot = true;
                }
            }
            else
            {
                voteResponse.GoodBallot = false;
                voteResponse.IsSynced = false;

                Print("LCS (PVReq) Mismatch for " + GetTrustedName(packet.PublicKeySource) +
                    " : " + voteRequest.LedgerCloseSequence + "!=" + LedgerCloseSequence + ", VS:" + voteResponse.VotingState + "/" + CurrentVotingState);
            }

            networkPacketSwitch.AddToQueue(packet.PublicKeySource, new NetworkPacket()
            {
                Token = packet.Token,
                PublicKeySource = nodeConfig.PublicKey,
                Data = voteResponse.Serialize(),
                Type = PacketType.TPT_CONS_VOTE_RESPONSE
            });

            if (VerboseDebugging) Print("Vote Request Replied to " + packet.PublicKeySource);
        }
        
        bool CheckAcceptableVotingState(VotingStates _vs)
        {
            int vs = (int) _vs;
            int cvs = (int) CurrentVotingState;

            if (vs == cvs) return true;

            if (Math.Abs(vs - cvs) == 1) return true; // THINK: A difference of 1
            
            return false;
        }

        void processVoteResponse(NetworkPacket packet)
        {
            if (networkPacketSwitch.VerifyPendingPacket(packet))
            {
                if (CurrentConsensusState == ConsensusStates.Vote)
                {
                    VoteResponseMessage voteResponse = new VoteResponseMessage();
                    voteResponse.Deserialize(packet.Data);

                    if (voteResponse.GoodBallot)
                    {
                        // We should be voting for the next ballot.
                        if (voteResponse.Ballot.LedgerCloseSequence == LedgerCloseSequence &&
                            CheckAcceptableVotingState(voteResponse.VotingState))
                        {
                            // Sender and ballot keys must be same
                            if (voteResponse.Ballot.PublicKey == packet.PublicKeySource)
                            {
                                // Signature should be valid.
                                if (voteResponse.Ballot.VerifySignature(packet.PublicKeySource))
                                {
                                    voteMap.AddBallot(voteResponse.Ballot);

                                }
                            }
                        }
                        else
                        {
                            Print("LCS Mismatch (PVResp) for " + GetTrustedName(packet.PublicKeySource) +
                                " : " + voteResponse.Ballot.LedgerCloseSequence + "!=" + LedgerCloseSequence + ", VS:" + voteResponse.VotingState +"/" + CurrentVotingState);
                        }
                    }
                }
            }

            if (VerboseDebugging) Print("Vote Response from " + packet.PublicKeySource + " Processed");
        }

        string GetTrustedName(Hash publicKey)
        {
            if (nodeConfig.TrustedNodes.ContainsKey(publicKey))
            {
                return nodeConfig.TrustedNodes[publicKey].Name;
            }
            else return "NOT_TRUSTED : " + publicKey.ToString().Substring(0, 12);
        }

        /* void sendConfirmationRequests()
         {
             foreach (var node in nodeState.ConnectedValidators)
             {
                 // Create VoteConfirmRequest
                 VoteConfirmRequest vcr = new VoteConfirmRequest();
                 vcr.LedgerCloseSequence = LedgerCloseSequence;
                 vcr.PublicKey = nodeConfig.PublicKey;

                 // Create NetworkPacket and Send           

                 networkPacketSwitch.AddToQueue(node.Key, new NetworkPacket()
                 {
                     PublicKeySource = nodeConfig.PublicKey,
                     Token = TNetUtils.GenerateNewToken(),
                     Data = vcr.Serialize(),
                     Type = PacketType.TPT_CONS_CONFIRM_REQUEST
                 });
             }

             if (VerboseDebugging) Print("Confirmation Requests sent to " + nodeState.ConnectedValidators.Count + " Nodes");
         }

         void processConfirmRequest(NetworkPacket packet)
         {
             VoteConfirmRequest voteConfirmRequest = new VoteConfirmRequest();
             voteConfirmRequest.Deserialize(packet.Data);

             VoteConfirmResponse voteConfirmResponse = new VoteConfirmResponse();

             if (voteConfirmRequest.LedgerCloseSequence == LedgerCloseSequence &&
                 voteConfirmRequest.PublicKey == packet.PublicKeySource)
             {
                 voteConfirmResponse.IsSynced = true;
                 voteMessageCounter.IncrementConfirmations();

                 if (isFinalBallotValid)
                 {
                     voteConfirmResponse.BallotGood = true;
                     voteConfirmResponse.FinalBallot = finalBallot;
                 }
             }
             else
             {
                 // If the peer is not synced, there no point sending the ballot anyway.
                 voteConfirmResponse.IsSynced = false;
                 voteConfirmResponse.BallotGood = false;

                 Print("LCS (PCReq) Mismatch for " + GetTrustedName(packet.PublicKeySource)
                     + " : " + voteConfirmRequest.LedgerCloseSequence + "!=" + LedgerCloseSequence);
             }

             networkPacketSwitch.AddToQueue(packet.PublicKeySource, new NetworkPacket()
             {
                 PublicKeySource = nodeConfig.PublicKey,
                 Token = packet.Token,
                 Data = voteConfirmResponse.Serialize(),
                 Type = PacketType.TPT_CONS_CONFIRM_RESPONSE
             });

             if (VerboseDebugging) Print("Confirm Response sent to " + packet.PublicKeySource);
         }

         void processConfirmResponse(NetworkPacket packet)
         {
             if (networkPacketSwitch.VerifyPendingPacket(packet))
             {
                 if (CurrentConsensusState == ConsensusStates.Confirm)
                 {
                     VoteConfirmResponse response = new VoteConfirmResponse();
                     response.Deserialize(packet.Data);

                     if (response.BallotGood && response.IsSynced)
                     {
                         if (response.FinalBallot.PublicKey == packet.PublicKeySource)
                         {
                             Ballot receivedBallot = response.FinalBallot;

                             if (isFinalBallotValid)
                             {
                                 // Verify ballot signature.
                                 if (receivedBallot.VerifySignature(packet.PublicKeySource))
                                 {
                                     // Assert that the voter should be already on the list
                                     if (synchronizedVoters.Contains(packet.PublicKeySource))
                                     {
                                         // Validate ballot similarity
                                         if (voteMap.CheckVoterSyncState(finalBallot, receivedBallot))
                                         {
                                             // Valiate ballot timestamp
                                             if (Utils.CheckTimeCloseness(finalBallot.Timestamp, receivedBallot.Timestamp, 1500))
                                             {
                                                 finalVoters.Add(packet.PublicKeySource, LedgerCloseSequence);

                                             }
                                         }
                                     }
                                 }
                             }
                         }
                     }
                 }
             }

             if (VerboseDebugging) Print("Vote Confirm Response from '" + packet.PublicKeySource + "' Processed");
         }*/


    }
}
