
// @Author : Arpan Jati
// @Date: 8th Feb 2015 

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Types;

namespace TNetD.Transactions
{
    /// <summary> 
    /// Manages the status results for current transactions; helps to reply queries about transaction progress and result.
    /// </summary>
    public class TransactionStateManager
    {
        // For status:
        public ConcurrentDictionary<Hash, TransactionState> TransactionProcessingMap = new ConcurrentDictionary<Hash, TransactionState>();

        public void Set(Hash TransactionID, TransactionProcessingResult transactionProcessingResult)
        {
            if (TransactionProcessingMap.ContainsKey(TransactionID))
            {
                TransactionProcessingMap[TransactionID].ProcessingResult = transactionProcessingResult;
            }
            else
            {
                TransactionProcessingMap.TryAdd(TransactionID, new TransactionState(transactionProcessingResult));
            }
        }

        public void Set(Hash TransactionID, TransactionStatusType transactionStatusType)
        {
            if (TransactionProcessingMap.ContainsKey(TransactionID))
            {
                TransactionProcessingMap[TransactionID].StatusType = transactionStatusType;
            }
            else
            {
                TransactionProcessingMap.TryAdd(TransactionID, new TransactionState(transactionStatusType));
            }
        }

        public bool Fetch(out TransactionState transactionState, Hash TransactionID)
        {
            if (TransactionProcessingMap.ContainsKey(TransactionID))
            {
                transactionState = TransactionProcessingMap[TransactionID];
                return true;
            }

            transactionState = new TransactionState();
            return false;
        }
        
        /// <summary>
        /// Clear the old transactions from the set.
        /// </summary>
        public void ProcessAndClear()
        {
            DateTime NOW = DateTime.UtcNow;

            Hash[] tsmKeys = TransactionProcessingMap.Keys.ToArray();

            foreach (Hash key in tsmKeys)
            {
                TransactionState transactionState;
                if (TransactionProcessingMap.TryGetValue(key, out transactionState))
                {
                    TimeSpan span = (NOW - transactionState.FetchTimeUTC);
                    if (span.TotalSeconds > Common.TRANSACTION_STATUS_PERSIST_SECONDS)
                    {
                        TransactionProcessingMap.TryRemove(key, out transactionState);
                    }
                }
            }
        }


    }
}
