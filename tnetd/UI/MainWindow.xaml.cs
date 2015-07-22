/*
 *  @Author: Arpan Jati
 *  @Description: MainWindow / UI Stuff
 */

using Grapevine;
using Grapevine.Server;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TNetD.Address;
using TNetD.Ledgers;
using TNetD.Network.Networking;
using TNetD.Nodes;
using TNetD.PersistentStore;
using TNetD.Transactions;
using TNetD.Tree;
using TNetD.UI;

namespace TNetD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<TransactionContent> _tranxData = new ObservableCollection<TransactionContent>();

        List<Node> nodes = new List<Node>();

        List<Thread> runningNodes = new List<Thread>();

        

        public MainWindow()
        {
            Common.Initialize();

            InitializeComponent();

            DisplayUtils.DisplayText += DisplayUtils_DisplayText;

            lv_TX.ItemsSource = _tranxData;

            
            Title += " | " + Common.NetworkType.ToString();


            //System.Timers.Timer tmr_UI = new System.Timers.Timer(100);
            //tmr_UI.Elapsed += tmr_UI_Elapsed;
            //tmr_UI.Start();

        }

        private void START_NODES()
        {
            Thread backgroundLoad;
            backgroundLoad = new Thread(LoadNodes);
            backgroundLoad.Start();

            runningNodes.Add(backgroundLoad);
        }

        /*void tmr_UI_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                   // textBlock_StatusLabel.Text = "" + ledgerEvent.ToString() + " - " + Message;
                    //textBlock_StatusLog.Inlines.Add(new Run(Text + "\n") { Foreground = new SolidColorBrush(color) });
                }));
            }
            catch { }
        }*/

        void AddNode(int idx)
        {
            Node nd = new Node(idx);
            nd.LocalLedger.LedgerEvent += LocalLedger_LedgerEvent;
            nd.NodeStatusEvent += nd_NodeStatusEvent;
            nd.BeginBackgroundLoad();

            nodes.Add(nd);
        }

        void nd_NodeStatusEvent(string Status, int NodeID)
        {
            if (NodeID == 0)
            {
                try
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        textBlock_Status.Text = Status;
                    }));
                }
                catch { }
            }
        }

        void LocalLedger_LedgerEvent(Ledgers.Ledger.LedgerEventType ledgerEvent, string Message)
        {
            try
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    textBlock_StatusLabel.Text = "" + ledgerEvent.ToString() + " - " + Message;
                    //textBlock_StatusLog.Inlines.Add(new Run(Text + "\n") { Foreground = new SolidColorBrush(color) });
                }));
            }
            catch { }
            //throw new NotImplementedException();
        }

        void LoadNodes()
        {
            AddNode(0);
            //AddNode(1);
        }

        void DisplayUtils_DisplayText(string Text, Color color, DisplayType type)
        {
            if (type >= Constants.DebugLevel)
            {
                try
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (textBlock_Log.Text.Length > Common.UI_TextBox_Max_Length)
                        {
                            textBlock_Log.Text = "";
                        }

                        textBlock_Log.Inlines.Add(new Run(Text + "\n") { Foreground = new SolidColorBrush(color) });
                    }));
                }
                catch { }
            }
        }

        private void menuItem_Simulation_Start_Click(object sender, RoutedEventArgs e)
        {
            List<Hash> accounts = new List<Hash>();
            SortedDictionary<Hash, int> hh = new SortedDictionary<Hash, int>();

            byte[] N_H = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };

            ListHashTree lht = new ListHashTree();

            int ACCOUNTS = 1000;

            long taka = 0;

            DisplayUtils.Display("Adding ... ");

            for (int i = 0; i < ACCOUNTS; i++)
            {
                N_H[0] = (byte)(i);

                Common.rngCsp.GetBytes(N_H);

                accounts.Add(new Hash(N_H));

                long _taks = Common.random.Next(0, 1000000000);

                taka += _taks;

                AccountInfo ai = new AccountInfo(new Hash(N_H), _taks);
                lht.AddUpdate(ai);
            }

            //lht.TraverseNodes();

            //long received_takas = 0;

            // for (int i = 0; i < ACCOUNTS; i++)
            // {
            //     byte[] N_H = { (byte)(i * 3), 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };
            //     Hash h = new Hash(N_H);
            //     AccountInfo ai = (AccountInfo)lht[h];
            //     //DisplayUtils.Display("Fetch: " + HexUtil.ToString(ai.GetID().Hex) + "   ---  Money: " + ai.Money);
            //     received_takas += ai.Money;
            // }

            DisplayUtils.Display("Initial Money : " + taka);

            DisplayUtils.Display("\nTraversing ... ");

            long Tres = 0;
            
            long LeafDataCount = 0;
            long FoundNodes = 0;

            lht.TraverseAllNodes(ref LeafDataCount, ref FoundNodes, (X) =>
            {
                foreach (AccountInfo AI in X)
                {
                    Tres += AI.Money;
                }
                return TreeResponseType.NothingDone;
            });


            DisplayUtils.Display("Traversed Money : " + Tres);
            DisplayUtils.Display("Traversed Nodes : " + lht.TraversedNodes);
            DisplayUtils.Display("Traversed Elements : " + lht.TraversedElements);

            int cnt = 0;
            foreach (Hash h in accounts)
            {
                //N_H[5] = (byte)(i * 3);

                if (++cnt > accounts.Count / 2) break;

                lht.DeleteNode(h);
            }

            DisplayUtils.Display("\nTraversing After Delete ... ");
            // lht.TraverseNodes();

            DisplayUtils.Display("Traversed Money : " + lht.TotalMoney);
            DisplayUtils.Display("Traversed Nodes : " + lht.TraversedNodes);
            DisplayUtils.Display("Traversed Elements : " + lht.TraversedElements);
        }

        private void menuItem_File_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            STOP_NODES();          
        }

        private void STOP_NODES()
        {
            foreach (Node nd in nodes)
            {
                nd.StopNode();
            }

            foreach (Thread thr in runningNodes)
            {
                if (thr.IsAlive)
                    thr.Abort();
            }
        }

        /// ///////

        private void menuItem_Server_Start_Click(object sender, RoutedEventArgs e)
        {
            START_NODES();

            /*SingleTransactionFactory stf = new SingleTransactionFactory(nodes[0].PublicKey, nodes[1].PublicKey, Constants.random.Next(100, 1000), Constants.random.Next(10, 150000));

            byte[] tranxData = stf.GetTransactionData();
            byte[] signature = nodes[0].nodeConfig.SignDataWithPrivateKey(tranxData);

            TransactionContent transactionContent;

            TransactionProcessingResult TransOk = stf.Create(new Hash(signature), out transactionContent);

            if (TransOk == TransactionProcessingResult.Accepted)
            {
                DisplayUtils.Display("Transaction Valid: ");

                DBResponse dBResponse = nodes[0].TransactionStore.AddUpdate(transactionContent);

                DisplayUtils.Display("dBResponse: " + dBResponse);

                _tranxData.Add(transactionContent);
            }*/
        }

        private void menuItem_Server_Stop_Click(object sender, RoutedEventArgs e)
        {
            /*NodeConfig nc = new NodeConfig(0);
            NodeConfig nc1 = new NodeConfig(1);

            IPersistentTransactionStore transactionStore = new SQLiteTransactionStore(nc);*/

            STOP_NODES();
        }

        private void lv_TX_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lv_TX != null)
            {
                if (lv_TX.SelectedItem != null)
                {
                    TransactionContent var = (TransactionContent)lv_TX.SelectedItem;
                    tb_Tx_txid.Text = var.TransactionID.ToString();
                }
            }
        }

        private void menu_CreateTransaction_Click(object sender, RoutedEventArgs e)
        {
            CreateTransaction ct = new CreateTransaction();
            ct.Show();
        }

        private void menu_CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            CreateAccount ca = new CreateAccount();
            ca.Show();
        }

        private void menu_Reset_Ledger_To_Genesis_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("Do you really want to reset the current state. All state information will be lost.",
                "Ledger State Reset !!!", MessageBoxButton.YesNo))
            {
                List<AccountInfo> aiData = new List<AccountInfo>();

                string[] Accs = Common.NetworkType == NetworkType.MainNet ? GenesisRawData.MainNet : GenesisRawData.TestNet;

                foreach (string acc in Accs)
                {
                    AccountIdentifier AI = new AccountIdentifier();
                    AI.Deserialize(Convert.FromBase64String(acc));

                    AccountInfo ai = new AccountInfo(new Hash(AI.PublicKey), Constants.FIN_TRE_PER_GENESIS_ACCOUNT);

                    ai.NetworkType = AI.AddressData.NetworkType;
                    ai.AccountType = AI.AddressData.AccountType;

                    ai.AccountState = AccountState.Normal;
                    ai.LastTransactionTime = 0;
                    ai.Name = AI.Name;

                    aiData.Add(ai);
                }

                // Write to nodes
                foreach (Node n in nodes)
                {
                    var resp = n.nodeState.PersistentAccountStore.DeleteEverything();

                    n.nodeState.PersistentAccountStore.AddUpdateBatch(aiData);
                }

                MessageBox.Show("ACCOUNTS RESET. It will take some time to synchronise with the network to resume normal operation." +
                    "\nApplication restart needed for proper operation.");
            }
        }

        async private void menu_Server_RecalculateTotalBalances_Click(object sender, RoutedEventArgs e)
        {
            long Value_Persistent = await nodes[0].CalculateTotalMoneyInPersistentStoreAsync();
            long Value_Tree = await nodes[0].CalculateTotalMoneyFromLedgerTreeAsync();

            MessageBox.Show("Total Balances\n\nPersistent: " + Value_Persistent +
                "\n\nTree: " + Value_Tree);
        }

        private void menu_BannedNames_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog().Value)
            {
                StreamReader sr = new StreamReader(ofd.FileName);

                SQLiteBannedNames sbn = new SQLiteBannedNames(nodes[0].nodeConfig);

                List<string> BN = new List<string>();

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();

                    line = line.ToLowerInvariant();

                    string[] parts = line.Split(',');

                    if (parts.Length == 2)
                    {
                        string Name = parts[1];

                        string[] nameparts = Name.Split('.');

                        if (nameparts.Length > 0)
                        {
                            string _part = nameparts[0];
                            BN.Add(_part);
                        }
                    }
                }

                sbn.AddUpdateBatch(BN);
                sr.Close();
                MessageBox.Show("DONE.");

            }
        }

        private void menuItem_Main2_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            MainWindow2 m2 = new MainWindow2();
            m2.ShowDialog();
            Show();
        }

        private void menuItem_Main3_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            DebugWindow3 m3 = new DebugWindow3();
            m3.ShowDialog();
            Show();
        }

        private void menuItem_Main4_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            DebugWindow4 m4 = new DebugWindow4();
            m4.ShowDialog();
            Show();
        }

    }
}
