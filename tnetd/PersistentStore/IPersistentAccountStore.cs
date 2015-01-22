//
// @Author: Arpan Jati
// @Date: Jan 1-5 , 2015 | Jan 15 2015
// Jan 22 2015 : BatchFetch, IEnumaerables

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Transactions;

namespace TNetD.PersistentStore
{
    public delegate bool AccountFetchEventHandler(AccountInfo accountInfo);

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
        int AddUpdateBatch(IEnumerable<AccountInfo> accountInfoData);

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
        /// Fetches a list of Specified User Accounts.
        /// </summary>
        /// <param name="accountInfoList"></param>
        /// <param name="AccountPKs"></param>
        /// <returns>Number of accounts successfully fetched.</returns>
        int BatchFetch(out List<AccountInfo> accountInfoList, IEnumerable<Hash> AccountPKs);

        /// <summary>
        /// Fetches all accounts in the associated Database. Order is not guranteed.
        /// </summary>
        /// <param name="accountFetch"></param>
        /// <returns></returns>
        Task<Tuple<DBResponse, long>> FetchAllAccounts(AccountFetchEventHandler accountFetch);

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
