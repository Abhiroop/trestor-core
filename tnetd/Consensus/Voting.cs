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
        int MergeStateCounter = 0;
        int VotingStateCounter = 0;
        int ConfirmationStateCounter = 0;

        public bool DebuggingMessages { get; set; }

        public bool Enabled { get; set; }

        private object VotingTransactionLock = new object();
        private object ConsensusLock = new object();
        
        private int previousRoundVoters = 0;

        public ConsensusStates CurrentConsensusState { get; private set; }

        NodeConfig nodeConfig;
        NodeState nodeState;
        NetworkPacketSwitch networkPacketSwitch;

        /// <summary>
        /// ID and content of current transactions
        /// </summary>
        ConcurrentDictionary<Hash, TransactionContent> CurrentTransactions;
        Ballot ballot;
        TransactionChecker transactionChecker = default(TransactionChecker);

        VoteMap voteMap;

        /// <summary>
        /// Set of nodes, who sent a transaction ID
        /// key: Transaction ID
        /// value: Set of nodes
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
            this.voteMap = new VoteMap();

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

            Print("class Voting created");
        }

        private void Print(string message)
        {
            if (DebuggingMessages)
                DisplayUtils.Display(" Node " + nodeConfig.NodeID + " | Voting: " + message);
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
                            isBallotValid = false;
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
            if (MergeStateCounter < 1) // TWEAK-POINT: Trim value.
            {
                SendMergeRequests();
            }

            MergeStateCounter++;
            
            // After 5 rounds: Assemble Ballot
            if (MergeStateCounter >= 5)
            {
                CreateBallot();

                isBallotValid = true; // Yayy.
                voteMap.Reset();
                CurrentConsensusState = ConsensusStates.Vote;
                MergeStateCounter = 0;
            }
        }

        private void CreateBallot()
        {
            // LCL + 1
            long ledgerCloseSequence = nodeState.Ledger.LedgerCloseData.SequenceNumber + 1;
            ballot = transactionChecker.CreateBallot(CurrentTransactions, ledgerCloseSequence);
        }

        void HandleVoting()
        {
            if (VotingStateCounter < 6) // TWEAK-POINT: Trim value.
            {
                if (VotingStateCounter % 2 == 0)
                {
                    SendBallotRequests();
                }
                else
                {
                    HashSet<Hash> missingTransactions;
                    voteMap.GetMissingTransactions(ballot, out missingTransactions);

                    sendFetchRequest()
                }
            }
            else
            {
                // Verify the received

            }

            VotingStateCounter++;

            // We will perform dummy voting cycles even when there are no transactions. those will
            // have a different counter, called ConsensusCount, the default is LedgerClose, the ledger close one 
            // is the one associated.

            // Request Ballots

            if (VotingStateCounter >= 6)
            {
                VotingStateCounter = 0;
                CurrentConsensusState = ConsensusStates.Confirm;

            }


        }

        void HandleConfirmation()
        {
            ConfirmationStateCounter++;

            CurrentConsensusState = ConsensusStates.Apply;
        }

        void HandleApply()
        {


            CurrentConsensusState = ConsensusStates.Collect;
        }

        #endregion


        #region Packet Handling

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
                case PacketType.TPT_CONS_MERGE_TX_FETCH_REQUEST:
                    ProcessFetchRequest(packet);
                    break;
                case PacketType.TPT_CONS_MERGE_TX_FETCH_RESPONSE:
                    ProcessFetchResponse(packet);
                    break;
            }
        }

        void networkPacketSwitch_VoteEvent(NetworkPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.TPT_CONS_STATE:
                    break;

                case PacketType.TPT_CONS_BALLOT_REQUEST:
                    ProcessBallotRequest(packet);
                    break;

                case PacketType.TPT_CONS_BALLOT_RESPONSE:
                    ProcessBallotResponse(packet);
                    break;

                case PacketType.TPT_CONS_VOTE_AGREE_REQUEST:

                    break;

                case PacketType.TPT_CONS_VOTE_AGREE_RESPONSE:
                    break;
            }
        }

        #endregion

    }
}
