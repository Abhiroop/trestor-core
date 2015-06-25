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
    class DoubleSpendBlacklist
    {
        private ConcurrentDictionary<Hash, long> blacklist;
        private NodeState nodeState;



        /// <summary>
        /// number of seconds to keep expired transactions before clearing
        /// </summary>
        private readonly long KEEP_EXPIRED = 5 * 60;



        public DoubleSpendBlacklist(NodeState nodeState)
        {
            blacklist = new ConcurrentDictionary<Hash, long>();
            this.nodeState = nodeState;
        }



        public void Add(Hash account, long time)
        {
            blacklist.AddOrUpdate(account, time, (ok, ov) => ov > time ? ov : time);
        }



        public bool IsBlacklisted(Hash account)
        {
            return blacklist.ContainsKey(account);
        }



        public void ClearExpired()
        {
            long ntime = nodeState.NetworkTime;
            foreach (KeyValuePair<Hash, long> kvp in blacklist)
            {
                long timediff = (long)(DateTime.FromFileTimeUtc(ntime) - DateTime.FromFileTimeUtc(kvp.Value)).TotalSeconds;
                if (timediff > KEEP_EXPIRED + Common.TransactionStaleTimer_Minutes * 60)
                {
                    long value;
                    blacklist.TryRemove(kvp.Key, out value);
                }
            }

        }
    }
}
