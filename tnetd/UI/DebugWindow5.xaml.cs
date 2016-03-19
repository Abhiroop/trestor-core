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
using TNetD.Transactions;
using TNetD.Tree;
using TNetD.UI;

namespace TNetD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DebugWindow5 : Window
    {
        object TimerLock = new object();
        MessageViewModel viewModel = new MessageViewModel();
        TransactionViewModel transactionViewModel = new TransactionViewModel();

        List<Node> nodes = new List<Node>();

        public DebugWindow5()
        {
            DataContext = viewModel;


            Common.Initialize();

            InitializeComponent();


            listBox_LCS.DataContext = transactionViewModel;

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
                try
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        textBlock_Status2.Text = "LCD: " + transactionViewModel.LedgerCloseData.Count;


                    }));
                }
                catch { }
            }
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
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (Node nd in nodes)
            {
                nd.StopNode();
            }
        }

        Node node = default(Node);

        private async void menuItem_Load_Node_Data_Click(object sender, RoutedEventArgs e)
        {
            node = new Node(0);
            node.LocalLedger.LedgerEvent += LocalLedger_LedgerEvent;
            node.NodeStatusEvent += nd_NodeStatusEvent;

            await node.nodeState.PersistentCloseHistory.FetchAllLCLAsync((LedgerCloseData lcd) =>
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            transactionViewModel.LedgerCloseData.Add(new DisplayLedgerCloseType(lcd));
                        }));
                    });

        }

        private void LocalLedger_LedgerEvent(Ledger.LedgerEventType ledgerEvent, string Message)
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    textBlock_Status.Text = ledgerEvent + " - " + Message;
                }));
            }
            catch { }
        }
    }
}
