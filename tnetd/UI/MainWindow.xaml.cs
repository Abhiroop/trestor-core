/*
 *  @Author: Arpan Jati
 *  @Description: MainWindow / UI Stuff
 */

using Grapevine;
using Grapevine.Server;
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
        GlobalConfiguration globalConfiguration;

        Thread background_Load;

        public MainWindow()
        {
            Common.Initialize();

            InitializeComponent();

            DisplayUtils.DisplayText += DisplayUtils_DisplayText;
            globalConfiguration = new GlobalConfiguration();

            lv_TX.ItemsSource = _tranxData;

            background_Load = new Thread(LoadNodes);

            background_Load.Start();
        }

        void AddNode(int idx)
        {
            Node nd = new Node(idx, globalConfiguration);
            nd.LocalLedger.LedgerEvent += LocalLedger_LedgerEvent;
            nd.BeginBackgroundLoad();

            nodes.Add(nd);
        }

        void LocalLedger_LedgerEvent(Ledgers.Ledger.LedgerEventType ledgerEvent, string Message)
        {
            try
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    textBlock_Status.Text = "" + ledgerEvent.ToString() + " - " + Message;
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
            try
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    if (textBlock_StatusLog.Text.Length > Common.UI_TextBox_Max_Length)
                    {
                        textBlock_StatusLog.Text = "";
                    }

                    textBlock_StatusLog.Inlines.Add(new Run(Text + "\n") { Foreground = new SolidColorBrush(color) });
                }));
            }
            catch { }
        }

        private void menuItem_Simulation_Start_Click(object sender, RoutedEventArgs e)
        {
            List<Hash> accounts = new List<Hash>();
            SortedDictionary<Hash, int> hh = new SortedDictionary<Hash, int>();

            byte[] N_H = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };

            ListHashTree lht = new ListHashTree();

            int ACCOUNTS = 2000;

            long taka = 0;

            DisplayUtils.Display("Adding ... ");

            for (int i = 0; i < ACCOUNTS; i++)
            {
                N_H[5] = (byte)(i * 3);

                Common.rngCsp.GetBytes(N_H);

                accounts.Add(new Hash(N_H));

                long _taks = Common.random.Next(0, 1000000000);

                taka += _taks;

                AccountInfo ai = new AccountInfo(new Hash(N_H), _taks);
                lht.AddUpdate(ai);
            }

            ////lht.TraverseNodes();

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
            lht.TraverseNodes();

            DisplayUtils.Display("Traversed Money : " + lht.TotalMoney);
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
            lht.TraverseNodes();

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
            foreach (Node nd in nodes)
            {
                nd.StopNode();
            }

            if (background_Load != null)
            {
                if (background_Load.IsAlive)
                {
                    background_Load.Abort();
                }
            }
        }

        /// ///////

        private void menuItem_Server_Start_Click(object sender, RoutedEventArgs e)
        {
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
            GlobalConfiguration gc = new GlobalConfiguration();
            NodeConfig nc = new NodeConfig(0, gc);
            NodeConfig nc1 = new NodeConfig(1, gc);

            IPersistentTransactionStore transactionStore = new SQLiteTransactionStore(nc);
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

            /*StringBuilder sb = new StringBuilder();
            AddressFactory af = new AddressFactory();

            for (int j = 0; j < 256; j++)
            {
                for (int i = 0; i < 256; i++)
                {
                    af.NetworkType = (byte)j;
                    af.AccountType = (byte)i;

                    byte[] Address = af.GetAddress(nodes[0].PublicKey.Hex, "arpan");
                    sb.AppendLine("" + i + " - " + j + " - " + af.GetAddressString(Address));
                }
            }

            File.WriteAllText("tk.txt", sb.ToString());
            DisplayUtils.Display("DONE.");*/
        }

        private void menu_CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            CreateAccount ca = new CreateAccount();
            ca.Show();
        }

        private void menu_Benchmarks_Click(object sender, RoutedEventArgs e)
        {
            Benchmarks bm = new Benchmarks();
            bm.Show();
        }

        private void menu_Reset_Ledger_To_Genesis_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("Do you really want to reset the current state. All state information will be lost.",
                "Ledger State Reset !!!", MessageBoxButton.YesNo))
            {
                GenesisFileParser gfp = new GenesisFileParser("ACCOUNTS.GEN_PUBLIC");

                List<AccountInfo> aiData = new List<AccountInfo>();
                List<GenesisAccountData> gData;

                gfp.GetAccounts(out gData);

                foreach (Node n in nodes)
                {
                    var resp = n.PersistentAccountStore.DeleteEverything();

                    foreach (GenesisAccountData gad in gData)
                    {
                        AccountInfo ai = new AccountInfo(new Hash(gad.Public), Constants.FIN_TRE_PER_GENESIS_ACCOUNT);

                        byte[] Address = Base58Encoding.DecodeWithCheckSum(gad.Address);

                        if (Address.Length == 22)
                        {
                            ai.NetworkType = (NetworkType)Address[0];
                            ai.AccountType = (AccountType)Address[1];

                            ai.AccountState = AccountState.Normal;
                            ai.LastTransactionTime = 0;
                            ai.Name = gad.Name;

                            aiData.Add(ai);
                        }
                    }

                    n.PersistentAccountStore.AddUpdateBatch(aiData);
                }

                MessageBox.Show("ACCOUNTS RESET. It will take some time to synchronise with the network to resume normal operation.");
            }
        }

        async private void menu_Server_RecalculateTotalBalances_Click(object sender, RoutedEventArgs e)
        {
            long Value = await nodes[0].CalculateTotalMoneyInPersistentStoreAsync();
            MessageBox.Show("Total Balances: " + Value);
        }


    }
}
