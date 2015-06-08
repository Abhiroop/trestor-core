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

        public Ledger Ledger;

        public IPersistentAccountStore PersistentAccountStore;
        public IPersistentTransactionStore PersistentTransactionStore;
        public SQLiteBannedNames PersistentBannedNameStore;
        public SQLiteCloseHistory PersistentCloseHistory;

        public IncomingTransactionMap IncomingTransactionMap;

        public TransactionStateManager TransactionStateManager;
        
        public ConcurrentBag<Hash> GlobalBlacklistedValidators { get; set; }

        public ConcurrentBag<Hash> GlobalBlacklistedUsers { get; set; }

        public ConcurrentBag<Hash> ConnectedValidators { get; set; }

        public long SystemTime{ get; set; }

        public long NetworkTime{ get; set; }

        private long diff=0;
        
        public JS_NodeInfo NodeInfo;
    
        public NodeState(NodeConfig nodeConfig)
        {
            PersistentAccountStore = new SQLiteAccountStore(nodeConfig);
            PersistentTransactionStore = new SQLiteTransactionStore(nodeConfig);
            PersistentBannedNameStore = new SQLiteBannedNames(nodeConfig);
            PersistentCloseHistory = new SQLiteCloseHistory(nodeConfig);

            Ledger = new Ledger(PersistentAccountStore);

            TransactionStateManager = new TransactionStateManager();
            IncomingTransactionMap = new IncomingTransactionMap(this, nodeConfig, TransactionStateManager);
            
            GlobalBlacklistedValidators = new ConcurrentBag<Hash>();
            GlobalBlacklistedUsers = new ConcurrentBag<Hash>();
            
            ConnectedValidators = new ConcurrentBag<Hash>();
            SystemTime = DateTime.UtcNow.ToFileTimeUtc();
            NetworkTime = DateTime.UtcNow.ToFileTimeUtc();
        }

        public void updateNetworkTime()
        {
            NetworkTime = SystemTime + diff;
        }

        public void updateDiff(long diff)
        {
            this.diff = diff;
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
