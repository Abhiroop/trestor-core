//
// @Author: Arpan Jati
// C++ Vesion: @Date: 22th Oct 2014 : To C# : 16th Jan 2015
// 10 Feb 2015: Added most of the common classes.
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Json.JS_Structs;
using TNetD.Ledgers;
using TNetD.Network.Networking;
using TNetD.PersistentStore;
using TNetD.Transactions;
using TNetD.Types;

namespace TNetD.Nodes
{
    /// <summary>
    /// Dynamic state information gathered during the running of the node.
    /// </summary>
    class NodeState
    {
        public Dictionary<Hash, DifficultyTimeData> WorkProofMap = new Dictionary<Hash, DifficultyTimeData>();

        public Ledger Ledger = default(Ledger);

        public IPersistentAccountStore PersistentAccountStore;
        public IPersistentTransactionStore PersistentTransactionStore;

        public SQLiteBannedNames PersistentBannedNameStore;
        public SQLiteCloseHistory PersistentCloseHistory;

        public IncomingTransactionMap IncomingTransactionMap;

        public TransactionStateManager TransactionStateManager;
        
        public ConcurrentBag<Hash> GlobalBlacklistedValidators { get; set; }

        public ConcurrentBag<Hash> GlobalBlacklistedUsers { get; set; }
        
        public ConcurrentDictionary<Hash, ConnectionProperties> ConnectedValidators { get; set; }

        /// <summary>
        /// Directory where a node's keyfiles etc. are stored
        /// </summary>
        public string privateDirectory;

        public Logging logger;

        public ConcurrentQueue<string> cq_logger;

        /// <summary>
        /// Key Token, Value: Time, Public key of recipient
        /// </summary>
        public ConcurrentDictionary<Hash, PendingNetworkRequest> PendingNetworkRequests;

        public NodeLatency NodeLatency;

        /// <summary>
        ///  Updated every 100ms
        /// </summary>
        public long SystemTime { get; set; }

        /// <summary>
        /// Updated every 100ms
        /// </summary>
        public long NetworkTime { get; set; }

        public JS_NodeInfo NodeInfo;
        
        private long timeDifference = 0;
        
        public NodeState(NodeConfig nodeConfig)
        {
            PersistentAccountStore = new SQLiteAccountStore(nodeConfig);
            PersistentTransactionStore = new SQLiteTransactionStore(nodeConfig);
            PersistentBannedNameStore = new SQLiteBannedNames(nodeConfig);
            PersistentCloseHistory = new SQLiteCloseHistory(nodeConfig);
            
            Ledger = new Ledger(PersistentAccountStore, PersistentCloseHistory, nodeConfig);

            TransactionStateManager = new TransactionStateManager();
            IncomingTransactionMap = new IncomingTransactionMap(this, nodeConfig, TransactionStateManager);
            
            GlobalBlacklistedValidators = new ConcurrentBag<Hash>();
            GlobalBlacklistedUsers = new ConcurrentBag<Hash>();

            ConnectedValidators = new ConcurrentDictionary<Hash, ConnectionProperties>();

            PendingNetworkRequests = new ConcurrentDictionary<Hash, PendingNetworkRequest>();

            NodeLatency = new NodeLatency(nodeConfig, this);

            SystemTime = DateTime.UtcNow.ToFileTimeUtc();
            NetworkTime = DateTime.UtcNow.ToFileTimeUtc();

            privateDirectory = "NODE_" + nodeConfig.NodeID;
            logger = new Logging(privateDirectory);
        }

        public void updateNetworkTime()
        {
            NetworkTime = SystemTime + timeDifference;
        }

        /// <summary>
        /// Re-calculate the precise newtwork time.
        /// Much more precise than the normal, SystemTime and NetworkTime, but, slightly expensive.
        /// </summary>
        public DateTime CurrentNetworkTime
        {    
            get { return DateTime.FromFileTimeUtc(DateTime.UtcNow.ToFileTimeUtc() + timeDifference); }
        }

        public void updateTimeDifference(long timeDifference)
        {
            this.timeDifference = timeDifference;
        }

        public bool IsGoodValidUserName(string Name)
        {
            if (!Utils.ValidateUserName(Name)) return false;

            if (PersistentBannedNameStore.Contains(Name)) return false;

            return true;
        }
        
        public void SetNodeInfo(JS_NodeInfo NodeInfo)
        {
            this.NodeInfo = NodeInfo;
        }

    }
}
