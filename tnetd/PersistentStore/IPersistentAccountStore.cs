//
// @Author: Arpan Jati
// @Date: Jan 1-5 , 2015
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
    interface IPersistentAccountStore
    {
        /// <summary>
        /// Adds or updates elements from the PersistentStore
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
        
        DBResponse FetchAccount(Hash publicKey, out AccountInfo accountInfo);

        bool AccountExists(Hash publicKey);

    }
}
