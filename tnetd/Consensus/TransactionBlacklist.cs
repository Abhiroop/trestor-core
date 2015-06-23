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
    class TransactionBlacklist
    {
        private ConcurrentDictionary<Hash, long> blacklist;
        private NodeState nodeState;



        /// <summary>
        /// number of seconds to keep expired transactions before clearing
        /// </summary>
        private readonly long KEEP_EXPIRED = 5 * 60;



        public TransactionBlacklist(NodeState nodeState)
        {
            blacklist = new ConcurrentDictionary<Hash, long>();
            this.nodeState = nodeState;
        }



        public void Add(TransactionContent[] transactions)
        {
            foreach (TransactionContent transaction in transactions)
            {
                foreach (TransactionEntity account in transaction.Sources)
                {
                    blacklist.AddOrUpdate(new Hash(account.PublicKey), transaction.Timestamp, (ok, ov) => ov > transaction.Timestamp ? ov : transaction.Timestamp);
                }
            }
        }



        public bool IsBlacklisted(Hash account)
        {
            return blacklist.ContainsKey(account);
        }



        private void clearExpired()
        {
            foreach (KeyValuePair<Hash, long> kvp in blacklist)
            {
                long timediff = (long)(DateTime.FromFileTimeUtc(nodeState.NetworkTime) - DateTime.FromFileTimeUtc(kvp.Value)).TotalSeconds;
                if (timediff > KEEP_EXPIRED + Common.TransactionStaleTimer_Minutes * 60)
                {
                    long value;
                    blacklist.TryRemove(kvp.Key, out value);
                }
            }

        }
    }
}
