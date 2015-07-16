using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Transactions;
using TNetD.Nodes;

namespace TNetD.Consensus
{
    class TransactionChecker
    {

        NodeState nodeState;
        DoubleSpendBlacklist blacklist;





        public TransactionChecker(NodeState nodeState)
        {
            this.nodeState = nodeState;
            blacklist = new DoubleSpendBlacklist(nodeState);
        }





        public Ballot CreateBallot(ConcurrentDictionary<Hash, TransactionContent> CurrentTransactions)
        {
            blacklist.ClearExpired();
            SortedSet<Hash> mergedTransactions = new SortedSet<Hash>();
            Dictionary<Hash, long> temporaryBalances = new Dictionary<Hash, long>();
            foreach (KeyValuePair<Hash, TransactionContent> transaction in CurrentTransactions)
            {
                List<Hash> badaccounts;
                if (Spendable(transaction.Value, temporaryBalances, out badaccounts))
                {
                    //update temporary balances
                    foreach (TransactionEntity te in CurrentTransactions[transaction.Key].Sources)
                    {
                        Hash account = new Hash(te.PublicKey);
                        if (temporaryBalances.ContainsKey(account))
                        {
                            temporaryBalances[account] -= te.Value;
                        }
                        else
                        {
                            AccountInfo accountInfo;
                            if (nodeState.Ledger.TryFetch(account, out accountInfo))
                            {
                                temporaryBalances[account] = accountInfo.Money - te.Value;
                            }
                            else
                            {
                                // this case should not happen
                                throw new Exception("account disappeared after check");
                            }
                        }
                    }
                    //add transaction to ballot proposal
                    mergedTransactions.Add(transaction.Key);
                }
                else
                {
                    //handle bad accounts
                    foreach (Hash badaccount in badaccounts)
                    {
                        //remove all bad transactions from ballot proposal
                        long time = 0;
                        foreach (Hash t in mergedTransactions)
                        {
                            foreach (TransactionEntity source in CurrentTransactions[t].Sources)
                            {
                                if (source.PublicKey == badaccount.Hex)
                                {
                                    mergedTransactions.Remove(t);
                                }
                            }
                        }
                        //blacklist bad account
                        blacklist.Add(badaccount, time);
                    }
                }
            }
            Ballot ballot = new Ballot();
            ballot.TransactionIds = mergedTransactions;
            return ballot;
        }




        /// <summary>
        /// returns true, if transaction is spendable under the current ledger
        /// in combination with transactions from mergedTransactions
        /// adds all accounts with too little balance to badaccounts for blacklisting
        /// removes all double-spending accounts from mergedTransactions
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="temporaryBalances"></param>
        /// <param name="badaccounts"></param>
        /// <returns></returns>
        public bool Spendable(TransactionContent transaction, Dictionary<Hash, long> temporaryBalances, out List<Hash> badaccounts)
        {
            badaccounts = new List<Hash>();
            bool spendable = true;
            foreach (TransactionEntity sender in transaction.Sources)
            {
                Hash account = new Hash(sender.PublicKey);

                AccountInfo accountInfo;
                bool ok = nodeState.Ledger.TryFetch(account, out accountInfo);

                // account does not exist
                if (!ok)
                {
                    spendable = false;
                    break;
                }
                // account already used in this voting round
                if (temporaryBalances.ContainsKey(account))
                {
                    if (sender.Value > temporaryBalances[account])
                    {
                        badaccounts.Add(account);
                        spendable = false;
                    }
                }
                // account not used before
                else
                {
                    if (sender.Value > accountInfo.Money)
                    {
                        badaccounts.Add(account);
                        spendable = false;
                    }
                }
            }
            return spendable;
        }
    }




}
