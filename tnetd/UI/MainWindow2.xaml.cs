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
using System.Collections.Concurrent;
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
using TNetD.Network.PeerDiscovery;
using TNetD.Network.Networking;
using TNetD.Nodes;
using TNetD.PersistentStore;
using TNetD.Transactions;
using TNetD.Tree;
using TNetD.UI;
using TNetD.Helpers;

namespace TNetD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow2 : Window
    {
        object TimerLock = new object();

        MessageViewModel viewModel = new MessageViewModel();

        List<Node> nodes = new List<Node>();
        Random rng = new Random();
        
        public MainWindow2()
        {
            DataContext = viewModel;

            Common.Initialize();

            InitializeComponent();

            DisplayUtils.DisplayText += DisplayUtils_DisplayText;

            Title += " | " + Common.NETWORK_TYPE.ToString();

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
                    }));
                }
                catch { }
            }
        }

        private void menuItem_Simulation_Start_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 2; i++)
            {
                AddNode(i);
            }
            generateTrustlist();

        }

        private ConcurrentDictionary<Hash, PeerData> generateFakePeerList(int max)
        {
            ConcurrentDictionary<Hash, PeerData> list = new ConcurrentDictionary<Hash, PeerData>();
            int n = rng.Next(max);
            for (int i = 0; i < n; i++) {
                Hash peer = TNetUtils.GenerateNewToken();
                //list.AddOrUpdate(peer, null, (ok, ov) => ov);
            }
            return list;
        }

        private void generateTrustlist()
        {
            using (FileStream fs = new FileStream("TrustedNodes.ini", FileMode.Create))
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

        /// ///////









    }
}
