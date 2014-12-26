using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Transactions;

namespace TNetD
{
    class CreditDebitData
    {
        public AccountInfo ai;
        public long Money;
        public long Credits;
        public long Debits;
        public CreditDebitData(AccountInfo ai, long Money, long Credits, long Debits)
        {
            this.ai = ai;
            this.Money = Money;
            this.Credits = Credits;
            this.Debits = Debits;
        }
    }

    /*struct TransactionSink
    {
        public Hash PublicKey_Sink;
        public long Amount;
        public TransactionSink(Hash PublicKey_Sink, long Amount)
        {
            this.PublicKey_Sink = PublicKey_Sink;
            this.Amount = Amount;
        }
    }*/

    

   


}
