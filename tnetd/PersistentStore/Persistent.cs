
//
// @Author: Arpan Jati
// @Date: 16 March 2016
//

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
            AccountStore = new SQLiteAccountStore(nodeConfig);
            TransactionStore = new SQLiteTransactionStore(nodeConfig);
            CloseHistory = new SQLiteCloseHistory(nodeConfig);
        }
    }
}
