using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Consensus
{
    class VoteType
    {
        public Hash TransactionID;
        // if vote is +ve the Vote = true
        // for -ve vote Vote = false
        public bool Vote;
        VoteType()
        {
            Vote = false;
        }
        VoteType(Hash transactionID, bool Vote)
        {
            this.TransactionID = transactionID;
            this.Vote = Vote;
        }

    }
}
