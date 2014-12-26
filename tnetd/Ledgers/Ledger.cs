using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Transactions;
using TNetD.Tree;

namespace TNetD.Ledgers
{
    class Ledger
    {
        public HashTree LedgerTree = null;
        public long TransactionFees;
        public long TotalAmount;
       // public DateTime CloseTime;

        Dictionary<Hash, TransactionContent> newCandidates;

        /// <summary>
        /// List of public nodes which have sent bad transactions.
        /// </summary>
        public HashSet<Hash> BlackList = new HashSet<Hash>();

        public Ledger()
        {
            this.LedgerTree = new HashTree();
            this.TransactionFees = 0;
            this.TotalAmount = 0;
            //this.CloseTime = 0;

            //ledgerData = new LedgerData();
            newCandidates = new Dictionary<Hash, TransactionContent>();
        }

        /// <summary>
        /// Add User, false if user already exists.
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public bool AddUserToLedger(AccountInfo userInfo)
        {
            // MAKE sure account does not exist
            bool contains = LedgerTree.NodeExists(userInfo.AccountID);

            if (!contains)
            {
                LedgerTree.AddUpdate(userInfo);
                return true;
            }
            else
            {
                return false;
            }
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

     

        public AccountInfo this[Hash account]
        {
            get
            {
                return (AccountInfo)LedgerTree[account];
            }
            //return ai.Money;
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
