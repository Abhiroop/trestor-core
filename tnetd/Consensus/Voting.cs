
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
using System.Reactive.Concurrency;

namespace TNetD.Consensus
{
    #region Enums

    public enum ConsensusStates
    {
        /// <summary>
        /// Synchronize the start of consensus.
        /// </summary>
        Sync,

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
    public enum VotingStates { STNone, ST40, ST60, ST75, ST80, STDone };

    #endregion

    partial class Voting
    {
        int mergeStateCounter = 0;
        int votingStateCounter = 0;
        int confirmationStateCounter = 0;
        int applyStateCounter = 0;

        public bool VerboseDebugging = false;

        public bool DebuggingMessages { get; set; }

        private bool enabled = false;

        public bool Enabled
        {
            get
            {
                return enabled;
            }

            set
            {
                enabled = true;
                InitLCS(nodeState); //FIX_TEMPORARY_TESTING MEASURE
            }
        }

        private object VotingTransactionLock = new object();
        private object ConsensusLock = new object();

        public LedgerCloseSequence LedgerCloseSequence { get; private set; }

        public ConsensusStates CurrentConsensusState { get; private set; }

        public VotingStates CurrentVotingState = VotingStates.STNone;

        NodeConfig nodeConfig;
        NodeState nodeState;
        NetworkPacketSwitch networkPacketSwitch;

        /// <summary>
        /// ID and content of current transactions
        /// </summary>
        ConcurrentDictionary<Hash, TransactionContent> CurrentTransactions;
        Ballot ballot, finalBallot;
        TransactionChecker transactionChecker;
        TransactionValidator transactionValidator;

        HashSet<Hash> synchronizedVoters;

        TimeStep timeStep;

        /// <summary>
        /// A Map to handle all the incoming votes.
        /// </summary>
        VoteMap voteMap;

        /// <summary>
        /// A list of voters who have confirmed to have the same final ballot as us.
        /// If this is above above some threshold, we should apply all the changes to ledger and move on.
        /// </summary>
        //FinalVoters finalVoters;

        /// <summary>
        /// Set of nodes, who sent a transaction ID
        /// Key: Transaction ID
        /// Value: Set of nodes
        /// </summary>
        ConcurrentDictionary<Hash, HashSet<Hash>> propagationMap;

        ConcurrentDictionary<Hash, SyncState> syncMap;

        VoteMessageCounter voteMessageCounter;

        

        //System.Timers.Timer TimerVoting = default(System.Timers.Timer);

        public Voting(NodeConfig nodeConfig, NodeState nodeState, NetworkPacketSwitch networkPacketSwitch)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;
            this.networkPacketSwitch = networkPacketSwitch;
            this.CurrentTransactions = new ConcurrentDictionary<Hash, TransactionContent>();
            this.propagationMap = new ConcurrentDictionary<Hash, HashSet<Hash>>();
            this.syncMap = new ConcurrentDictionary<Hash, SyncState>();
            this.voteMap = new VoteMap(nodeConfig, nodeState);
            this.synchronizedVoters = new HashSet<Hash>();
            this.timeStep = new TimeStep();

            //finalVoters = new FinalVoters();

            finalBallot = new Ballot();
            ballot = new Ballot();

            networkPacketSwitch.ConsensusEvent += networkPacketSwitch_ConsensusEvent;

            transactionChecker = new TransactionChecker(nodeState);
            transactionValidator = new TransactionValidator(nodeConfig, nodeState);

            CurrentConsensusState = ConsensusStates.Sync;

            voteMessageCounter = new VoteMessageCounter();

            Observable.Interval(TimeSpan.FromMilliseconds(TimeStep.DEFAULT_TIMER_FASTSTEP))
                .Subscribe(async x => await TimerVoting_FastElapsed(x));

            // generate 25 random integers sequence
            /*  var generator = Observable.Generate(
                  0,         // initial value
                  i => true, // while predicate
                  i => i,    // iterator
                  next => next,  // selector
                  delay => TimeSpan.FromMilliseconds(nextTimeStep),   // time interval
                  Scheduler.Default
              );          

              // print all values one by one
              generator.Subscribe(async x => await Exec(x));*/

            //System.Threading.Timer t;
            //t.Change()

