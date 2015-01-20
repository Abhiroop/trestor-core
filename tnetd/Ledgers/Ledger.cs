
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.PersistentStore;
using TNetD.Transactions;
using TNetD.Tree;

namespace TNetD.Ledgers
{
    class Ledger
    {
        public enum LedgerEventType { Insert, Update, Progress };

        public delegate void LedgerEventHandler(LedgerEventType ledgerEvent, string Message);

        public event LedgerEventHandler LedgerEvent;
        
        IPersistentAccountStore persistentStore;

        /// <summary>
        /// A mapping between Adresses and Account. Later to be converted to DB based implementation.
        /// </summary>
        public Dictionary<string, AccountInfo> AddressAccountInfoMap = new Dictionary<string, AccountInfo>();

        /// <summary>
        /// A mapping between Name and Account.
        /// </summary>
        public Dictionary<string, AccountInfo> NameAccountInfoMap = new Dictionary<string, AccountInfo>();

        public ListHashTree LedgerTree = null;
        public long TransactionFees;
        public long TotalAmount;
        // public DateTime CloseTime;

        long _load_stats = 0;

        public long InitiallyLoadedNodes
        {
            get { return _load_stats; }
        }

        Dictionary<Hash, TransactionContent> newCandidates;

        /// <summary>
        /// List of public nodes which have sent bad transactions.
        /// </summary>
        public HashSet<Hash> BlackList = new HashSet<Hash>();

        public Ledger(IPersistentAccountStore persistentStore)
        {
            this.LedgerTree = new ListHashTree();
            this.persistentStore = persistentStore;
            this.TransactionFees = 0;
            this.TotalAmount = 0;
            //this.CloseTime = 0;

            //ledgerData = new LedgerData();
            newCandidates = new Dictionary<Hash, TransactionContent>();

            //Initial Load.           
        }

        public void InitializeLedger()
        {
             ReloadFromPersistentStore();
        }

        /// <summary>
        /// This will reset the entire state for the ledger, invalidating all the un-commited changes.
        /// TODO. CONSIDER USING A TREE DELETE/CLEAR OPERATION before the fetch.
        /// </summary>
        public long ReloadFromPersistentStore()
        {
            _load_stats = 0;
            AddressAccountInfoMap.Clear();
            NameAccountInfoMap.Clear();
            persistentStore.FetchAllAccounts(AddUserToLedger);
            return _load_stats;
        }

        /// <summary>
        /// Add User, false if user already exists.
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public bool AddUserToLedger(AccountInfo userInfo)
        {
            TreeResponseType response = LedgerTree.AddUpdate(userInfo);

            if (response == TreeResponseType.Added || response == TreeResponseType.Updated)
            {
                string address = userInfo.GetAddress();
                if (AddressAccountInfoMap.ContainsKey(address))
                {
                    AddressAccountInfoMap[address] = userInfo; // Update
                }
                else
                {
                    AddressAccountInfoMap.Add(address, userInfo); // Add
                }

                if (userInfo.Name.Length >= Constants.Pref_MinNameLength)
                {
                    if (NameAccountInfoMap.ContainsKey(userInfo.Name))
                    {
                        NameAccountInfoMap[userInfo.Name] = userInfo; // Update
                    }
                    else
                    {
                        NameAccountInfoMap.Add(userInfo.Name, userInfo); // Add
                    }
                }

                if ((_load_stats % 100 == 0) && (LedgerEvent != null))
                    LedgerEvent(LedgerEventType.Progress, "Loaded " + _load_stats + " Accounts.");

                _load_stats++;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Inserts the list of new transactions to the proposed candidate set.
        /// </summary>
        /// <param name="proposedTransactions"></param>
        public void PushNewProposedTransactions(TransactionContent[] proposedTransactions)
        {
            foreach (TransactionContent tc in proposedTransactions)
            {
                Hash tHash = tc.TransactionID;
                if (!newCandidates.ContainsKey(tHash))
                {
                    newCandidates.Add(tHash, tc);
                }
            }
        }

        /// <summary>
        /// Inserts the TransactionContent to the proposed candidate set.
        /// </summary>
        /// <param name="proposedTransactions"></param>
        public void PushNewCandidate(TransactionContent transaction)
        {
            Hash tID = transaction.TransactionID;
            if (!newCandidates.ContainsKey(tID))
            {
                newCandidates.Add(tID, transaction);
            }
        }

        public void RefreshValidTransactions()
        {
            //newCandidates = GetValidatedTransactions(newCandidates);
        }

        /// <summary>
        /// Gets an account from the tree.
        /// </summary>
        /// <param name="account">PublicKey of the account.</param>
        /// <returns></returns>
        public AccountInfo this[Hash account]
        {
            get
            {
                return (AccountInfo)LedgerTree[account];
            }
        }

        public bool AccountExists(Hash account)
        {
            return LedgerTree.NodeExists(account);
        }

        public Dictionary<Hash, TransactionContent> Candidates
        {
            get { return newCandidates; }
        }

        public Hash GetRootHash()
        {
            return LedgerTree.GetRootHash();
        }


    }
}
