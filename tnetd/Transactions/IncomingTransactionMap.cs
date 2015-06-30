
// @Author: Arpan Jati
// @Date: 16th January 2015
//  21st Jan 2015 : IncomingPropagations / Single node TEST_MODE.
//  8th Feb 2015 : TransactionStateManager

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TNetD.Nodes;
using TNetD.Types;

namespace TNetD.Transactions
{

    class IncomingTransactionMap
    {
        NodeState nodeState;
        NodeConfig nodeConfig;
        TransactionStateManager transactionStateManager;

        bool TimerEventProcessed = true;

        // Makeshift :P
        public Object transactionLock = new Object();

        //[Transaction ID] -> [[Transaction Content] -> [vector of sender address]]
        ConcurrentDictionary<Hash, TransactionContentData> TransactionMap = new ConcurrentDictionary<Hash, TransactionContentData>();

        /// <summary>
        /// Incoming transactions from multiple sources to be processes.
        /// //[Transaction ID] -> Transaction Data
        /// TODO: MAKE PRIVATE
        /// </summary>
        public ConcurrentDictionary<Hash, TransactionContent> IncomingTransactions { get; private set; }
        
        /// <summary>
        /// Incoming propagations from Wallets using /propagateraw and /propagate.
        /// [Transaction ID] -> Transaction Data
        /// </summary>
        ConcurrentDictionary<Hash, TransactionContent> IncomingPropagations = new ConcurrentDictionary<Hash, TransactionContent>();

        public ConcurrentDictionary<Hash, TransactionContent> IncomingPropagations_ALL = new ConcurrentDictionary<Hash, TransactionContent>();
        
        public IncomingTransactionMap(NodeState nodeState, NodeConfig nodeConfig, TransactionStateManager transactionStateManager)
        {
            this.nodeState = nodeState;
            this.nodeConfig = nodeConfig;
            this.transactionStateManager = transactionStateManager;

            IncomingTransactions = new ConcurrentDictionary<Hash, TransactionContent>();

            System.Timers.Timer Tmr = new System.Timers.Timer();
            Tmr.Elapsed += Tmr_Elapsed;
            Tmr.Enabled = true;
            Tmr.Interval = nodeConfig.UpdateFrequencyPacketProcessMS;
            Tmr.Start();
        }
        


        void Tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(TimerEventProcessed) // Lock to prevent multiple invocations 
            {
                TimerEventProcessed = false;

                // // // // // // // // //

                try
                {

                    lock (transactionLock)
                    {

                        // Add the propagations to the transactions list.
                        foreach (KeyValuePair<Hash, TransactionContent> kvp in IncomingPropagations)
                        {
                            if (!IncomingTransactions.ContainsKey(kvp.Key))
                            {
                                if (!IncomingTransactions.TryAdd(kvp.Value.TransactionID, kvp.Value))
                                {
                                    DisplayUtils.Display("IncomingTransactionMap : Tmr_Elapsed : IncomingTransactions.TryAdd Failed", DisplayType.Warning);
                                }
                                else
                                {
                                    transactionStateManager.Set(kvp.Value.TransactionID, TransactionStatusType.InProcessingQueue);
                                    Interlocked.Increment(ref nodeState.NodeInfo.NodeDetails.TransactionsAccepted);
                                }
                            }
                        }

                        IncomingPropagations.Clear();

                    }

                    // TODO: Forward to connected peers.
                    // More processing.

                }
                catch
                {
                    DisplayUtils.Display("Timer Event : Exception : IncomingTransactionMap", DisplayType.Warning);
                }

                // // // // // // // // //
            }
            else
            {
                DisplayUtils.Display("Timer Expired : IncomingTransactionMap" , DisplayType.Warning);
            }

            TimerEventProcessed = true;            
        }

        public Tuple<TransactionContent, bool> GetPropagationContent(Hash transactionID)
        {
            if (IncomingPropagations.ContainsKey(transactionID))
            {
                TransactionContent transactionContent;
                bool okay = IncomingPropagations.TryGetValue(transactionID, out transactionContent);

                if (!okay)
                    DisplayUtils.Display("Propagation Fetch Fail : GetPropagationContent", DisplayType.Warning);

                return new Tuple<TransactionContent, bool>(transactionContent, okay);
            }

            return new Tuple<TransactionContent, bool>(null, false);
        }