            /*TimerVoting = new System.Timers.Timer();
            TimerVoting.Elapsed += TimerVoting_Elapsed;
            TimerVoting.Enabled = true;
            TimerVoting.Interval = 500;
            TimerVoting.Start();*/

            DebuggingMessages = true;

            Print("Class \"Voting\" created.");
        }

        /* public async Task Exec(object obj)
         {
             await Task.Delay(10);
             nextTimeStep = (int) obj;
             Print("VALUE: " + obj);
         }*/

        void UpdateLCD()
        {
            LedgerCloseData lcd;
            if (nodeState.PersistentCloseHistory.GetLastRowData(out lcd))
            {
                LedgerCloseSequence = new LedgerCloseSequence(lcd);
            }
            else
            {
                LedgerCloseSequence = new LedgerCloseSequence(0, nodeState.Ledger.GetRootHash());
            }
        }

        private void InitLCS(NodeState nodeState)
        {
            if (nodeState.Ledger.LedgerCloseData.LedgerHash == null)
            {
                // No voting has happened yet !
                LedgerCloseSequence = new LedgerCloseSequence(0, nodeState.Ledger.GetRootHash());
            }
            else
            {
                LedgerCloseData lcd;
                if (nodeState.PersistentCloseHistory.GetLastRowData(out lcd))
                {
                    LedgerCloseSequence = new LedgerCloseSequence(lcd);
                }
                else
                {
                    LedgerCloseSequence = new LedgerCloseSequence(0, nodeState.Ledger.GetRootHash());
                }
            }
        }

        private void Print(string message)
        {
            if (DebuggingMessages)
                DisplayUtils.Display("V:" + LedgerCloseSequence + ": " + nodeConfig.ID() + ": " + message);
        }

        private void PrintImpt(string message)
        {
            if (DebuggingMessages)
                DisplayUtils.Display("V:" + LedgerCloseSequence + ": " + nodeConfig.ID() + ": " + message, DisplayType.Warning);
        }

        /* void TimerVoting_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
         {
             if (Enabled)
                 VotingEvent();
         }*/
         
        bool timerLockFree = true;

        async Task TimerVoting_FastElapsed(object sender)
        {
            if (Enabled && timerLockFree)
            {
                timeStep.Step(TimeStep.DEFAULT_TIMER_FASTSTEP);

                if (timeStep.IsComplete())
                {
                    timerLockFree = false;
                    await VotingEvent();
                    
                    timerLockFree = true;
                }
            }
        }

        bool isBallotValid = false;
        bool isFinalBallotValid = false;
        bool isFinalConfirmedVotersValid = false;
        bool isAcceptMapValid = false;

        int syncStateCounter = 0;
        bool syncSend = false;

        #region State-Machine

        SemaphoreSlim semaphoreVoting = new SemaphoreSlim(1);

       

        private async Task VotingEvent()
        {
            try
            {
                timeStep.Initalize();
                await semaphoreVoting.WaitAsync();

                switch (CurrentConsensusState)
                {
                    case ConsensusStates.Sync:

                        await HandleSync();

                        break;

                    case ConsensusStates.Collect:

                        // LCS = LCL + 1  |  CRITICAL FIX !   
                        // ledgerCloseSequence = nodeState.Ledger.LedgerCloseData.SequenceNumber + 1;

                        UpdateLCD();

                        isBallotValid = false;
                        isFinalBallotValid = false;
                        isAcceptMapValid = false;
                        isFinalConfirmedVotersValid = false;
                        voteMap.Reset();
                        voteMessageCounter.ResetVotes();
                        voteMessageCounter.ResetUniqueVoters();
                        finalBallot = new Ballot(LedgerCloseSequence);
                        ballot = new Ballot(LedgerCloseSequence);
                        //finalVoters.Reset(LedgerCloseSequence);                   

                        processPendingTransactions();
                        CurrentConsensusState = ConsensusStates.Merge;
                        break;

                    case ConsensusStates.Merge:
                        await HandleMerge();
                        break;

                    case ConsensusStates.Vote:
                        await HandleVoting();
                        break;

                    case ConsensusStates.Confirm:
                        //HandleConfirmation();
                        break;

                    case ConsensusStates.Apply:
                        HandleApply();
                        break;
                }
            }
            catch (Exception ex)
            {
                DisplayUtils.Display("TimerCallback_Voting", ex, true);
            }
            finally
            {
                semaphoreVoting.Release();

                // If forced time is not set, set the step time to default.
                timeStep.ResetTimeStepIfNotSet();
            }
        }

