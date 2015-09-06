//
//  @Author: Arpan Jati | Stephan Verbuecheln
//  @Date: June 2015 
// The voting and consensus is handled by two files. 
// Voting and VotingRequests (both pertain to the same partial class Voting)
// The basic state machine code is in Voting.cs and the rest of the handling is in VotingRequests.cs
//

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
    #region Enums


    public enum ConsensusStates
    {
        /// <summary>
        /// Initial Collection of pending transactions.
        /// </summary>
        Collect,

        /// <summary>
        /// Merge pending transaction by asking and fetching for transactions.
        /// </summary>
        Merge,

        /// <summary>
        /// Send requests to get ballots and finalise votes.
        /// </summary>
        Vote,

        /// <summary>
        /// Re-Confirm Votes by sending singel requests to all the voters.
        /// </summary>
        Confirm,

        /// <summary>
        /// Apply to ledger.
        /// </summary>
        Apply
    };

    #endregion

    partial class Voting
    {
        int mergeStateCounter = 0;
        int votingStateCounter = 0;
        int confirmationStateCounter = 0;

        public bool VerboseDebugging = false;

        public bool DebuggingMessages { get; set; }

        public bool Enabled { get; set; }

        private object VotingTransactionLock = new object();
        private object ConsensusLock = new object();

        private int previousRoundVoters = 0;

        private long ledgerCloseSequence = 0;

        public ConsensusStates CurrentConsensusState { get; private set; }

        NodeConfig nodeConfig;
        NodeState nodeState;
        NetworkPacketSwitch networkPacketSwitch;

        /// <summary>
        /// ID and content of current transactions
        /// </summary>
        ConcurrentDictionary<Hash, TransactionContent> CurrentTransactions;
        Ballot ballot, finalBallot;
        TransactionChecker transactionChecker = default(TransactionChecker);

        HashSet<Hash> synchronizedVoters;

        /// <summary>
        /// A Map to handle all the incoming votes.
        /// </summary>
        VoteMap voteMap;

        /// <summary>
        /// A list of voters who have confirmed to have the same final ballot as us.
        /// If this is above above some threshold, we should apply all the changes to ledger and move on.
        /// </summary>
        HashSet<Hash> finalConfirmedVoters;

        /// <summary>
        /// Set of nodes, who sent a transaction ID
        /// Key: Transaction ID
        /// Value: Set of nodes
        /// </summary>
        ConcurrentDictionary<Hash, HashSet<Hash>> propagationMap;

        System.Timers.Timer TimerVoting = default(System.Timers.Timer);

        public Voting(NodeConfig nodeConfig, NodeState nodeState, NetworkPacketSwitch networkPacketSwitch)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;
            this.networkPacketSwitch = networkPacketSwitch;
            this.CurrentTransactions = new ConcurrentDictionary<Hash, TransactionContent>();
            this.propagationMap = new ConcurrentDictionary<Hash, HashSet<Hash>>();
            this.voteMap = new VoteMap(nodeConfig, nodeState);
            synchronizedVoters = new HashSet<Hash>();
            finalConfirmedVoters = new HashSet<Hash>();

            networkPacketSwitch.VoteEvent += networkPacketSwitch_VoteEvent;
            networkPacketSwitch.VoteMergeEvent += networkPacketSwitch_VoteMergeEvent;
            transactionChecker = new TransactionChecker(nodeState);

            CurrentConsensusState = ConsensusStates.Collect;

            /*Observable.Interval(TimeSpan.FromMilliseconds(1000))
                .Subscribe(async x => await TimerCallback_Voting(x));*/

            TimerVoting = new System.Timers.Timer();
            TimerVoting.Elapsed += TimerVoting_Elapsed;
            TimerVoting.Enabled = Enabled;
            TimerVoting.Interval = 500;
            TimerVoting.Start();

            DebuggingMessages = true;

            Print("Class \"Voting\" created.");
        }

        private void Print(string message)
        {
            if (DebuggingMessages)
                DisplayUtils.Display("V:" +ledgerCloseSequence + ": "+ nodeConfig.ID() + ": " + message);
        }

        void TimerVoting_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            VotingEvent();
        }

        /* private async Task TimerCallback_Voting(Object o)
         {
             if (Enabled)
             {
                 await Task.Run(() =>
                 {
                     VotingEvent();

                 });
             }
         }*/

        bool isBallotValid = false;
        bool isFinalBallotValid = false;
        bool isFinalConfirmedVotersValid = false;
        bool isAcceptMapValid = false;

        #region State-Machine

        private void VotingEvent()
        {
            try
            {
                lock (ConsensusLock)
                {
                    switch (CurrentConsensusState)
                    {
                        case ConsensusStates.Collect:

                            // LCS = LCL + 1     
                            // ledgerCloseSequence = nodeState.Ledger.LedgerCloseData.SequenceNumber + 1;

                            isBallotValid = false;
                            isFinalBallotValid = false;
                            isAcceptMapValid = false;
                            isFinalConfirmedVotersValid = false;
                            voteMap.Reset();
                            finalBallot = new Ballot(ledgerCloseSequence);
                            ballot = new Ballot(ledgerCloseSequence);
                            finalConfirmedVoters = new HashSet<Hash>();


                            processPendingTransactions();
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
                            HandleApply();
                            break;

                    }
                }

            }
            catch (Exception ex)
            {
                DisplayUtils.Display("TimerCallback_Voting", ex, true);
            }
        }

        void HandleMerge()
        {
            if (mergeStateCounter < 1) // TWEAK-POINT: Trim value.
            {
                sendMergeRequests();
            }

            mergeStateCounter++;

            // After 5 rounds: Assemble Ballot
            if (mergeStateCounter >= 5)
            {
                CreateBallot();

                isBallotValid = true; // Yayy.
                voteMap.Reset();
                CurrentConsensusState = ConsensusStates.Vote;
                mergeStateCounter = 0;

                Print("Merge Finished.");
            }
        }

        private void CreateBallot()
        {
            ballot = transactionChecker.CreateBallot(CurrentTransactions, ledgerCloseSequence);
            ballot.UpdateSignature(nodeConfig.SignDataWithPrivateKey(ballot.GetSignatureData()));
        }

        void HandleVoting()
        {
            if (votingStateCounter < 8) // TWEAK-POINT: Trim value.
            {
                if (votingStateCounter % 2 == 0)
                {
                    sendVoteRequests();
                }
                else
                {
                    Dictionary<Hash, HashSet<Hash>> missingTransactions;
                    voteMap.GetMissingTransactions(ballot, out missingTransactions);

                    sendFetchRequests(missingTransactions);
                }
            }
            else
            {
                // Verify the received

            }

            votingStateCounter++;

            // We will perform dummy voting cycles even when there are no transactions. those will
            // have a different counter, called ConsensusCount, the default is LedgerClose, the ledger close one 
            // is the one associated.

            // Request Ballots

            if (votingStateCounter >= 8)
            {
                votingStateCounter = 0;

                finalBallot = new Ballot(ledgerCloseSequence);
                finalBallot.PublicKey = nodeConfig.PublicKey;
                finalBallot.TransactionIds = voteMap.FilterTransactionsByVotes(ballot, Constants.CONS_FINAL_VOTING_THRESHOLD_PERC);
                finalBallot.Timestamp = nodeState.NetworkTime;

                finalBallot.UpdateSignature(nodeConfig.SignDataWithPrivateKey(finalBallot.GetSignatureData()));

                synchronizedVoters = voteMap.GetSynchronisedVoters(finalBallot);

                finalConfirmedVoters = new HashSet<Hash>(); // Maybe repeat, but okay.

                isFinalBallotValid = true; // TODO: CRITICAL THINK THINK, TESTS !!                

                CurrentConsensusState = ConsensusStates.Confirm;

                Print("Voting Finished.");
            }
        }

        void HandleConfirmation()
        {
            if (confirmationStateCounter < 1) // Send it once only.
            {
                if (isFinalBallotValid)
                {
                    // Send Confirmation
                    sendConfirmationRequests();
                }
            }

            confirmationStateCounter++;

            if (confirmationStateCounter >= 5)
            {
                confirmationStateCounter = 0;

                isFinalConfirmedVotersValid = true;

                CurrentConsensusState = ConsensusStates.Apply; // Verify Confirmation

                Print("Confirm Finished.");
            }
        }

        void HandleApply()
        {
            if (isFinalConfirmedVotersValid)
            {
                // Check that the confirmed voters are all trusted

                int trustedConfirmedVoters = 0;

                if (finalConfirmedVoters == null) Print("BAD_1");
                
                foreach (var voter in finalConfirmedVoters)
                {
                    if (voter == null)
                        Print("BAD_2");

                    if(nodeConfig.TrustedNodes == null) Print("BAD_3");

                    if (nodeConfig.TrustedNodes.ContainsKey(voter))
                    {
                        trustedConfirmedVoters++;
                    }
                }

                int totalTrustedConnections = nodeState.ConnectedValidators.Where(node => node.Value.IsTrusted).Count();

                if ((trustedConfirmedVoters >= Constants.VOTE_MIN_VOTERS) &&
                    (totalTrustedConnections >= Constants.VOTE_MIN_VOTERS))
                {
                    double percentage = ((double)trustedConfirmedVoters * 100.0) / (double)totalTrustedConnections;

                    if (percentage >= Constants.CONS_FINAL_VOTING_THRESHOLD_PERC)
                    {
                        Print("Voting Successful. Applying to ledger.");

                        ledgerCloseSequence++;
                    }
                    else
                    {
                        Print("Voting Unsuccessful. Consesus percentage: " + percentage);
                    }
                }
                else
                {
                    Print("Voting Unsuccessful. Not Enough Trusted Voters. Trusted Voters: " + trustedConfirmedVoters +
                        " Trusted Conns :" + totalTrustedConnections);
                }

                CurrentConsensusState = ConsensusStates.Collect;

               // Print("Apply Finished. Consensus Finished.");
            }
        }

        #endregion

        #region Packet Handling

        void networkPacketSwitch_VoteMergeEvent(NetworkPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.TPT_CONS_MERGE_REQUEST:
                    processMergeRequest(packet);
                    break;
                case PacketType.TPT_CONS_MERGE_RESPONSE:
                    processMergeResponse(packet);
                    break;
                case PacketType.TPT_CONS_MERGE_TX_FETCH_REQUEST:
                    processFetchRequest(packet);
                    break;
                case PacketType.TPT_CONS_MERGE_TX_FETCH_RESPONSE:
                    processFetchResponse(packet);
                    break;
            }
        }

        void networkPacketSwitch_VoteEvent(NetworkPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.TPT_CONS_STATE:
                    break;

                case PacketType.TPT_CONS_VOTE_REQUEST:
                    processVoteRequest(packet);
                    break;

                case PacketType.TPT_CONS_VOTE_RESPONSE:
                    processVoteResponse(packet);
                    break;

                case PacketType.TPT_CONS_CONFIRM_REQUEST:
                    processConfirmRequest(packet);
                    break;

                case PacketType.TPT_CONS_CONFIRM_RESPONSE:
                    processConfirmResponse(packet);
                    break;
            }
        }

        #endregion

    }
}
