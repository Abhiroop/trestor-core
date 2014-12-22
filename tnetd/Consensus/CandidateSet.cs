using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Transactions;

namespace TNetD.Consensus
{

    class CandidateSet
    {
        List<TransactionContent> _transactions;

        public TransactionContent this[int index]
        {
            get
            {
                return _transactions[index];
            }
        }

        public int Count
        {
            get { return _transactions.Count; }
        }

        public List<TransactionContent> Transactions
        {
            get { return _transactions; }
        }

        public CandidateSet()
        {
            _transactions = new List<TransactionContent>();
        }

        public void AddTransaction(TransactionContent transaction)
        {
            _transactions.Add(transaction);
        }

        ///// <summary>
        ///// *******  TEST FUNCTION ONLY  ******
        ///// </summary>
        ///// <param name="accounts"></param>
        //public void GenerateTransactions(List<AccountInfo> accounts)
        //{
        //    _transactions.Clear();

        //    int totalAccounts = accounts.Count;

        //    Random rnd = new Random();

        //    foreach (AccountInfo account in accounts)
        //    {
        //        int nodes = rnd.Next(1, totalAccounts / 5);

        //        long amountToSpend = (long)(account.Money * 1.5F);

        //        long spendingAmt = (amountToSpend / (long)nodes);

        //        List<TransactionSink> tsks = new List<TransactionSink>();

        //        for (int dest = 0; dest < nodes; dest++)
        //        {
        //            int _id = rnd.Next(0, totalAccounts);
        //            AccountInfo sinkAccount = accounts[_id];

        //            if (sinkAccount.AccountID == account.AccountID) continue;

        //            long amountToSpendPerSink = (long)(spendingAmt * rnd.NextDouble()); //(long)rnd.Next(Math.Max((int)spendingAmt/2, 1), Math.Max((int)spendingAmt, 1));

        //            TransactionSink tsk = new TransactionSink(sinkAccount.AccountID, amountToSpendPerSink);
        //            tsks.Add(tsk);
        //        }

        //        TransactionContent tco = new TransactionContent(account.AccountID, 0, tsks.ToArray(), new Hash[0]);
        //        _transactions.Add(tco);
        //    }

        //}

    }

}
