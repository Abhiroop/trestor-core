//
// @Author: Arpan Jati
// @Date: Jan 5, 2015 | 21 Feb 2015 
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNetD.SyncFramework.Packets;
using TNetD.Transactions;

namespace TNetD.PersistentStore
{
    /// <summary>
    /// This is an interface to implement all the PersistentStorage techniques.
    /// Plans to support SQLite, LevelDB/RocksDB
    /// </summary>
    interface IPersistentTransactionStore
    {
        int AddUpdateBatch(Dictionary<Hash, TransactionContent> transactionContents, long sequenceNumber);

        int AddUpdateBatch(List<TransactionContentSet> transactionContentSets);

        /// <summary>
        /// Adds or updates elements to the PersistentStore
        /// </summary>
        /// <param name="transactionContent"></param>
        /// <returns></returns>
        //DBResponse AddUpdate(TransactionContent transactionContent);

        /// <summary>
        /// Deletes the transaction to the Persistent Store [Must not be used in practice !!!]
        /// </summary>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        DBResponse Delete(Hash transactionID);

        /// <summary>
        /// Fetches a transaction from the Persistent Store 
        /// </summary>
        /// <param name="transactionContent"></param>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        DBResponse FetchTransaction(out TransactionContent transactionContent, out long sequenceNumber, Hash transactionID);

        /// <summary>
        /// Fetches transaction history from the database. 
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="publicKey">Public Key of Account</param>
        /// <param name="TimeStamp">The time after which tranactions are needed</param>
        /// <param name="Limit">Max result count, 0 means all (Bounded by system limit.)</param>
        /// <returns></returns>
        DBResponse FetchTransactionHistory(out List<TransactionContent> transactions, Hash publicKey, long timeStamp, int Limit);

        /// <summary>
        /// Returns an object that can be sent over network so that the transaction history can be rebuild.
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="sequenceNumber"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        DBResponse FetchBySequenceNumber(out List<TransactionContentSet> transactions, long sequenceNumber, long Count);

        /// <summary>
        /// Returns true if the transaction exists in the database.
        /// </summary>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        bool Exists(Hash transactionID);
               
        /// <summary>
        /// Deletes everything in the DB. Returns 'DeleteFailed' if already empty.
        /// ONLY FOR TEST. DO NOT USE.
        /// </summary>
        /// <returns></returns>
        Tuple<DBResponse, long> DeleteEverything();

    }
}