        /// <summary>
        /// Given a transactionID returns current associated TransactionContent, else return false.
        /// </summary>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        public Tuple<TransactionContent, bool> GetTransactionContent(Hash transactionID)
        {
            if (IncomingTransactions.ContainsKey(transactionID))
            {
                TransactionContent transactionContent;
                bool okay = IncomingTransactions.TryGetValue(transactionID, out transactionContent);

                if(!okay)
                    DisplayUtils.Display("Incoming Transaction Fetch Fail : GetTransactionContent", DisplayType.Warning);

                return new Tuple<TransactionContent, bool>(transactionContent, okay);
            }

            return new Tuple<TransactionContent, bool>(null, false);
        }

        /// <summary>
        /// Given a transactionID returns current associated TransactionContentData, else return false.
        /// </summary>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        Tuple<TransactionContentData, bool> GetTransactionContentData(Hash transactionID)
        {
            if (TransactionMap.ContainsKey(transactionID))
            {
                return new Tuple<TransactionContentData, bool>(TransactionMap[transactionID], true);
            }

            return new Tuple<TransactionContentData, bool>(null, false);
        }

        /// <summary>
        /// Insert a new transaction to the incoming transaction processing queue.
        /// This queue is processed from time to time.
        /// </summary>
        /// <param name="transactionContent"></param>
        /// <param name="senderPublicKey"></param>
        public void InsertNewTransaction(TransactionContent transactionContent, Hash senderPublicKey)
        {
            TransactionProcessingResult rslt = transactionContent.VerifySignature();

            if (rslt == TransactionProcessingResult.Accepted)
            {
                // Insert if the transaction does not already exist.
                if (!IncomingTransactions.ContainsKey(transactionContent.TransactionID))
                {
                    IncomingTransactions.TryAdd(transactionContent.TransactionID, transactionContent);
                }
            }
            else
            {
                // Add the user to the blacklist if signature is invalid or integrity checks fails.
                nodeState.GlobalBlacklistedUsers.Add(senderPublicKey);
            }
        }

        /// <summary>
        /// Handles Propagation request
        /// </summary>
        /// <param name="transactionContent"></param>
        /// <returns></returns>
        public TransactionProcessingResult HandlePropagationRequest(TransactionContent transactionContent)
        {
            Interlocked.Increment(ref nodeState.NodeInfo.NodeDetails.TransactionsProcessed);

            TransactionProcessingResult rslt = transactionContent.VerifySignature();

            transactionStateManager.Set(transactionContent.TransactionID, rslt);
            transactionStateManager.Set(transactionContent.TransactionID, TransactionStatusType.Proposed);

            if (!IncomingPropagations_ALL.ContainsKey(transactionContent.TransactionID))
            {
                IncomingPropagations_ALL.TryAdd(transactionContent.TransactionID, transactionContent);
            }

            if (rslt == TransactionProcessingResult.Accepted)
            {
                lock (transactionLock)
                {
                    // Insert if the transaction does not already exist.
                    if (!IncomingPropagations.ContainsKey(transactionContent.TransactionID))
                    {
                        if (IncomingPropagations.TryAdd(transactionContent.TransactionID, transactionContent))
                        {
                            transactionStateManager.Set(transactionContent.TransactionID, TransactionStatusType.InPreProcessing);
                        }
                        else
                        {
                            DisplayUtils.Display("IncomingTransactionMap : HandlePropagationRequest : IncomingPropagations.TryAdd Failed", DisplayType.Warning);
                        }
                    }
                }

            }

            return rslt;
        }

        void RemoveTransactionsFromTransactionMap(List<Hash> transactionIDs)
        {
            foreach (Hash transactionID in transactionIDs)
            {
                if (TransactionMap.ContainsKey(transactionID))
                {
                    TransactionContentData tcd;
                    TransactionMap.TryRemove(transactionID, out tcd);
                }
            }
        }

        Hash[] FetchAllTransactionIDs()
        {
            return (Hash[])TransactionMap.Keys;
        }

        bool HaveTransactionInfo(Hash transactionID)
        {
            return (TransactionMap.ContainsKey(transactionID));
        }
    }
}

