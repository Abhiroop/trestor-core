/*
 *  @Author: Arpan Jati
 *  @Version: 1.0  
 *  @Description: Node: It represents a full fledged transaction processor / validator / the complete thing.
 *  @Date: Oct 2015 | Jan-Feb 2015 | 8 June 2015
 *  FEB 10 2015 - Separate out the RPC Handlers
 */

using Chaos.NaCl;
using Grapevine;
using Grapevine.Server;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TNetD.Address;
using TNetD.Json.JS_Structs;
using TNetD.Ledgers;
using TNetD.Network;
using TNetD.Network.Networking;
using TNetD.PersistentStore;
using TNetD.Transactions;
using TNetD.Tree;
using TNetD.Types;
using TNetD.Time;
using TNetD.Network.PeerDiscovery;

namespace TNetD.Nodes
{
    public delegate void NodeStatusEventHandler(string Status, int NodeID);

    internal class Node
    {
        #region Locals

        public delegate void NodeSecondHandler();
        public event NodeSecondHandler NodeSecond;

        public event NodeStatusEventHandler NodeStatusEvent;

        bool TimerEventProcessed = true;
        bool MinuteEventProcessed = true;

        // TODO: MAKE PRIVATE : AND FAST
        public NodeConfig nodeConfig = default(NodeConfig);

        // TODO: MAKE PRIVATE : AND FAST
        public NodeState nodeState = default(NodeState);

        // TODO: MAKE PRIVATE (public only for testing)
        public PeerDiscovery peerDiscovery = default(PeerDiscovery);


        /// <summary>
        /// Handles all received network packets and dispatches them to various sub-systems.
        /// </summary>
        public NetworkPacketSwitch networkPacketSwitch = default(NetworkPacketSwitch);

        RpcHandlers rpcHandlers = default(RpcHandlers);        
        TransactionHandler transactionHandler = default(TransactionHandler);
        TimeSync timeSync = default(TimeSync);
        LedgerSync ledgerSync = default(LedgerSync);

        public AccountInfo AI;

        #endregion

        #region ConstructorsAndTimers

        public Ledger LocalLedger
        {
            get { return nodeState.Ledger; }
        }

        public Hash PublicKey
        {
            get
            {
                return nodeConfig.PublicKey;
            }
        }

        System.Timers.Timer TimerConsensus;
        System.Timers.Timer TimerFast;
        System.Timers.Timer TimerSecond;
        System.Timers.Timer TimerMinute;
        System.Timers.Timer TimerTimeSync;

        /// <summary>
        /// Initializes a node. Node ID is 0 for most cases.
        /// Only other use is hosting multiple validators from an IP (bad-idea) and simulation.
        /// </summary>
        /// <param name="ID"></param>
        public Node(int ID, GlobalConfiguration globalConfiguration)
        {
            nodeConfig = new NodeConfig(ID, globalConfiguration);

            nodeState = new NodeState(nodeConfig);

            nodeState.NodeInfo = nodeConfig.Get_JS_Info();

            rpcHandlers = new RpcHandlers(nodeConfig, nodeState);
            networkPacketSwitch = new NetworkPacketSwitch(nodeConfig, nodeState, globalConfiguration);
            transactionHandler = new TransactionHandler(nodeConfig, nodeState);
            timeSync = new TimeSync(nodeState, nodeConfig, networkPacketSwitch);
            ledgerSync = new LedgerSync(nodeState, nodeConfig, networkPacketSwitch);
            peerDiscovery = new PeerDiscovery(nodeState, nodeConfig, networkPacketSwitch);

            AI = new AccountInfo(PublicKey, Money);

            TimerConsensus = new System.Timers.Timer();
            TimerConsensus.Elapsed += TimerConsensus_Elapsed;
            TimerConsensus.Enabled = true;
            TimerConsensus.Interval = nodeConfig.UpdateFrequencyConsensusMS;
            TimerConsensus.Start();

            // ////////////////////

            TimerSecond = new System.Timers.Timer();
            TimerSecond.Elapsed += TimerSecond_Elapsed;
            TimerSecond.Enabled = true;
            TimerSecond.Interval = 100;
            TimerSecond.Start();

            TimerFast = new System.Timers.Timer();
            TimerFast.Elapsed += TimerFast_Elapsed;
            TimerFast.Enabled = true;
            TimerFast.Interval = 100;
            TimerFast.Start();

            TimerMinute = new System.Timers.Timer();
            TimerMinute.Elapsed += TimerMinute_Elapsed;
            TimerMinute.Enabled = true;
            TimerMinute.Interval = 30000;
            TimerMinute.Start();

            TimerTimeSync = new System.Timers.Timer();
            TimerTimeSync.Elapsed += TimerTimeSync_Elapsed;
            TimerTimeSync.Enabled = true;
            TimerTimeSync.Interval = 10000;
            TimerTimeSync.Start();

            DisplayUtils.Display("Started Node " + nodeConfig.NodeID, DisplayType.ImportantInfo);
        }

        void TimerTimeSync_Elapsed(object sender, ElapsedEventArgs e)
        {
            nodeState.updateTimeDifference(timeSync.SyncTime());
        }