        private async Task HandleSync()
        {
            if (!syncSend)
            {
                syncMap.Clear();
                await sendSyncRequests();
                syncSend = true;
            }
            else
            {
                syncSend = false;

                var syncResults = MedianTrustedState();

                if (syncResults.Item2)
                {
                    /*var syncResults2 = MedianTrustedVotingState();

                    if (syncResults2.Item2)
                    {
                        // Voting and State;

                        CurrentConsensusState = ConsensusStates.Collect;
                        syncStateCounter = 0;

                        Print("Sync ###### COPY MEDIAN STATE ######### Normal.");

                        if (CurrentConsensusState == ConsensusStates.Sync)
                            CurrentConsensusState = ConsensusStates.Merge;

                        CurrentVotingState = syncResults2.Item1;
                    }
                    */

                    // Great We know something
                    if (syncResults.Item1 == ConsensusStates.Sync || syncResults.Item1 == ConsensusStates.Merge)
                    {
                        // Awesome ! we can continue :)
                        CurrentConsensusState = ConsensusStates.Collect;
                        syncStateCounter = 0;

                        // bool b = nodeState.NodeLatency.GetAverageLatency(nodeConfig.PublicKey)

                        Print("Sync Done. Normal.");
                    }
                    else
                    {
                        // Too bad we need to wait for sync
                        Print("Sync Wait.");
                    }
                }
                else
                {
                    // We dont have anyone replying
                    // Wait a few cycles and then start anyway.

                    if (syncStateCounter > 200)
                    {
                        CurrentConsensusState = ConsensusStates.Collect;
                        syncStateCounter = 0;

                        Print("Sync Done. Forced.");
                    }
                }
            }

            syncStateCounter++;
        }

        string GetTxCount(Ballot ballot)
        {
            if (ballot.TransactionCount > 0)
            {

                return "" + ballot.TransactionCount + " Txns";
            }

            return "";
        }

        async Task HandleMerge()
        {
            if (mergeStateCounter < 1) // TWEAK-POINT: Trim value.
            {
                await sendMergeRequests();
            }

            mergeStateCounter++;

            // After 5 rounds: Assemble Ballot
            if (mergeStateCounter >= 5)
            {
                CreateBallot();

                isBallotValid = true; // Yayy.
                voteMap.Reset();
                CurrentConsensusState = ConsensusStates.Vote;
                voteMessageCounter.ResetVotes();
                mergeStateCounter = 0;

                Print("Merge Finished. " + GetTxCount(ballot));
            }
        }

        private void CreateBallot()
        {
            ballot = transactionChecker.CreateBallot(CurrentTransactions, LedgerCloseSequence);
            ballot.UpdateSignature(nodeConfig.SignDataWithPrivateKey(ballot.GetSignatureData()));
        }

        int MAX_EXTRA_VOTING_STEP_WAIT_CYCLES = 10;
        int extraVotingDelayCycles = 0; // Wait for all the voters to send their requests.
        bool currentVotingRequestSent = false;

        //int extraConfirmationDelayCycles = 0;

        enum VoteNextState { Wait, Next }

        VoteNextState CheckReceivedExpectedVotePackets()
        {
            if (voteMessageCounter.Votes < voteMessageCounter.UniqueVoteResponders)
            {
                if (extraVotingDelayCycles < MAX_EXTRA_VOTING_STEP_WAIT_CYCLES)
                {
                    if (extraVotingDelayCycles > 0)
                    {
                        Print("Waiting a cycle for pending voting requests : " + voteMessageCounter.Votes +
                            "/" + voteMessageCounter.UniqueVoteResponders + " Received");

                        timeStep.SetNextTimeStep(50);
                    }

                    extraVotingDelayCycles++;

                    return VoteNextState.Wait;
                }
            }

            return VoteNextState.Next;
        }

