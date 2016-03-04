
//
//  @Author: Arpan Jati | Stephan Verbuecheln | Abhiroop Sarkar
//  @Date: June 2015 | Jan 2016
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
using TNetD.Helpers;

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

        int syncAttempt = 0;
        int mergeAttempt = 0;
        int voteAttempt = 0;
        int voteInternalAttempt = 0;
        int voteFinalFailCounter = 0;

        public bool VerboseDebugging = false;

        public bool DebuggingMessages { get; set; }

        private bool enabled = false;

        public Tests.PacketLogger packetLogger = default(Tests.PacketLogger);

        public bool Enabled
        {
            get
            {
                return enabled;
            }

            set
            {
                enabled = value;
                timeStep.EventEnabled = value;
                InitLCS(nodeState); //FIX_TEMPORARY_TESTING MEASURE
                Init();
            }
        }

        private object VotingTransactionLock = new object();
        private object ConsensusLock = new object();

        public LedgerCloseSequence LedgerCloseSequence { get; private set; }

        public ConsensusStates CurrentConsensusState { get; private set; }

        public VotingStates CurrentVotingState = VotingStates.STNone;

        ConcurrentDictionary<Hash, ConsensusStates> stateMap;

        ConcurrentDictionary<Hash, VotingStates> votingStateMap;
        
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

        ConcurrentQueue<string> logger;

        VoteMessageCounter voteMessageCounter;

        public Voting(NodeConfig nodeConfig, NodeState nodeState, NetworkPacketSwitch networkPacketSwitch,ConcurrentQueue<string> logger)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;
            this.networkPacketSwitch = networkPacketSwitch;
            this.CurrentTransactions = new ConcurrentDictionary<Hash, TransactionContent>();
            this.propagationMap = new ConcurrentDictionary<Hash, HashSet<Hash>>();
            this.syncMap = new ConcurrentDictionary<Hash, SyncState>();
            this.voteMap = new VoteMap(nodeConfig, nodeState);
            this.synchronizedVoters = new HashSet<Hash>();
            this.timeStep = new TimeStep(nodeState);
            this.timeStep.Step += VotingEvent;
            this.packetLogger = new Tests.PacketLogger(nodeConfig, nodeState);
            this.logger = logger;

            //finalVoters = new FinalVoters();

            finalBallot = new Ballot();
            ballot = new Ballot();

            networkPacketSwitch.ConsensusEvent += networkPacketSwitch_ConsensusEvent;

            transactionChecker = new TransactionChecker(nodeState);
            transactionValidator = new TransactionValidator(nodeConfig, nodeState);

            CurrentConsensusState = ConsensusStates.Sync;

            voteMessageCounter = new VoteMessageCounter();



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
            this.timeStep = new TimeStep(nodeState);
            this.timeStep.Step += VotingEvent;
            this.packetLogger = new Tests.PacketLogger(nodeConfig, nodeState);

            //finalVoters = new FinalVoters();

            finalBallot = new Ballot();
            ballot = new Ballot();

            networkPacketSwitch.ConsensusEvent += networkPacketSwitch_ConsensusEvent;

            transactionChecker = new TransactionChecker(nodeState);
            transactionValidator = new TransactionValidator(nodeConfig, nodeState);

            CurrentConsensusState = ConsensusStates.Sync;

            voteMessageCounter = new VoteMessageCounter();



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

        private void Init()
        {
            List<ConsensusStates> states = Enumerable.Repeat(ConsensusStates.Sync, nodeState.ConnectedValidators.Count).ToList();
            var dictionary = nodeState.ConnectedValidators.Keys.Zip(states, (k, v) => new { Key = k, Value = v })
                                                           .ToDictionary(x => x.Key, x => x.Value);
            this.stateMap = new ConcurrentDictionary<Hash, ConsensusStates>(dictionary);


            List<VotingStates> votingStates = Enumerable.Repeat(VotingStates.STNone, nodeState.ConnectedValidators.Count).ToList();
            var dict = nodeState.ConnectedValidators.Keys.Zip(votingStates, (k, v) => new { Key = k, Value = v })
                                                           .ToDictionary(x => x.Key, x => x.Value);
            this.votingStateMap = new ConcurrentDictionary<Hash, VotingStates>(dict);
        }

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

        bool isBallotValid = false;
        bool isFinalBallotValid = false;
        bool isFinalConfirmedVotersValid = false;
        bool isAcceptMapValid = false;

        int syncStateCounter = 0;
        

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
                        HandleCollect();
                        break;
                    // LCS = LCL + 1  |  CRITICAL FIX !   
                    // ledgerCloseSequence = nodeState.Ledger.LedgerCloseData.SequenceNumber + 1;


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

        private void HandleCollect()
        {
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
            Thread.Sleep(500);
            CurrentConsensusState = ConsensusStates.Merge;
        }

        private async Task HandleSync()
        {
            await updateStateMap();
            //filter out trusted nodes here and then run the below check
            if (stateMap.Values.Distinct().Count()==1 && stateMap.Values.First()==CurrentConsensusState)
            {
                syncStateWork();
            }
            else
            {
                syncAttempt++;
                if(syncAttempt>10) //preventing deadlock; this happens in case of message loss
                {
                    Print("Couldn't Sync");
                    await sendSyncRequest();
                    if (stateMap.Values.Where(x => ((x == ConsensusStates.Collect) || (x == ConsensusStates.Merge))).Count() >= Constants.VOTE_MIN_SYNC_NODES)
                    {
                        syncStateWork();
                        syncAttempt = 0;
                    }
                }
                if (stateMap.Count != nodeState.ConnectedValidators.Count)
                    Init();
            }
        }

        private void syncStateWork()
        {
            syncAttempt = 0;
            Print("Sync Done. Normal.");
            Thread.Sleep(500);
            CurrentConsensusState = ConsensusStates.Collect;
        }


        /// <summary>
        /// Check if all my friends have the same state map if not send them my current state and 
        /// keep checking again until failure is handled. If after 3/5 attempts it doesn't work
        /// assume the machine has failed. And handle that round accordingly.
        /// </summary>
        private void handleFailure()
        {
            //optimistic failure handling sending 2 messages
            switch(CurrentConsensusState)
            {
                case ConsensusStates.Sync:
                    Task.Run(() => sendSyncRequest(stateMap.Keys.ToList()));
                    break;
                case ConsensusStates.Merge:
                    Task.Run(() => sendSyncRequest(stateMap.Keys.ToList()));
                    break;

            }
        }

        string GetTxCount(Ballot ballot)
        {
            if (ballot.TransactionCount > 0)
            {

                return "" + ballot.TransactionCount + " Txns";
            }

            return "";
        }

        private async Task HandleMerge()
        {
            await updateStateMap();
            if (stateMap.Values.Distinct().Count() == 1 && stateMap.Values.First() == CurrentConsensusState)
            {
                mergeStateWork();
            }
            else
            {
                mergeAttempt++;
                if (mergeAttempt > 10) 
                {
                    await sendMergeRequests();
                    if (stateMap.Values.Where(x => (x == ConsensusStates.Vote)).Count() >= Constants.VOTE_MIN_SYNC_NODES)
                    {
                        mergeAttempt = 0;
                        mergeStateWork();
                    }
                }
            }
        }

        private void mergeStateWork()
        {
            //handleFailure();
            mergeAttempt = 0;
            CreateBallot();
            isBallotValid = true; // Yayy.
            voteMap.Reset();
            voteMessageCounter.ResetVotes();
            Print("Merge Finished. " + GetTxCount(ballot));

            Thread.Sleep(500);
            //logger.Enqueue(nodeConfig.NodeID + "-Merge Finished");
            CurrentConsensusState = ConsensusStates.Vote;
        }

        /// <summary>
        /// This method blocks until every/necessary no of node arrives at the necessary state
        /// </summary>
        /// <param name="currentConsensusState"></param>
        private async Task updateStateMap()
        {
            var filteredFriendHashes = stateMap.Where(x => x.Value != CurrentConsensusState)
                                          .ToDictionary(x => x.Key, x => x.Value)
                                          .Keys
                                          .ToList();
            //another layer of filtering will happen based on the trusted nodes
            if (CurrentConsensusState == ConsensusStates.Sync)
            {
                await sendSyncRequest(filteredFriendHashes); 
            }

            if (CurrentConsensusState==ConsensusStates.Merge)
            {
                await sendMergeRequests(filteredFriendHashes);
            }
            
            if(CurrentConsensusState==ConsensusStates.Vote)
            {
                await sendVoteRequests(filteredFriendHashes);
            }
        }

        private void CreateBallot()
        {
            ballot = transactionChecker.CreateBallot(CurrentTransactions, LedgerCloseSequence);
            ballot.UpdateSignature(nodeConfig.SignDataWithPrivateKey(ballot.GetSignatureData()));
        }

        int MAX_EXTRA_VOTING_STEP_WAIT_CYCLES = 5;
        int extraVotingDelayCycles = 0; // Wait for all the voters to send their requests.
      

        //int extraConfirmationDelayCycles = 0;

        enum VoteNextState { Wait, Next }

        async Task<VoteNextState> WaitForPendingVotesIfNeeded()
        {
            int waitCount = 0;

            while (waitCount < MAX_EXTRA_VOTING_STEP_WAIT_CYCLES)
            {
                if (voteMessageCounter.Votes < voteMessageCounter.UniqueVoteResponders)
                {
                    waitCount++;

                    Print("Waiting for pending voting requests : " + voteMessageCounter.Votes +
                           "/" + voteMessageCounter.UniqueVoteResponders + " Received");

                    await Task.Delay(30);
                }
                else
                {
                    return VoteNextState.Next;
                }
            }

            return VoteNextState.Next;
        }

        async Task VotingPostRound(VotingStates state, float Percentage)
        {
            voteInternalAttempt = 0;
            extraVotingDelayCycles = 0;
            Dictionary<Hash, HashSet<Hash>> missingTransactions;
            voteMap.GetMissingTransactions(ballot, out missingTransactions);

            if(missingTransactions.Count>0)
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

            CurrentVotingState = state + 1;
        }

        async Task HandleVotingInternal(VotingStates state, float percentage)
        {
            await updateVoteMap();
            if (votingStateMap.Values.Distinct().Count() == 1 && votingStateMap.Values.First() == CurrentVotingState)
                await VotingPostRound(state, percentage);
            else
            {
                voteInternalAttempt++;
                //this activity is for nodes that are behind
                //once we fail to enter the "if" a FEW times then do this
                if (voteInternalAttempt > 9)
                {
                    await sendVoteRequests(); //this will update stateMap
                    //now calculate the median and move on if you really want to
                    if (calculateMedianStateOfMap(votingStateMap))
                    {
                        CurrentVotingState += 1; //NEW ISSUE: In case of ST80-> STDONE problematic
                    }
                    voteInternalAttempt = 0;
                }
            }
        }

        private bool calculateMedianStateOfMap(ConcurrentDictionary<Hash,VotingStates> map) 
        {
            var filtered = map.Values.Where(x => x==(CurrentVotingState+1));
            if (filtered.Count() >= Constants.VOTE_MIN_SYNC_NODES)
                return true;
            return false;
        }

        private async Task updateVoteMap()
        {
            var filteredFriendHashes = votingStateMap.Where(x => x.Value != CurrentVotingState)
                                          .ToDictionary(x => x.Key, x => x.Value)
                                          .Keys
                                          .ToList();
            //another layer of filtering will happen based on the trusted nodes
            await sendVoteRequests(filteredFriendHashes);
            
        }

        async Task HandleVoting()
        {
            /*
            await updateStateMap();
            if (stateMap.Values.Distinct().Count() == 1 && stateMap.Values.First() == CurrentConsensusState)
            {*/
                switch (CurrentVotingState)
                {
                    case VotingStates.STNone:
                        // Pre-Voting Stuff !! 
                        voteAttempt = 0;
                        voteMap.Reset();
                        CurrentVotingState = VotingStates.ST40;
                        break;

                    case VotingStates.ST40:
                        await HandleVotingInternal(CurrentVotingState, 40);
                        break;

                    case VotingStates.ST60:
                        await HandleVotingInternal(CurrentVotingState, 60);
                        break;

                    case VotingStates.ST75:
                        await HandleVotingInternal(CurrentVotingState, 75);
                        break;

                    case VotingStates.ST80:
                        await HandleVotingInternal(CurrentVotingState, 80);
                        break;

                    case VotingStates.STDone:
                        await barrierCheck();
                        break;
                }
            //}
            /*
            else
            {
                voteAttempt++;
                if (voteAttempt > 10) //preventing deadlock; this happens in case of message loss
                {
                    //ignoring if I am behind case
                    //Assume I am forward
                    //filter out those nodes which are not allowing entry and send a request to them
                    voteAttempt = 0;
                }
            }*/
        }

        private async Task barrierCheck()
        {
            await updateVoteMap();
            if (votingStateMap.Values.Distinct().Count() == 1 && votingStateMap.Values.First() == CurrentVotingState)
            {
                postVotingOperations();
            }
            else
            {
                //after I fail to enter multiple number of times I can check state map to verify if friends are in APPLY or SYNC
                voteFinalFailCounter++;
                if (voteFinalFailCounter > 10)
                {
                    await sendVoteRequests();
                    if(stateMap.Values.Where(x=> ((x== ConsensusStates.Apply) || (x == ConsensusStates.Sync))).Count() >= Constants.VOTE_MIN_SYNC_NODES)
                    {
                        voteFinalFailCounter = 0;
                        postVotingOperations();
                    }
                }

            }
        }

        private void postVotingOperations()
        {
            voteFinalFailCounter = 0;
            finalBallot.Reset(LedgerCloseSequence);
            finalBallot.PublicKey = nodeConfig.PublicKey;
            finalBallot.AddRange(voteMap.FilterTransactionsByVotes(ballot, Constants.CONS_FINAL_VOTING_THRESHOLD_PERC));
            finalBallot.Timestamp = nodeState.NetworkTime;

            finalBallot.UpdateSignature(nodeConfig.SignDataWithPrivateKey(finalBallot.GetSignatureData()));

            synchronizedVoters = voteMap.GetSynchronisedVoters(finalBallot);

            //finalVoters.Reset(LedgerCloseSequence); // Maybe repeat, but okay.

            isFinalBallotValid = true; // TODO: CRITICAL THINK THINK, TESTS !!                

            //logger.Enqueue(nodeConfig.NodeID + "-Voting Done. Normal.");
            Print(nodeConfig.NodeID + "-Voting Done. Normal.");
            CurrentConsensusState = ConsensusStates.Apply; // SKIP CONFIRMATION (Maybe not needed afterall)
            CurrentVotingState = VotingStates.STNone;

            voteMessageCounter.ResetConfirmations();

        }

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
            Thread.Sleep(1500);
            CurrentConsensusState = ConsensusStates.Sync;

            //if (applyStateCounter > 2)
            //{
            //    if (NotEnoughVoters)
            //        CurrentConsensusState = ConsensusStates.Sync;
            //    else
            //        CurrentConsensusState = ConsensusStates.Collect;

            //    applyStateCounter = 0;
            //}

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
                {
                    PrintImpt("Created " + THD.NewAccounts.Count + " account !!!");
                }

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
                    processVoteResponse(packet);
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
