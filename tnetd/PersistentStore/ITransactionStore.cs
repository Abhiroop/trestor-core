//
// @Author: Arpan Jati
// @Date: Jan 5, 2015
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNetD.Transactions;

namespace TNetD.PersistentStore
{
    /// <summary>
    /// This is an interface to implement all the PersistentStorage techniques.
    /// Plans to support SQLite, LevelDB/RocksDB
    /// </summary>
    interface ITransactionStore
    {
        /// <summary>
        /// Adds or updates elements from the PersistentStore
        /// </summary>
        /// <param name="transactionContent"></param>
        /// <returns></returns>
        DBResponse AddUpdate(TransactionContent transactionContent);
        
        /// <summary>
        /// Deletes the transaction from the Persistent Store [Must not be used in practice !!!]
        /// </summary>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        DBResponse Delete(Hash transactionID);

        /// <summary>
        /// Fetches a transaction from the Persistent Store 
        /// </summary>
        /// <param name="transactionID"></param>
        /// <param name="transactionContent"></param>
        /// <returns></returns>
        DBResponse FetchTransaction(Hash transactionID, out TransactionContent transactionContent);

        /// <summary>
        /// Returns true if the transaction exists in the database.
        /// </summary>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        bool Exists(Hash transactionID);

    }
}