        async Task VotingPostRound(VotingStates state, float Percentage)
        {
            extraVotingDelayCycles = 0;
            currentVotingRequestSent = false;

            Dictionary<Hash, HashSet<Hash>> missingTransactions;
            voteMap.GetMissingTransactions(ballot, out missingTransactions);

            await sendFetchRequests(missingTransactions);

            //////////////////////////////////////////////////////////////////

            SortedSet<Hash> passedTxs = voteMap.FilterTransactionsByVotes(ballot, Percentage);

            ballot.Reset(LedgerCloseSequence);
            ballot.PublicKey = nodeConfig.PublicKey;
            ballot.AddRange(passedTxs);
            ballot.Timestamp = nodeState.NetworkTime;

            ballot.UpdateSignature(nodeConfig.SignDataWithPrivateKey(ballot.GetSignatureData()));

            isBallotValid = true;

            Print("Voting " + state + " Done" + voteMessageCounter.Votes +
                "/" + voteMessageCounter.UniqueVoteResponders + " Accepted " + passedTxs.Count +
                " Txns, Fetching " + missingTransactions.SelectMany(p => p.Value).Count() + " Txns");

            voteMessageCounter.ResetVotes();
        }

        async Task<VotingStates> HandleVotingInternal(VotingStates state, float percentage)
        {
            if (!currentVotingRequestSent)
            {
                voteMap.Reset();
                await sendVoteRequests();
                currentVotingRequestSent = true;
            }

            if (CheckReceivedExpectedVotePackets() == VoteNextState.Next)
            {
                await VotingPostRound(state, percentage);

                return (state + 1);
            }

            return state;
        }

        async Task HandleVoting()
        {
            switch (CurrentVotingState)
            {
                case VotingStates.STNone:
                    // Pre-Voting Stuff !! 
                    voteMap.Reset();
                    CurrentVotingState = VotingStates.ST40;
                    break;

                case VotingStates.ST40:
                    CurrentVotingState = await HandleVotingInternal(CurrentVotingState, 40);
                    break;

                case VotingStates.ST60:
                    CurrentVotingState = await HandleVotingInternal(CurrentVotingState, 60);
                    break;

                case VotingStates.ST75:
                    CurrentVotingState = await HandleVotingInternal(CurrentVotingState, 75);
                    break;

                case VotingStates.ST80:
                    CurrentVotingState = await HandleVotingInternal(CurrentVotingState, 80);
                    break;

                case VotingStates.STDone:
                    PostVotingOperations();
                    break;
            }
        }

        /*
                    void HandleVoting()
                {
                    switch (CurrentVotingState)
                    {
                        case VotingStates.STNone:
                            // Pre-Voting Stuff !!                    
                            CurrentVotingState = VotingStates.ST40;
                            break;

                        case VotingStates.ST40:

                            if (!currentVotingRequestSent)
                            {
                                voteMap.Reset();
                                sendVoteRequests();
                                currentVotingRequestSent = true;
                            }

                            if (CheckReceivedExpectedVotePackets() == VoteNextState.Next)
                            {
                                Print("Voting Step40 Done" + voteMessageCounter.Votes +
                                "/" + voteMessageCounter.UniqueVoteResponders + "");

                                VotingPostRound(40);

                                CurrentVotingState = VotingStates.ST60;
                            }

                            break;

                        case VotingStates.ST60:

                            if (!currentVotingRequestSent)
                            {
                                voteMap.Reset();
                                sendVoteRequests();
                                currentVotingRequestSent = true;
                            }

                            if (CheckReceivedExpectedVotePackets() == VoteNextState.Next)
                            {
                                Print("Voting Step60 Done" + voteMessageCounter.Votes +
                                "/" + voteMessageCounter.UniqueVoteResponders + "");

                                VotingPostRound(60);

                                CurrentVotingState = VotingStates.ST75;
                            }

                            break;

                        case VotingStates.ST75:

                            if (!currentVotingRequestSent)
                            {
                                voteMap.Reset();
                                sendVoteRequests();
                                currentVotingRequestSent = true;
                            }

                            if (CheckReceivedExpectedVotePackets() == VoteNextState.Next)
                            {
                                Print("Voting Step75 Done" + voteMessageCounter.Votes +
                                "/" + voteMessageCounter.UniqueVoteResponders + "");

                                VotingPostRound(75);
                                CurrentVotingState = VotingStates.ST80;
                            }

                            break;

                        case VotingStates.ST80:

                            if (!currentVotingRequestSent)
                            {
                                voteMap.Reset();
                                sendVoteRequests();
                                currentVotingRequestSent = true;
                            }

                            if (CheckReceivedExpectedVotePackets() == VoteNextState.Next)
                            {
                                Print("Voting Step80 Done" + voteMessageCounter.Votes +
                                "/" + voteMessageCounter.UniqueVoteResponders + "");

                                VotingPostRound(80);
                                CurrentVotingState = VotingStates.STDone;
                            }

                            break;

                        case VotingStates.STDone:
                            PostVotingOperations();
                            break;
                    }


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
                    else // Initial Sync Part is over.
                    {
                        // Verify the received
                        if (voteMessageCounter.Votes < voteMessageCounter.PreviousVotes)
                        {
                            // Okay, all the votes have not reached till now.
                            if (extraVotingDelayCycles < 10)
                            {
                                extraVotingDelayCycles++;

                                Print("Waiting for pending voting requests : " + voteMessageCounter.Votes +
                                    "/" + voteMessageCounter.PreviousVotes + " Received");
                            }
                        }
                    }

                    votingStateCounter++;

                    // We will perform dummy voting cycles even when there are no transactions. those will
                    // have a different counter, called ConsensusCount, the default is LedgerClose, the ledger close one 
                    // is the one associated.

                    // Request Ballots

                    if (votingStateCounter - extraVotingDelayCycles >= 12)
                    {
                        votingStateCounter = 0;
                        extraVotingDelayCycles = 0;

                        PostVotingOperations();

                        Print("Voting Finished. " + GetTxCount(finalBallot));
                    }
                }*/

