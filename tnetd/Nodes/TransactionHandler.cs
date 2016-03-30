using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TNetD.Consensus;
using TNetD.Transactions;

namespace TNetD.Nodes
{
    class TransactionHandler
    {
        private object transactionSyncLock = new object();

        NodeConfig nodeConfig;
        NodeState nodeState;

        public TransactionHandler(NodeConfig nodeConfig, NodeState nodeState)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;
        }

        public void ProcessPendingTransactions()
        {
            lock (transactionSyncLock)
            {
                try
                {
                    if ((nodeState.IncomingTransactionMap.IncomingTransactions.Count > 0) &&
                        (Common.NODE_OPERATION_TYPE == NodeOperationType.Centralized))
                    {
                        var currentTransactions = new List<TransactionContent>();

                        lock (nodeState.IncomingTransactionMap.transactionLock)
                        {
                            foreach (var transaction in nodeState.IncomingTransactionMap.IncomingTransactions)
                            {
                                currentTransactions.Add(transaction.Value);

                                Interlocked.Increment(ref nodeState.NodeInfo.NodeDetails.TransactionsVerified);
                            }

                            nodeState.IncomingTransactionMap.IncomingTransactions.Clear();
                            nodeState.IncomingTransactionMap.IncomingPropagations_ALL.Clear();
                        }

                        var transactionValidator = new TransactionValidator(nodeConfig, nodeState);

                        var transactionHandlingData = transactionValidator.ValidateTransactions(currentTransactions);

                        if (transactionHandlingData.AcceptedTransactions.Any())
                        {
                            transactionValidator.ApplyTransactions(transactionHandlingData);
                        }
                    }

                }
                catch (Exception ex)
                {
                    DisplayUtils.Display("Timer Event : ProcessPendingTransactions()", ex);
                }
            }
        }
    }
}
