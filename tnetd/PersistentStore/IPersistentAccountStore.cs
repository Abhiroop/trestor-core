//
// @Author: Arpan Jati
// @Date: Jan 1-5 , 2015 | Jan 15 2015
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNetD.Transactions;

namespace TNetD.PersistentStore
{
    public delegate void AccountFetchEventHandler(AccountInfo accountInfo);

    /// <summary>
    /// This is an interface to implement all the PersistentStorage techniques.
    /// Plans to support SQLite, LevelDB/RocksDB
    /// </summary>
    interface IPersistentAccountStore
    {
        /// <summary>
        /// Adds or updates elements to the PersistentStore
        /// </summary>
        /// <param name="accountInfoData"></param>
        /// <returns></returns>
        int AddUpdateBatch(List<AccountInfo> accountInfoData);

        /// <summary>
        /// Adds or updates elements to the PersistentStore
        /// </summary>
        /// <param name="accountInfo"></param>
        /// <returns></returns>
        DBResponse AddUpdate(AccountInfo accountInfo);

        /// <summary>
        /// Deletes an element from the PersistentStore
        /// </summary>
        /// <param name="PublicKey"></param>
        /// <returns></returns>
        DBResponse Delete(Hash publicKey);

        /// <summary>
        /// Fetches a single account given a public key.
        /// </summary>
        /// <param name="accountInfo"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        DBResponse FetchAccount(out AccountInfo accountInfo, Hash publicKey);

        /// <summary>
        /// Fetches all accounts in the associated Database. Order is not guranteed.
        /// </summary>
        /// <param name="accountFetch"></param>
        /// <returns></returns>
        Tuple<DBResponse, long> FetchAllAccounts(AccountFetchEventHandler accountFetch);

        /// <summary>
        /// Returns true if the account exists.
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        bool AccountExists(Hash publicKey);
        
        /// <summary>
        /// Deletes everything in the DB. Returns 'DeleteFailed' if already empty.
        /// ONLY FOR TEST. DO NOT USE.
        /// </summary>
        /// <returns></returns>
        Tuple<DBResponse, long> DeleteEverything();

    }
}