        private void PostVotingOperations()
        {
            finalBallot.Reset(LedgerCloseSequence);
            finalBallot.PublicKey = nodeConfig.PublicKey;
            finalBallot.AddRange(voteMap.FilterTransactionsByVotes(ballot, Constants.CONS_FINAL_VOTING_THRESHOLD_PERC));
            finalBallot.Timestamp = nodeState.NetworkTime;

            finalBallot.UpdateSignature(nodeConfig.SignDataWithPrivateKey(finalBallot.GetSignatureData()));

            synchronizedVoters = voteMap.GetSynchronisedVoters(finalBallot);

            //finalVoters.Reset(LedgerCloseSequence); // Maybe repeat, but okay.

            isFinalBallotValid = true; // TODO: CRITICAL THINK THINK, TESTS !!                

            CurrentConsensusState = ConsensusStates.Apply; // SKIP CONFIRMATION (Maybe not needed afterall)
            CurrentVotingState = VotingStates.STNone;

            voteMessageCounter.ResetConfirmations();
        }

        /* void HandleConfirmation()
         {
             if (confirmationStateCounter < 1) // Send it once only.
             {
                 if (isFinalBallotValid)
                 {
                     // Send Confirmation
                     sendConfirmationRequests();
                 }
             }
             else
             {
                 if (voteMessageCounter.Confirmations < voteMessageCounter.PreviousConfirmations)
                 {
                     // Okay, all the votes have not reached till now.
                     if (extraConfirmationDelayCycles < 10)
                     {
                         extraConfirmationDelayCycles++;

                         Print("Waiting for pending confirmation requests : " + voteMessageCounter.Confirmations +
                             "/" + voteMessageCounter.PreviousConfirmations + " Received");
                     }
                 }
             }

             confirmationStateCounter++;

             if (confirmationStateCounter - extraConfirmationDelayCycles >= 6)
             {
                 confirmationStateCounter = 0;
                 extraConfirmationDelayCycles = 0;

                 isFinalConfirmedVotersValid = true;

                 CurrentConsensusState = ConsensusStates.Apply; // Verify Confirmation

                 voteMessageCounter.SetPreviousConfirmations();

                 Print("Confirm Finished. " + GetTxCount(finalBallot));
             }
         }*/

        bool NotEnoughVoters = false;

