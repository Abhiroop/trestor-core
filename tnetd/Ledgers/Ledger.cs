/*
 *  @Author: Arpan Jati
 *  @Version: 1.0
 *  @Date: Initial Versions: Oct - Jan 2015 | June 2015
 *  22 Jan 2015 : AddUpdateBatch
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Nodes;
using TNetD.PersistentStore;
using TNetD.Transactions;
using TNetD.Tree;

namespace TNetD.Ledgers
{
    class Ledger
    {
        public enum LedgerEventType { Insert, Update, Progress };

        LedgerCloseData ledgerCloseData = default(LedgerCloseData);        

        public delegate void LedgerEventHandler(LedgerEventType ledgerEvent, string Message);

        public event LedgerEventHandler LedgerEvent;
        public NodeConfig nodeConfig;

        IPersistentAccountStore persistentStore;
        IPersistentCloseHistory persistentCloseHistory;

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
        bool initialLoading = false;

        public long InitiallyLoadedNodes
        {
            get { return _load_stats; }
        }
        
        public Ledger(IPersistentAccountStore persistentStore, IPersistentCloseHistory persistentCloseHistory, NodeConfig nodeConfig)
        {
            this.LedgerTree = new ListHashTree();
            this.persistentStore = persistentStore;
            this.nodeConfig = nodeConfig;
            this.persistentCloseHistory = persistentCloseHistory;
            this.TransactionFees = 0;
            this.TotalAmount = 0;
            this.ledgerCloseData = new LedgerCloseData();
        }

        public LedgerCloseData LedgerCloseData
        {
            get
            {
                return ledgerCloseData;
            }

            set
            {
                ledgerCloseData = value;
            }
        }

        async public Task<long> InitializeLedger()
        {
            initialLoading = true;

            long records = await ReloadFromPersistentStore();

            if (LedgerEvent != null)
            {
                LedgerEvent(LedgerEventType.Progress, "Ready");
            }

            bool ok = persistentCloseHistory.GetLastRowData(out ledgerCloseData);
            
            initialLoading = false;

            return records;
        }

        /// <summary>
        /// This will reset the entire state for the ledger, invalidating all the un-commited changes.
        /// TODO. CONSIDER USING A TREE DELETE/CLEAR OPERATION before the fetch.
        /// </summary>
        async public Task<long> ReloadFromPersistentStore()
        {
            _load_stats = 0;
            AddressAccountInfoMap.Clear();
            NameAccountInfoMap.Clear();
            Tuple<DBResponse,long> result = await persistentStore.FetchAllAccountsAsync(AddUserToLedger);
            return result.Item2;
        }

        /// <summary>
        /// Batch add accounts to LedgerTree
        /// </summary>
        /// <param name="accountInfoData"></param>
        /// <returns>Numbe of accounts added/updated</returns>
        public int AddUpdateBatch(IEnumerable<AccountInfo> accountInfoData)
        {
            int Successes = 0;

            foreach (AccountInfo ai in accountInfoData)
            {
                TreeResponseType resp = AddUserToLedger(ai);
                if ((resp == TreeResponseType.Added) || (resp == TreeResponseType.Updated))
                {
                    Successes++;
                }
            }

            return Successes;
        }

        /// <summary>
        /// TODO: PROPER TESTBENCH { CRITICAL }
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public TreeResponseType AddUserToLedger(AccountInfo userInfo)
        {
            DisplayUtils.Display("NODE_" + nodeConfig.NodeID + 
                " Found Account: " + Newtonsoft.Json.JsonConvert.SerializeObject(userInfo) + "\n OUR ROOT " + this.LedgerTree.GetRootHash());

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

                if (Utils.ValidateUserName( userInfo.Name))
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

                if (initialLoading)
                {
                    if ((_load_stats % 100 == 0) && (LedgerEvent != null))
                        LedgerEvent(LedgerEventType.Progress, "Loaded " + _load_stats + " Accounts.");

                    _load_stats++;
                }

            }

            return response;
        }


        public int BatchFetch(out  Dictionary<Hash, AccountInfo> accountInfoList, IEnumerable<Hash> accountPKs)
        {
            accountInfoList = new Dictionary<Hash, AccountInfo>();

            foreach (Hash accountPK in accountPKs)
            {
                if (LedgerTree.NodeExists(accountPK))
                {
                    accountInfoList.Add(accountPK, (AccountInfo)LedgerTree[accountPK]);
                }
            }

            return accountInfoList.Count();
        }

        /// <summary>
        /// Gets/Sets an account from the tree.
        /// </summary>
        /// <param name="account">PublicKey of the account.</param>
        /// <returns></returns>
        public AccountInfo this[Hash account]
        {
            get
            {
                return (AccountInfo)LedgerTree[account];
            }
            set
            {
                LedgerTree.AddUpdate(value);
            }
        }

        /// <summary>
        /// Returns true if the account exists in the Hash Tree.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public bool AccountExists(Hash account)
        {
            return LedgerTree.NodeExists(account);
        }

        public Hash GetRootHash()
        {
            return LedgerTree.GetRootHash();
        }

        public bool TryFetch(Hash publicKey, out AccountInfo account)
        {
            LeafDataType ldt ;
            if (LedgerTree.GetNodeData(publicKey, out ldt) == TraverseResult.Success)
            {
                account = (AccountInfo)ldt;
                return true;
            }
            account = new AccountInfo();
            return false;
        }


    }
}
