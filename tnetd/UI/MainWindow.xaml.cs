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
using TNetD.Helpers;
using TNetD.Ledgers;
using TNetD.Network.Networking;
using TNetD.Nodes;
using TNetD.PersistentStore;
using TNetD.Tests;
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

        MessageViewModel viewModel = new MessageViewModel();

        List<Node> nodes = new List<Node>();
        
        public MainWindow()
        {
            DataContext = viewModel;

            Common.Initialize();

            InitializeComponent();

            DisplayUtils.DisplayText += DisplayUtils_DisplayText;

            lv_TX.ItemsSource = _tranxData;

            Title += " | " + Common.NETWORK_TYPE + " | " + Common.NODE_OPERATION_TYPE;
        }

        private void StartNodes()
        {
            Task.Run(async () => {
                await AddNodeAsync(0);
            }).ConfigureAwait(false);             
        }

        async Task AddNodeAsync(int idx)
        {
            Node nd = new Node(idx);
            nd.LocalLedger.LedgerEvent += LocalLedger_LedgerEvent;
            nd.NodeStatusEvent += nd_NodeStatusEvent;
            await nd.BeginBackgroundLoad();

            nodes.Add(nd);

            if (Common.NODE_OPERATION_TYPE == NodeOperationType.Distributed)
            {
                nd.VotingEnabled = true;
                nd.LedgerSyncEnabled = true;
            }
        }

        void nd_NodeStatusEvent(string Status, int NodeID)
        {
            if (NodeID == 0)
            {
                try
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        textBlock_Status.Text = Status;
                    }));
                }
                catch { }
            }
        }

        void LocalLedger_LedgerEvent(Ledger.LedgerEventType ledgerEvent, string Message)
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    textBlock_StatusLabel.Text = "" + ledgerEvent.ToString() + " - " + Message;
                    //textBlock_StatusLog.Inlines.Add(new Run(Text + "\n") { Foreground = new SolidColorBrush(color) });
                }));
            }
            catch { }
        }
        
        void DisplayUtils_DisplayText(DisplayMessageType displayMessage)
        {
            if (displayMessage.DisplayType >= Constants.DebugLevel)
            {
                try
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        displayMessage.Text = displayMessage.Text.Trim();
                        viewModel.ProcessSkips();
                        viewModel.LogMessages.Add(displayMessage);
                    }));
                }
                catch { }
            }
        }

        private void menuItem_File_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopNodes();
            Constants.ApplicationRunning = false;
        }

        private void StopNodes()
        {
            foreach (Node nd in nodes)
            {
                nd.StopNode();
            }
        }
        
        private void menuItem_Server_Start_Click(object sender, RoutedEventArgs e)
        {
            StartNodes();
        }

        private void menuItem_Server_Stop_Click(object sender, RoutedEventArgs e)
        {
            StopNodes();
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
                // Write to nodes
                foreach (Node n in nodes)
                {
                    n.nodeState.Persistent.DeleteEverything();

                    n.nodeState.Persistent.AccountStore.AddUpdateBatch(Constants.GetGenesisData());
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
        private void menuItem_Main5_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            DebugWindow5 m5 = new DebugWindow5();
            m5.ShowDialog();
            Show();
        }

        private void menuItem_setup_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            SimulationSetup swin = new SimulationSetup();
            swin.ShowDialog();
            Show();
        }

        private async void menu_IntegrityTest_Click(object sender, RoutedEventArgs e)
        {
            if (nodes.Count == 0)
            {
                using (LedgerIntegrity le = new LedgerIntegrity(0, 100, 101))
                {
                    await le.ValidateLedger();                   
                }
            }

        }
    }
}
