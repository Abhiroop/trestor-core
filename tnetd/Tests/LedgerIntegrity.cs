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
        Node node = default(Node);

        public LedgerIntegrity(int NodeID)
        {
            node = new Node(NodeID);

        }

        public async Task ValidateLedger()
        {
            // We don't need to initialize the node.

            DisplayUtils.Display("Starting Validation for " + node.nodeConfig.ID(), DisplayType.Debug);

            var LCDs = new List<LedgerCloseData>();

            await node.nodeState.Persistent.CloseHistory.FetchAllLCLAsync((LedgerCloseData lcd) =>
            {
                LCDs.Add(lcd);

                List<TransactionContentSet> tcsS = new List<TransactionContentSet>();

                if (node.nodeState.Persistent.TransactionStore.FetchBySequenceNumber(out tcsS,
                    lcd.SequenceNumber, 1) == PersistentStore.DBResponse.FetchSuccess)
                {
                    if (tcsS.Count == 1)
                    {
                        TransactionValidator tv = new TransactionValidator(node.nodeConfig, node.nodeState);



                        var tcs = tcsS[0].TxContent;
                        
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