        void TimerMinute_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (MinuteEventProcessed) // Lock to prevent multiple invocations
            {
                MinuteEventProcessed = false;

                try
                {
                    DateTime NOW = DateTime.UtcNow;

                    // Clean temporary proof of work Queue
                    // TODO : CRITICAL : Make sure the valid ones are removed, and not the critical ones.

                    Hash[] Kys = nodeState.WorkProofMap.Keys.ToArray();

                    foreach (Hash key in Kys)
                    {
                        DifficultyTimeData dtd;
                        if (nodeState.WorkProofMap.TryGetValue(key, out dtd))
                        {
                            TimeSpan span = (NOW - dtd.IssueTime);
                            if (span.TotalSeconds > 60) // 1 Minute
                            {
                                nodeState.WorkProofMap.Remove(key);
                            }
                        }
                    }

                    nodeState.TransactionStateManager.ProcessAndClear();

                }
                catch (Exception ex)
                {
                    DisplayUtils.Display("TimerMinute_Elapsed", ex);
                }
            }
            else
            {
                DisplayUtils.Display("Timer Expired : TimerMinute", DisplayType.Warning);
            }

            MinuteEventProcessed = true;
        }

        void TimerSecond_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (NodeSecond != null) NodeSecond();
        }

        void TimerFast_Elapsed(object sender, ElapsedEventArgs e)
        {
            nodeState.NodeInfo.NodeDetails.NetworkTime = DateTime.FromFileTimeUtc(nodeState.NetworkTime);
            nodeState.NodeInfo.NodeDetails.SystemTime = DateTime.FromFileTimeUtc(nodeState.SystemTime);

            nodeState.NodeInfo.LastLedgerInfo.Hash = nodeState.Ledger.GetRootHash().Hex;

            nodeState.NodeInfo.NodeDetails.ProofOfWorkQueueLength = nodeState.WorkProofMap.Count;

            nodeState.SystemTime = DateTime.UtcNow.ToFileTimeUtc();
            nodeState.updateNetworkTime();
            
            if (NodeStatusEvent != null)
            {
                var json = JsonConvert.SerializeObject(nodeState.NodeInfo.GetResponse(), Common.JsonSerializerSettings);
                NodeStatusEvent(json, nodeConfig.NodeID);
            }
        }

        /// <summary>
        /// Add more content to be loaded in background here.
        /// </summary>
        async public Task BeginBackgroundLoad()
        {
            await Task.Run(async () =>
            {
                long records = await nodeState.Ledger.InitializeLedger();

                Interlocked.Add(ref nodeState.NodeInfo.NodeDetails.TotalAccounts, records);

                await networkPacketSwitch.InitialConnectAsync();

                LedgerCloseData ledgerCloseData;
                nodeState.PersistentCloseHistory.GetLastRowData(out ledgerCloseData);
                nodeState.NodeInfo.LastLedgerInfo = new JS_LedgerInfo(ledgerCloseData);
            });
        }

        #endregion

        public void StopNode()
        {
            Constants.ApplicationRunning = false;

            networkPacketSwitch.Stop();
            rpcHandlers.StopServer();
        }


        #region TRANSACTION PROCESSING

        /// <summary>
        /// Timer now fast. Actual / Final timer would depend on consensus rate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerConsensus_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (TimerEventProcessed) // Lock to prevent multiple invocations
            {
                TimerEventProcessed = false;

                // // // // // // // // //

                transactionHandler.ProcessPendingTransactions();

                // // // // // // // // //
            }
            else
            {
                DisplayUtils.Display("Timer Expired : Consensus / Node", DisplayType.Warning);
            }

            TimerEventProcessed = true;
        }

        #endregion

        public async Task<long> CalculateTotalMoneyInPersistentStoreAsync()
        {
            long Tres = 0;

            await nodeState.PersistentAccountStore.FetchAllAccountsAsync((X) =>
            {
                Tres += X.Money;
                return TreeResponseType.NothingDone;
            });

            return Tres;
        }


        public async Task<long> CalculateTotalMoneyFromLedgerTreeAsync()
        {
            long Tres = 0;

            await Task.Run(() =>
            {
                long LeafDataCount = 0;
                long FoundNodes = 0;

                nodeState.Ledger.LedgerTree.TraverseAllNodes(ref LeafDataCount, ref FoundNodes, (X) =>
                {
                    foreach (AccountInfo AI in X)
                    {
                        Tres += AI.Money;
                    }
                    return TreeResponseType.NothingDone;
                });
            });

            return Tres;
        }

        public long Money
        {
            get
            {
                if (nodeState.Ledger != null)
                    if (nodeState.Ledger.AccountExists(PublicKey))
                        return nodeState.Ledger[PublicKey].Money;
                    else return -1;

                return -1;
            }
        }

        /*Stack<TransactionContentPack> PendingIncomingCandidates = new Stack<TransactionContentPack>();
        Stack<TransactionContentPack> PendingIncomingTransactions = new Stack<TransactionContentPack>();

        /// <summary>
        /// [TO BE CALLED BY OTHER NODES] Sends transactions to destination, only valid ones will be processed.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="Transactions"></param>
        public void SendTransaction(Hash source, TransactionContent Transaction)
        {
            PendingIncomingTransactions.Push(new TransactionContentPack(source, Transaction));
            InTransactionCount++;
        }

        /// <summary>
        /// [TO BE CALLED BY OTHER NODES] Sends candidates to destination [ONLY AFTER > 50% voting], only valid ones will be processed.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="Transactions"></param>
        public void SendCandidates(Hash source, TransactionContent[] Transactions)
        {
            if (TrustedNodes.ContainsKey(source))
            {
                foreach (TransactionContent tc in Transactions)
                {
                    PendingIncomingCandidates.Push(new TransactionContentPack(source, tc));
                    InCandidatesCount++;
                }
            }
        }*/


    }
}
