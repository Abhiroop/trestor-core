using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TNetD.Consensus;
using TNetD.Ledgers;
using TNetD.Nodes;
using TNetD.SyncFramework.Packets;
using TNetD.Transactions;
using TNetD.Tree;

namespace TNetD.Tests
{
    // Tests all the transactions and applies them in sequence to the ledger 
    // in order to verify that all the close sequences are valid for all transactions.
    // The accounts and balances are therefore correct.
    class LedgerIntegrity : IDisposable
    {
        Node inputNode = default(Node);
        Node destNode = default(Node);
        Node exportNode = default(Node);
        Node tempNode = default(Node);

        public LedgerIntegrity(int inputNodeID, int destinationNodeID, int exportNodeID)
        {
            tempNode = new Node(inputNodeID);

            inputNode = new Node(inputNodeID + 1, true);

            tempNode.nodeState.Persistent.ExportToNodeSQLite(inputNode);

            destNode = new Node(destinationNodeID, true);
            exportNode = new Node(exportNodeID);
        }

        public void Dispose()
        {
            tempNode.StopNode();
            inputNode.StopNode();
            destNode.StopNode();
            exportNode.StopNode();
        }

        public async Task ValidateLedger()
        {
            // We don't need to initialize the node.

            exportNode.nodeState.Persistent.DeleteEverything();

            // Delete the data in the destination node.
            destNode.nodeState.Persistent.DeleteEverything();
            destNode.nodeState.Persistent.AccountStore.AddUpdateBatch(Constants.GetGenesisData());
            await destNode.nodeState.Ledger.ReloadFromPersistentStore();

            DisplayUtils.Display("Starting Validation for " + inputNode.nodeConfig.ID(), DisplayType.Debug);

            var LCDs = new List<LedgerCloseData>();

            CancellationTokenSource cts = new CancellationTokenSource();

            await inputNode.nodeState.Persistent.CloseHistory.FetchAllLCLAsync((LedgerCloseData lcd) =>
            {
                LCDs.Add(lcd);

                List<TransactionContent> transactions;

                if (inputNode.nodeState.Persistent.TransactionStore.FetchBySequenceNumber(out transactions,
                    lcd.SequenceNumber) == PersistentStore.DBResponse.FetchSuccess)
                {
                    TransactionValidator tv_dest = new TransactionValidator(destNode.nodeConfig, destNode.nodeState);

                    var tx_opers = tv_dest.ValidateTransactions(transactions);

                    //lcd.CloseTime = tcs[0].Timestamp;
                    if (tx_opers.AcceptedTransactions.Any())
                    {
                        tv_dest.ApplyTransactions(tx_opers, lcd.CloseTime);
                    }

                    LedgerCloseData l_lcd;

                    bool ok = destNode.nodeState.Persistent.CloseHistory.GetLastRowData(out l_lcd);

                    Hash dest_h = destNode.nodeState.Ledger.RootHash;
                    Hash curr_h = new Hash(lcd.LedgerHash);

                    DisplayUtils.Display("Ledger " + l_lcd.SequenceNumber + "/" + lcd.SequenceNumber + " Closed:" +
                                                dest_h, DisplayType.ImportantInfo);

                    /* if (dest_h == curr_h)
                     {
                         DisplayUtils.Display("MATCH:" + curr_h, DisplayType.Debug);
                     }
                     else
                     {
                         DisplayUtils.Display("MISMATCH:" + curr_h + "-" + dest_h, DisplayType.CodeAssertionFailed);
                     }*/

                    if (Constants.ApplicationRunning == false) cts.Cancel();

                }

            }, cts.Token);


            /*foreach (var lcd in LCDs)
            {
                DisplayUtils.Display("LCD: " + lcd.SequenceNumber);
            }*/

            destNode.nodeState.Persistent.ExportToNodeSQLite(exportNode);
        }
    }
}
