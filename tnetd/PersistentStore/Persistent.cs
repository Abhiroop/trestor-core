
//
// @Author: Arpan Jati
// @Date: 16-29 March 2016
//

using System.Data.SQLite;
using TNetD.Nodes;

namespace TNetD.PersistentStore
{
    class Persistent
    {
        public IPersistentAccountStore AccountStore { get; set; }
        public IPersistentTransactionStore TransactionStore { get; set; }
        public IPersistentCloseHistory CloseHistory { get; set; }

        public Persistent()
        {

        }

        public Persistent(IPersistentAccountStore accountStore,
                    IPersistentTransactionStore transactionStore,
                    IPersistentCloseHistory closeHistory)
        {
            AccountStore = accountStore;
            TransactionStore = transactionStore;
            CloseHistory = closeHistory;
        }

        public void InitializeSQLite(NodeConfig nodeConfig)
        {
            InitializeSQLite(nodeConfig, false);
        }

        public void InitializeSQLite(NodeConfig nodeConfig, bool isMemoryDB)
        {
            AccountStore = new SQLiteAccountStore(nodeConfig, isMemoryDB);
            TransactionStore = new SQLiteTransactionStore(nodeConfig, isMemoryDB);
            CloseHistory = new SQLiteCloseHistory(nodeConfig, isMemoryDB);
        }

        public void DeleteEverything()
        {
            TransactionStore.DeleteEverything();
            CloseHistory.DeleteEverything();
            AccountStore.DeleteEverything();
        }

        public void ExportToNodeSQLite(Node destinationNode)
        {
            var source_AccountStore = (SQLiteConnection)AccountStore.GetConnection();
            var source_TransactionStore = (SQLiteConnection)TransactionStore.GetConnection();
            var source_CloseHistory = (SQLiteConnection)CloseHistory.GetConnection();

            var dest_AccountStore = destinationNode.nodeState.Persistent.AccountStore.GetConnection();
            var dest_TransactionStore = destinationNode.nodeState.Persistent.TransactionStore.GetConnection();
            var dest_CloseHistory = destinationNode.nodeState.Persistent.CloseHistory.GetConnection();
            
            source_AccountStore.BackupDatabase((SQLiteConnection)dest_AccountStore, "main", "main", -1, null, -1);
            source_TransactionStore.BackupDatabase((SQLiteConnection)dest_TransactionStore, "main", "main", -1, null, -1);
            source_CloseHistory.BackupDatabase((SQLiteConnection)dest_CloseHistory, "main", "main", -1, null, -1);
        }
    }
}
