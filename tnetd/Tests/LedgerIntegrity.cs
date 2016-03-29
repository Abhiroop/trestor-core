using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Consensus;
using TNetD.Ledgers;
using TNetD.Nodes;
using TNetD.SyncFramework.Packets;
using TNetD.Tree;

namespace TNetD.Tests
{
    // Tests all the transactions and applies them in sequence to the ledger 
    // in order to verify that all the close sequences are valid for all transactions.
    // The accounts and balances are therefore correct.
    class LedgerIntegrity
    {
        Node inputNode = default(Node);
        Node destNode = default(Node);

        public LedgerIntegrity(int inputNodeID, int destinationNodeID)
        {
            inputNode = new Node(inputNodeID);
            destNode = new Node(destinationNodeID);
        }

        public async Task ValidateLedger()
        {
            // We don't need to initialize the node.

            // Delete the data in the destination node.
            destNode.nodeState.Persistent.DeleteEverything();
            destNode.nodeState.Persistent.AccountStore.AddUpdateBatch(Constants.GetGenesisData());
            await destNode.nodeState.Ledger.ReloadFromPersistentStore();

            DisplayUtils.Display("Starting Validation for " + inputNode.nodeConfig.ID(), DisplayType.Debug);

            var LCDs = new List<LedgerCloseData>();

            await inputNode.nodeState.Persistent.CloseHistory.FetchAllLCLAsync((LedgerCloseData lcd) =>
            {
                LCDs.Add(lcd);

                List<TransactionContentSet> tcsS = new List<TransactionContentSet>();

                if (inputNode.nodeState.Persistent.TransactionStore.FetchBySequenceNumber(out tcsS,
                    lcd.SequenceNumber, 1) == PersistentStore.DBResponse.FetchSuccess)
                {
                    if (tcsS.Count == 1)
                    {
                        var tcs = tcsS[0].TxContent;
                        
                        TransactionValidator tv_dest = new TransactionValidator(destNode.nodeConfig, destNode.nodeState);

                        var tx_opers = tv_dest.ValidateTransactions(tcs);

                        //lcd.CloseTime = tcs[0].Timestamp;

                        tv_dest.ApplyTransactions(tx_opers, lcd);
                        
                        LedgerCloseData l_lcd;

                        bool ok = destNode.nodeState.Persistent.CloseHistory.GetLastRowData(out l_lcd);

                        Hash dest_h = destNode.nodeState.Ledger.RootHash;
                        Hash curr_h = new Hash(lcd.LedgerHash);

                        DisplayUtils.Display("Ledger Closed:" + dest_h, DisplayType.Debug);

                        /* if (dest_h == curr_h)
                         {
                             DisplayUtils.Display("MATCH:" + curr_h, DisplayType.Debug);
                         }
                         else
                         {
                             DisplayUtils.Display("MISMATCH:" + curr_h + "-" + dest_h, DisplayType.CodeAssertionFailed);
                         }*/
                    }
                    else
                    {
                        DisplayUtils.Display("ERR !", DisplayType.Debug);
                    }
                }

            });

            ListHashTree LedgerTree = new ListHashTree();

            // LedgerTree.AddUpdate()
            // AccountInfo

            foreach (var lcd in LCDs)
            {
                DisplayUtils.Display("LCD: " + lcd.SequenceNumber);
            }




        }



    }
}