        void HandleApply()
        {
            // Have some delay before apply to allow others to catch up.
            if (isFinalBallotValid && (applyStateCounter == 1)) // DISABLE CONFIRMATION
            {
                NotEnoughVoters = false;

                // Check that the confirmed voters are all trusted

                int trustedSynchronizedVoters = 0;

                foreach (var voter in synchronizedVoters)
                {
                    if (nodeConfig.TrustedNodes.ContainsKey(voter))
                    {
                        trustedSynchronizedVoters++;
                    }
                }

                int totalTrustedConnections = nodeState.ConnectedValidators.Where(node => node.Value.IsTrusted).Count();

                if ((trustedSynchronizedVoters >= Constants.VOTE_MIN_VOTERS) &&
                    (totalTrustedConnections >= Constants.VOTE_MIN_VOTERS))
                {
                    double percentage = ((double)trustedSynchronizedVoters * 100.0) / (double)totalTrustedConnections;

                    if (percentage >= Constants.CONS_FINAL_VOTING_THRESHOLD_PERC)
                    {
                        PrintImpt("Voting Successful. Applying to ledger. " + GetTxCount(finalBallot) +
                            " | Consesus percentage: " + percentage);

                        ApplyToLedger(finalBallot);

                        //LedgerCloseSequence++;
                    }
                    else
                    {
                        PrintImpt("Voting Unsuccessful. Consesus percentage: " + percentage);
                    }
                }
                else
                {
                    PrintImpt("Voting Unsuccessful. Not Enough Trusted Voters. Trusted Voters: " + trustedSynchronizedVoters +
                        " Trusted Conns :" + totalTrustedConnections);

                    NotEnoughVoters = true;
                }

                // Print("Apply Finished. Consensus Finished.");
            }

            applyStateCounter++;

            if (applyStateCounter > 2)
            {
                if (NotEnoughVoters)
                    CurrentConsensusState = ConsensusStates.Sync;
                else
                    CurrentConsensusState = ConsensusStates.Collect;

                applyStateCounter = 0;
            }

        }

        void ApplyToLedger(Ballot applyBallot)
        {
            var transactions = CurrentTransactions.Where(d => applyBallot.Contains(d.Key)).Select(d => d.Value);

            TransactionHandlingData THD = transactionValidator.ValidateTransactions(transactions);

            if (THD.AcceptedTransactions.Any())
            {
                transactionValidator.ApplyTransactions(THD);

                PrintImpt("Applied " + THD.AcceptedTransactions.Count + " transaction !!!");

                if (THD.NewAccounts.Count > 0)
                    PrintImpt("Created " + THD.NewAccounts.Count + " account !!!");

                UpdateLCD();
            }

            // Remove Txns from current set !
            foreach (var tx in ballot)
            {
                if (CurrentTransactions.ContainsKey(tx))
                {
                    TransactionContent tc;
                    CurrentTransactions.TryRemove(tx, out tc);
                }
            }
        }

        #endregion

        #region Packet Handling

        async Task networkPacketSwitch_ConsensusEvent(NetworkPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.TPT_CONS_MERGE_REQUEST:
                    await processMergeRequest(packet);
                    break;
                case PacketType.TPT_CONS_MERGE_RESPONSE:
                    await processMergeResponse(packet);
                    break;
                case PacketType.TPT_CONS_MERGE_TX_FETCH_REQUEST:
                    await processFetchRequest(packet);
                    break;
                case PacketType.TPT_CONS_MERGE_TX_FETCH_RESPONSE:
                    processFetchResponse(packet);
                    break;

                ///////////////////////////////////////////

                case PacketType.TPT_CONS_SYNC_REQUEST:
                    await processSyncRequest(packet);
                    break;

                case PacketType.TPT_CONS_SYNC_RESPONSE:
                    processSyncResponse(packet);
                    break;

                case PacketType.TPT_CONS_VOTE_REQUEST:
                    await processVoteRequest(packet);
                    break;

                case PacketType.TPT_CONS_VOTE_RESPONSE:
                    await processVoteResponse(packet);
                    break;

                case PacketType.TPT_CONS_CONFIRM_REQUEST:
                    //processConfirmRequest(packet);
                    break;

                case PacketType.TPT_CONS_CONFIRM_RESPONSE:
                    //processConfirmResponse(packet);
                    break;
            }
        }

        #endregion

    }
}
