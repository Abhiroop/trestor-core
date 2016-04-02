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
using TNetD.Consensus;
using TNetD.Helpers;
using System.Collections.Concurrent;

namespace TNetD.Nodes
{
    public delegate void NodeStatusEventHandler(string Status, int NodeID);

    internal class Node : IDisposable
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

        public Voting voting = default(Voting);

        RpcHandlers rpcHandlers = default(RpcHandlers);
        TransactionHandler transactionHandler = default(TransactionHandler);
        TimeSync timeSync = default(TimeSync);
        LedgerSync ledgerSync = default(LedgerSync);

        NodeDetailHandler nodeDetailHandler = default(NodeDetailHandler);

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

        System.Timers.Timer timerConsensus;
        System.Timers.Timer timerFast;
        System.Timers.Timer timerSecond;
        System.Timers.Timer timerMinute;
        System.Timers.Timer timerTimeSync;

        /// <summary>
        /// Initializes a node. Node ID is 0 for most cases.
        /// Only other use is hosting multiple validators from an IP (bad-idea) and simulation.
        /// </summary>
        /// <param name="ID">NodeID</param>
        public Node(int ID) : this(ID, false) { }

        /// <summary>
        /// Initializes a node. Node ID is 0 for most cases.
        /// Only other use is hosting multiple validators from an IP (bad-idea) and simulation.
        /// </summary>
        /// <param name="ID">NodeID</param>      
        /// <param name="isMemoryDB">True to use in-memory persistent databases (for testing).</param>
        public Node(int ID, bool isMemoryDB)
        {
            nodeConfig = new NodeConfig(ID);

            nodeState = new NodeState(nodeConfig, isMemoryDB);

            nodeState.NodeInfo = nodeConfig.Get_JS_Info();

            rpcHandlers = new RpcHandlers(nodeConfig, nodeState);
            networkPacketSwitch = new NetworkPacketSwitch(nodeConfig, nodeState);
            transactionHandler = new TransactionHandler(nodeConfig, nodeState);

            nodeDetailHandler = new NodeDetailHandler(nodeConfig, nodeState);

            timeSync = new TimeSync(nodeState, nodeConfig, networkPacketSwitch);
            ledgerSync = new LedgerSync(nodeState, nodeConfig, networkPacketSwitch);
            peerDiscovery = new PeerDiscovery(nodeState, nodeConfig, networkPacketSwitch);

            peerDiscovery.AddKnownPeer(new PeerData(new NodeSocketData(nodeConfig.PublicKey, 
                nodeConfig.ListenPortProtocol, "127.0.0.1", nodeConfig.Name), nodeState, nodeConfig));

            voting = new Voting(nodeConfig, nodeState, networkPacketSwitch);

            networkPacketSwitch.HeartbeatEvent += NetworkPacketSwitch_HeartbeatEvent;

            if (Common.NODE_OPERATION_TYPE == NodeOperationType.Centralized)
            {
                timerConsensus = new System.Timers.Timer();
                timerConsensus.Elapsed += TimerConsensus_Elapsed;
                timerConsensus.Enabled = true;
                timerConsensus.Interval = nodeConfig.UpdateFrequencyConsensusMS;
            }

            timerSecond = new System.Timers.Timer();
            timerSecond.Elapsed += TimerSecond_Elapsed;
            timerSecond.Enabled = true;
            timerSecond.Interval = 1000;

            timerFast = new System.Timers.Timer();
            timerFast.Elapsed += TimerFast_Elapsed;
            timerFast.Enabled = true;
            timerFast.Interval = 100;

            timerMinute = new System.Timers.Timer();
            timerMinute.Elapsed += TimerMinute_Elapsed;
            timerMinute.Enabled = true;
            timerMinute.Interval = 30000;

            timerTimeSync = new System.Timers.Timer();
            timerTimeSync.Elapsed += TimerTimeSync_Elapsed;
            timerTimeSync.Enabled = true;
            timerTimeSync.Interval = 1 * 30 * 1000;

            //peerDiscovery.Start(30 * 1000);
            StartNode();
        }

        public void StartNode()
        {
            timerConsensus.Start();
            timerSecond.Start();
            timerFast.Start();
            timerMinute.Start();
            timerTimeSync.Start();

            var detailHandlerTask = nodeDetailHandler.Load();            

            DisplayUtils.Display("Started Node " + nodeConfig.NodeID, DisplayType.ImportantInfo);
        }

