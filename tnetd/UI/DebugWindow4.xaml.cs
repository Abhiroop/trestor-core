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
using System.Windows.Controls;
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
using TNetD.Transactions;
using TNetD.Tree;
using TNetD.UI;

namespace TNetD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DebugWindow4 : Window
    {
        object TimerLock = new object();
        MessageViewModel viewModel = new MessageViewModel();
        List<Node> nodes = new List<Node>();

        public DebugWindow4()
        {
            DataContext = viewModel;
            Common.Initialize();

            InitializeComponent();            

            DisplayUtils.DisplayText += DisplayUtils_DisplayText;

            Title += " | " + Common.NETWORK_TYPE.ToString();

            comboBox_Graph_DisplayMode.SelectedIndex = 0;

            System.Timers.Timer tmr_UI = new System.Timers.Timer(1000);
            tmr_UI.Elapsed += tmr_UI_Elapsed;
            tmr_UI.Start();
        }

        private void tmr_UI_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (TimerLock)
            {
                string connString = TNetUtils.GetNodeConnectionInfoString(nodes);

                try
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        textBlock_Log2.Text = connString;
                    }));
                }
                catch { }
            }
        }

        void AddNode(int idx)
        {
            Node nd = new Node(idx);
            // nd.LocalLedger.LedgerEvent += LocalLedger_LedgerEvent;
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


        void LoadNodes()
        {

        }

        void DisplayUtils_DisplayText(DisplayMessageType displayMessage)
        {
            if (displayMessage.DisplayType >= Constants.DebugLevel)
            {
                try
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        displayMessage.Text = displayMessage.Text.Trim();
                        viewModel.ProcessSkips();
                        viewModel.LogMessages.Add(displayMessage);

                        if (!listBox_Log.IsMouseOver)
                        {
                            //listBox_Log.Items.MoveCurrentToLast();
                            //listBox_Log.ScrollIntoView(listBox_Log.Items.CurrentItem);
                            try { listBox_Log.ScrollIntoView(listBox_Log.Items[listBox_Log.Items.Count - 1]); } catch { }
                        }
                    }));
                }
                catch { }
            }
        }

        private void menuItem_Simulation_Start_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 6; i++)
            {
                AddNode(i);
            }

            generateTrustlist();

            connectionMap.InitNodes(nodes);
        }

        private void generateTrustlist()
        {
            using (FileStream fs = new FileStream("newTrustList.ini", FileMode.Create))
            {
                using (StreamWriter w = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (Node n in nodes)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(n.nodeConfig.PublicKey);
                        sb.Append(" 127.0.0.1 ");
                        sb.Append(n.nodeConfig.ListenPortProtocol);
                        sb.Append(" Node_");
                        sb.Append(n.nodeConfig.NodeID);

                        w.WriteLine(sb.ToString());
                    }
                }
            }
        }

        private void menuItem_File_Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (Node nd in nodes)
            {
                nd.StopNode();
            }
        }

        private void menuItem_Simulation_RamdomlyAddAccounts_Click(object sender, RoutedEventArgs e)
        {
            int count = 5000;

            long received_takas = 0;

            for (int i = 0; i < 5000; i++)
            {
                byte[] N_H = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };
                Common.SECURE_RNG.GetBytes(N_H);

                Hash h = new Hash(N_H);

                AccountInfo ai = new AccountInfo(h, Common.NORMAL_RNG.Next(79382, 823649238),
                    "name_" + Common.NORMAL_RNG.Next(0, 823649238), AccountState.Normal, NetworkType.TestNet, AccountType.TestNormal, 0);

                nodes[0].nodeState.Ledger.AddUserToLedger(ai);
            }
        }

        private void menuItem_ResetLayout_Click(object sender, RoutedEventArgs e)
        {
            connectionMap.InitNodes(nodes);
        }

        private void comboBox_Graph_DisplayMode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string text = (e.AddedItems[0] as ComboBoxItem).Content as string;

            switch(text)
            {
                case "Voting":
                    connectionMap.DisplayMode = ConnectionMapDisplayMode.Voting;
                    break;

                case "Ledger Sync": 
                    connectionMap.DisplayMode = ConnectionMapDisplayMode.LedgerSync;
                    break;

                case "Trust": 
                    connectionMap.DisplayMode = ConnectionMapDisplayMode.Trust;
                    break;

                case "Time Sync": 
                    connectionMap.DisplayMode = ConnectionMapDisplayMode.TimeSync;
                    break;
            }
        }

        private void button_Graph_ResetLayout_Click(object sender, RoutedEventArgs e)
        {
            connectionMap.InitNodes(nodes);
        }

        private void menuItem_EnableVoting_Click(object sender, RoutedEventArgs e)
        {
            foreach(var node in nodes)
            {
                node.VotingEnabled = true;
            }
        }

        private void menuItem_DisableVoting_Click(object sender, RoutedEventArgs e)
        {
            foreach (var node in nodes)
            {
                node.VotingEnabled = false;
            }
        }

        private void menuItem_ResetLedgerToGenesis_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("Do you really want to reset the current state. All state information will be lost.",
               "Ledger State Reset !!!", MessageBoxButton.YesNo))
            {
                List<AccountInfo> aiData = new List<AccountInfo>();

                string[] Accs = Common.NETWORK_TYPE == NetworkType.MainNet ? GenesisRawData.MainNet : GenesisRawData.TestNet;

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

        /// ///////

    }
}
