﻿/*
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
    public partial class DebugWindow4 : Window
    {
        object TimerLock = new object();


        List<Node> nodes = new List<Node>();
        GlobalConfiguration globalConfiguration;

        public DebugWindow4()
        {
            Common.Initialize();

            InitializeComponent();

            DisplayUtils.DisplayText += DisplayUtils_DisplayText;
            globalConfiguration = new GlobalConfiguration();


            Title += " | " + Common.NetworkType.ToString();

            System.Timers.Timer tmr_UI = new System.Timers.Timer(1000);
            tmr_UI.Elapsed += tmr_UI_Elapsed;
            tmr_UI.Start();
        }

        private void tmr_UI_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (TimerLock)
            {
                StringBuilder connData = new StringBuilder();

                foreach (Node nd in nodes)
                {
                    connData.AppendLine("\n\n\n NODE ID " + nd.nodeConfig.NodeID + "   KEY: " + nd.PublicKey);
                    connData.AppendLine(" Ledger Hash : " + nd.nodeState.Ledger.GetRootHash());

                    connData.AppendLine("  OUTGOING ----> ");

                    foreach (Hash h in nd.networkPacketSwitch.GetConnectedNodes(ConnectionListType.Outgoing))
                    {
                        connData.AppendLine("\t" + h.ToString());
                    }

                    connData.AppendLine("  INCOMING ----> ");

                    foreach (Hash h in nd.networkPacketSwitch.GetConnectedNodes(ConnectionListType.Incoming))
                    {
                        connData.AppendLine("\t" + h.ToString());
                    }

                    connData.AppendLine("  ConnectedValidators ----> ");

                    foreach (Hash h in nd.nodeState.ConnectedValidators)
                    {
                        connData.AppendLine("\t" + h.ToString());
                    }
                }

                try
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {

                        textBlock_Log2.Text = connData.ToString();
                    }));
                }
                catch { }

            }
        }

        void AddNode(int idx)
        {
            Node nd = new Node(idx, globalConfiguration);
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
            for (int i = 0; i < 2; i++)
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
                Common.rngCsp.GetBytes(N_H);
                
                Hash h = new Hash(N_H);

                AccountInfo ai = new AccountInfo(h, Common.random.Next(79382, 823649238), 
                    "name_" + Common.random.Next(0, 823649238), AccountState.Normal, NetworkType.TestNet, AccountType.TestNormal, 0);

                nodes[0].nodeState.Ledger.AddUserToLedger(ai);
            }

        }

        /// ///////

    }
}