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

        /*
        /// <summary>
        /// Inserts the list of new transactions to the proposed candidate set.
        /// </summary>
        /// <param name="proposedTransactions"></param>
        public void PushNewProposedTransactions(TransactionContent[] proposedTransactions)
        {
            foreach (TransactionContent tc in proposedTransactions)
            {
                Hash tHash = tc.GetHash();
                if (!newCandidates.ContainsKey(tHash))
                {
                    newCandidates.Add(tHash, tc);
                }
            }
        }*/

        /// <summary>
        /// Inserts the TransactionContent to the proposed candidate set.
        /// </summary>
        /// <param name="proposedTransactions"></param>
        public void PushNewCandidate(TransactionContent transaction)
        {
            Hash tHash = transaction.GetHash();
            if (!newCandidates.ContainsKey(tHash))
            {
                newCandidates.Add(tHash, transaction);
            }
        }

        /// <summary>
        /// Check the list of proposed transactions for consistencey, and add it to candidate set.
        /// Do it when the source is trustworthy [think].
        /// </summary>
        /// <param name="Candidates"></param>
        /// <returns></returns>
        private Dictionary<Hash, TransactionContent> GetValidatedTransactions(Dictionary<Hash, TransactionContent> Candidates)
        {
            Dictionary<Hash, TransactionContent> finalGoodTransactions = new Dictionary<Hash, TransactionContent>();

            Dictionary<Hash, CreditDebitData> cdData = new Dictionary<Hash, CreditDebitData>();

            //HashSet<Hash> BlackList = new HashSet<Hash>();

            long TotalTransactions = 0;
            long FailedTransactions = 0;
            long GoodTransactions = 0;
            long BlackLists = 0;

            foreach (KeyValuePair<Hash, TransactionContent> _ts in Candidates)
            {
                TransactionContent ts = _ts.Value;

                Hash Source = ts.PublicKey_Source;

                if (!cdData.ContainsKey(Source))
                {
                    // This may cause exception if Source is not in ledgerData.
                    AccountInfo ai = (AccountInfo)LedgerTree[Source];  //(AccountInfo)account_tree.GetNodeData(Source);
                    cdData.Add(Source, new CreditDebitData(ai, ai.Money, 0, 0));
                }

                bool FAILED_TRANSACTION = false;

                foreach (TransactionSink sink in ts.Destinations)
                {
                    TotalTransactions++;

                    if (!cdData.ContainsKey(sink.PublicKey_Sink))
                    {
                        //AccountInfo ai = (AccountInfo)account_tree.GetNodeData(sink.PublicKey_Sink);
                        // This may cause exception if Source is not in ledgerData.
                        AccountInfo ai = (AccountInfo)LedgerTree[sink.PublicKey_Sink];
                        cdData.Add(sink.PublicKey_Sink, new CreditDebitData(ai, ai.Money, 0, 0));
                    }

                    long Deductible = sink.Amount;

                    long Remaining = cdData[Source].Money - cdData[Source].Debits - Deductible;

                    if ((Remaining > 0) && (!BlackList.Contains(sink.PublicKey_Sink)) && (!BlackList.Contains(Source)))
                    {
                        cdData[Source].Debits += Deductible;
                        cdData[sink.PublicKey_Sink].Credits += Deductible;
                        GoodTransactions++;

                        //DisplayUtils.Display("\t ::GOOD:: " + HexUtil.ToString(Source.Hex) + " : " + HexUtil.ToString(sink.PublicKey_Sink.Hex) + " : " + sink.Amount);
                    }
                    else
                    {
                        if (!BlackList.Contains(Source))
                        {
                            BlackLists++;
                            BlackList.Add(Source);
                        }

                        //DisplayUtils.Display("\t ::FAIL:: " + HexUtil.ToString(Source.Hex) + " : " + HexUtil.ToString(sink.PublicKey_Sink.Hex) + " : " + sink.Amount);
                        FAILED_TRANSACTION = true;
                        FailedTransactions++;
                    }
                }

                if (!FAILED_TRANSACTION)
                {
                    finalGoodTransactions.Add(_ts.Key, _ts.Value);
                }

            }

            return finalGoodTransactions;

        }

        /// <summary>
        /// ########################################################
        ///         TEST VERSION / NEEDS RIGOROUS REVIEW
        /// ########################################################
        /// </summary>
        /// <param name="Candidates"></param>
        /// <returns></returns>
        public bool ApplyTransactionToLedger(Dictionary<Hash, TransactionContent> Candidates)
        {
            Dictionary<Hash, CreditDebitData> cdData = new Dictionary<Hash, CreditDebitData>();

            HashSet<Hash> BlackList = new HashSet<Hash>();

            long TotalTransactions = 0;
            long FailedTransactions = 0;
            long GoodTransactions = 0;
            long BlackLists = 0;

            foreach (KeyValuePair<Hash, TransactionContent> _ts in Candidates)
            {
                TransactionContent ts = _ts.Value;

                Hash Source = ts.PublicKey_Source;

                if (!cdData.ContainsKey(Source))
                {
                    // This may cause exception if Source is not in ledgerData.
                    AccountInfo ai = (AccountInfo)LedgerTree[Source];  //(AccountInfo)account_tree.GetNodeData(Source);
                    cdData.Add(Source, new CreditDebitData(ai, ai.Money, 0, 0));
                }

                foreach (TransactionSink sink in ts.Destinations)
                {
                    TotalTransactions++;

                    if (!cdData.ContainsKey(sink.PublicKey_Sink))
                    {
                        //AccountInfo ai = (AccountInfo)account_tree.GetNodeData(sink.PublicKey_Sink);
                        // This may cause exception if Source is not in ledgerData.
                        AccountInfo ai = (AccountInfo)LedgerTree[sink.PublicKey_Sink];
                        cdData.Add(sink.PublicKey_Sink, new CreditDebitData(ai, ai.Money, 0, 0));
                    }

                    long Deductible = sink.Amount;

                    long Remaining = cdData[Source].Money - cdData[Source].Debits - Deductible;

                    if ((Remaining > 0) && (!BlackList.Contains(sink.PublicKey_Sink)) && (!BlackList.Contains(Source)))
                    {
                        cdData[Source].Debits += Deductible;
                        cdData[sink.PublicKey_Sink].Credits += Deductible;
                        GoodTransactions++;

                        //DisplayUtils.Display("\t ::GOOD:: " + HexUtil.ToString(Source.Hex) + " : " + HexUtil.ToString(sink.PublicKey_Sink.Hex) + " : " + sink.Amount);
                    }
                    else
                    {
                        if (!BlackList.Contains(Source))
                        {
                            BlackLists++;
                            BlackList.Add(Source);
                        }

                        //DisplayUtils.Display("\t ::FAIL:: " + HexUtil.ToString(Source.Hex) + " : " + HexUtil.ToString(sink.PublicKey_Sink.Hex) + " : " + sink.Amount);

                        FailedTransactions++;
                    }
                }
            }


            if ((GoodTransactions > 0) && (FailedTransactions == 0))
            {
                foreach (KeyValuePair<Hash, CreditDebitData> kvp in cdData)
                {
                    AccountInfo ai = new AccountInfo(kvp.Value.ai.AccountID, kvp.Value.Money + kvp.Value.Credits - kvp.Value.Debits);
                    LedgerTree.AddUpdate(ai); // GetNodeData(kvp.Key);
                }
            }
            else
            {
                return false;
            }

            return true;
        }



        public void RefreshValidTransactions()
        {
            newCandidates = GetValidatedTransactions(newCandidates);
        }

        public bool ApplyNewCandidates()
        {




            return false;
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