        /// <summary>
        /// Currently this does not entirely stop/destroy everything.
        /// The network handlers are closed, so the application can exit properly.
        /// Should be called at the time of application exit.
        /// </summary>
        public void StopNode()
        {
            var detailHandlerTask = nodeDetailHandler.Save();

            timerConsensus.Stop();
            timerSecond.Stop();
            timerFast.Stop();
            timerMinute.Stop();
            timerTimeSync.Stop();

            networkPacketSwitch.Stop();
            rpcHandlers.StopServer();

            DisplayUtils.Display("Stopped Node " + nodeConfig.NodeID, DisplayType.ImportantInfo);
        }

        public bool VotingEnabled
        {
            get { return voting.Enabled; }
            set { voting.Enabled = value; }
        }

        public bool LedgerSyncEnabled
        {
            get { return ledgerSync.Enabled; }
            set { ledgerSync.Enabled = value; }
        }

        void TimerTimeSync_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Common.NODE_OPERATION_TYPE == NodeOperationType.Distributed)
            {
                nodeState.updateTimeDifference(timeSync.SyncTime().Result);
                var heartbeatTask = sendHeartbeatRequests(); // For Testing
            }
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

                    nodeState.NodeLatency.Prune();

                    var detailHandlerTask = nodeDetailHandler.Save();
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
            NodeSecond?.Invoke();
        }

        void TimerFast_Elapsed(object sender, ElapsedEventArgs e)
        {
            nodeState.NodeInfo.NodeDetails.NetworkTime = DateTime.FromFileTimeUtc(nodeState.NetworkTime);
            nodeState.NodeInfo.NodeDetails.SystemTime = DateTime.FromFileTimeUtc(nodeState.SystemTime);

            nodeState.NodeInfo.LastLedgerInfo.Hash = nodeState.Ledger.RootHash.Hex;

            nodeState.NodeInfo.NodeDetails.ProofOfWorkQueueLength = nodeState.WorkProofMap.Count;

            nodeState.SystemTime = DateTime.UtcNow.ToFileTimeUtc();
            nodeState.updateNetworkTime();

            if (NodeStatusEvent != null)
            {
                var json = JsonConvert.SerializeObject(nodeState.NodeInfo.GetResponse(), Common.JSON_SERIALIZER_SETTINGS);
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

                nodeState.NodeInfo.NodeDetails.TotalAccounts = records;

                if (Common.NODE_OPERATION_TYPE == NodeOperationType.Distributed)
                {
                    await networkPacketSwitch.InitialConnectAsync();
                }

                LedgerCloseData ledgerCloseData;
                nodeState.Persistent.CloseHistory.GetLastRowData(out ledgerCloseData);
                nodeState.NodeInfo.LastLedgerInfo = new JS_LedgerInfo(ledgerCloseData);
            });
        }

        #endregion


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

                if (Common.NODE_OPERATION_TYPE == NodeOperationType.Centralized)
                {
                    transactionHandler.ProcessPendingTransactions();
                }
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

            await nodeState.Persistent.AccountStore.FetchAllAccountsAsync((X) =>
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

        async Task sendHeartbeatRequests()
        {
            foreach (var node in nodeState.ConnectedValidators)
            {
                Hash token = TNetUtils.GenerateNewToken();

                await networkPacketSwitch.SendAsync(node.Key, new NetworkPacket()
                {
                    PublicKeySource = nodeConfig.PublicKey,
                    Token = token,
                    Type = PacketType.TPT_HEARTBEAT_REQUEST
                });
            }

            // if (VerboseDebugging) Print("Sync requests sent to " + nodeState.ConnectedValidators.Count + " Nodes");
        }

        async Task NetworkPacketSwitch_HeartbeatEvent(NetworkPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.TPT_HEARTBEAT_REQUEST:
                    {
                        Hash sender = packet.PublicKeySource;
                        Hash token = packet.Token;

                        HeartbeatMessage heartbeatMessage = new HeartbeatMessage();

                        heartbeatMessage.ConsensusState = voting.Params.ConsensusState;
                        heartbeatMessage.VotingState = voting.Params.VotingState;
                        heartbeatMessage.LCS = voting.Params.LCS;

                        await networkPacketSwitch.SendAsync(sender, new NetworkPacket()
                        {
                            Token = token,
                            PublicKeySource = nodeConfig.PublicKey,
                            Data = heartbeatMessage.Serialize(),
                            Type = PacketType.TPT_HEARTBEAT_RESPONSE
                        });
                    }
                    break;

                case PacketType.TPT_HEARTBEAT_RESPONSE:
                    {
                        HeartbeatMessage heartbeatMessage = new HeartbeatMessage();
                        heartbeatMessage.Deserialize(packet.Data);

                        DisplayUtils.Display("Received Heartbeat: " + heartbeatMessage.VotingState, DisplayType.ImportantInfo);
                    }
                    break;
            }

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    StopNode();


                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Node() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
