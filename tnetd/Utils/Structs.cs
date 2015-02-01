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

    struct CandidateStatus
    {
        public bool Vote;
        public bool Forwarded;
        public CandidateStatus(bool Vote, bool Forwarded)
        {
            this.Vote = Vote;
            this.Forwarded = Forwarded;
        }
    }

    struct TransactionContentPack
    {
        public Hash Source;
        public TransactionContent Transacation;
        public TransactionContentPack(Hash Source, TransactionContent Transacation)
        {
            this.Source = Source;
            this.Transacation = Transacation;
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


    struct TimeStruct
    {
        public long sendTime;
        public long receivedTime;
        public long TimeFromValidator;
        public Hash token;
        public long timeDifference; //my time - other time
    };


    class TransactionContentData
    {
        TransactionContent TransactionContent;
        HashSet<Hash> ForwardersPK;

        public TransactionContentData()
        {
            TransactionContent = new TransactionContent();
            ForwardersPK = new HashSet<Hash>();
        }
    }
    
    
}
