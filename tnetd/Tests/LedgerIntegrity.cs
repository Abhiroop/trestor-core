using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TNetD.Consensus;
using TNetD.Json.JS_Structs;
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
        public delegate void LedgerIntegrityExentHandler(string Message);
        public event LedgerIntegrityExentHandler LedgerIntegrityExent;

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

            // var LCDs = new List<LedgerCloseData>();

            CancellationTokenSource cts = new CancellationTokenSource();

            await inputNode.nodeState.Persistent.CloseHistory.FetchAllLCLAsync((LedgerCloseData lcd) =>
            {
                //LCDs.Add(lcd);

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

                    if (tx_opers.AcceptedTransactions.Count != transactions.Count)
                    {
                        foreach (var tx in transactions)
                        {
                            if (!tx_opers.AcceptedTransactions.ContainsKey(tx.TransactionID))
                            {
                                // Transaction not accepted.
                                var d = new JS_TransactionReply(tx);

                                string txString = JsonConvert.SerializeObject(d, Common.JSON_SERIALIZER_SETTINGS);

                                DisplayUtils.Display("For LCS:SEQ" + lcd.SequenceNumber + ", Transaction Failed.\nTxInfo:\n" + txString);
                            }
                        }
                    }

                    LedgerCloseData l_lcd;

                    bool ok = destNode.nodeState.Persistent.CloseHistory.GetLastRowData(out l_lcd);

                    Hash dest_h = destNode.nodeState.Ledger.RootHash;
                    Hash curr_h = new Hash(lcd.LedgerHash);

                    LedgerIntegrityExent?.Invoke("Ledger " + l_lcd.SequenceNumber + "/" + lcd.SequenceNumber + " Closed:" + dest_h);

                    // DisplayUtils.Display("Ledger " + l_lcd.SequenceNumber + "/" + lcd.SequenceNumber + " Closed:" +
                    //                          dest_h, DisplayType.ImportantInfo);

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

        public async Task CompareAccounts()
        {
            await inputNode.LocalLedger.ReloadFromPersistentStore();

            var sw_removed = new StreamWriter("Compare_RemovedAccounts.log", false);
            var sw_mismatch = new StreamWriter("Compare_Mismatch.log", false);

            sw_removed.WriteLine("\nCOMPARE ACCOUNTS - Removed Log\n");
            sw_mismatch.WriteLine("\nCOMPARE ACCOUNTS - Mimatched Log\n");

            int removed_count = 0;
            int mismatch_count = 0;

            try
            {
                long a = 0, b = 0;
                inputNode.LocalLedger.LedgerTree.TraverseAllNodes(ref a, ref b, (accounts) =>
                {
                    var accountList = accounts;

                    foreach (var _sourceAccount in accountList)
                    {
                        var sourceAccount = (AccountInfo)_sourceAccount;

                        AccountInfo destAccount;

                        if (destNode.LocalLedger.TryFetch(sourceAccount.PublicKey, out destAccount))
                        {
                            if (sourceAccount.Money != destAccount.Money)
                            {
                                string json = JsonConvert.SerializeObject(sourceAccount,
                                    Common.JSON_SERIALIZER_SETTINGS);

                                string dst = "\n[Destination Value Mismatch] SourceValue: " + sourceAccount.Money +
                                             ", DestValue: " + destAccount.Money + ", Account:\n" + json + "\n";

                                sw_mismatch.WriteLine(dst);
                                mismatch_count++;

                                DisplayUtils.Display("LedgerIntegrity: " + dst, DisplayType.Debug);
                            }
                        }
                        else
                        {
                            // Account does not exist.

                            string json = JsonConvert.SerializeObject(sourceAccount,
                                Common.JSON_SERIALIZER_SETTINGS);

                            string dst = "\n[Account Removed] Account:\n" + json;

                            sw_removed.WriteLine("\n" + json);
                            removed_count++;

                            DisplayUtils.Display("LedgerIntegrity: " + dst, DisplayType.Debug);
                        }
                    }

                    return TreeResponseType.NothingDone;
                });

                sw_removed.WriteLine("\n\nRemoved Accounts: " + removed_count);
                sw_removed.WriteLine("\n\nMismatching Accounts: " + mismatch_count);
            }
            finally
            {
                sw_removed.Close();
                sw_mismatch.Close();
            }

        }
    }
}
