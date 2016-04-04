/*
 *  @Author: Arpan Jati
 *  @Description: MainWindow / UI Stuff
 */

using Grapevine;
using Grapevine.Server;
using Microsoft.Win32;
using Newtonsoft.Json;
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
using TNetD.Json.JS_Structs;
using TNetD.Ledgers;
using TNetD.Network.Networking;
using TNetD.Nodes;
using TNetD.PersistentStore;
using TNetD.SyncFramework.Packets;
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
        AccountViewModel accountViewModel = new AccountViewModel();

        public DebugWindow5()
        {
            DataContext = viewModel;

            Common.Initialize();

            InitializeComponent();

            listBox_LCS.DataContext = transactionViewModel;
            listBox_TransactionData.DataContext = transactionViewModel;
            listBox_Accounts.DataContext = accountViewModel;
            listBox_AccountHistory.DataContext = accountViewModel;

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
                        // textBlock_TransactionDetails.Text = "LCD: " + transactionViewModel.LedgerCloseData.Count;


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
                    Dispatcher.Invoke(new Action(() =>
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
            node?.StopNode();
        }

        Node node = null;

        private async void menuItem_Load_Node_Data_Click(object sender, RoutedEventArgs e)
        {
            transactionViewModel.LedgerCloseData.Clear();
            transactionViewModel.TransactionData.Clear();

            accountViewModel.Accounts.Clear();

            string nodeString = textBox_NodeID.Text;

            int node_id;

            if (!int.TryParse(nodeString, out node_id))
            {
                node_id = 0;
            }

            node?.StopNode();

            node = new Node(node_id);

            textBlock_StatusLabel.Text = "Loaded: " + node.nodeConfig.ID() + " {" + node.nodeConfig.Name + "}";

            node.LocalLedger.LedgerEvent += LocalLedger_LedgerEvent;
            node.NodeStatusEvent += nd_NodeStatusEvent;

            await node.nodeState.Persistent.CloseHistory.FetchAllLCLAsync((LedgerCloseData lcd) =>
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            transactionViewModel.LedgerCloseData.Add(new DisplayLedgerCloseType(lcd));
                        }));
                    }, null);

            await node.nodeState.Persistent.AccountStore.FetchAllAccountsAsync((AccountInfo accountInfo) =>
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            accountViewModel.Accounts.Add(new DisplayAccountInfoType(accountInfo));
                        }));

                        return TreeResponseType.NothingDone;
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

        private void listBox_LCS_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            transactionViewModel.TransactionData.Clear();

            var item = (DisplayLedgerCloseType)listBox_LCS.SelectedItem;

            if (item != null)
            {
                List<TransactionContentSet> data = new List<TransactionContentSet>();

                if (node.nodeState.Persistent.TransactionStore.FetchBySequenceNumber(out data, item.sequenceNumber, 1)
                    == DBResponse.FetchSuccess)
                {
                    if (data.Count == 1)
                    {
                        var txCSet = data[0];

                        foreach (var tc in txCSet.TxContent)
                        {
                            transactionViewModel.TransactionData.Add(new DisplayTransactionContentType(tc));
                        }
                    }
                }

                listBox_TransactionData.SelectedIndex = 0;
            }
        }

        private void listBox_TransactionData_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var tc = ((DisplayTransactionContentType)listBox_TransactionData.SelectedItem)?.TransactionContent;

            if (tc != null)
            {
                var d = new JS_TransactionReply(tc);

                textBlock_TransactionDetails.Text = JsonConvert.SerializeObject(tc, Common.JSON_SERIALIZER_SETTINGS);
            }
        }

        private void listBox_Accounts_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            accountsSelectionChanged();
        }

        private void accountsSelectionChanged()
        {
            var ai = ((DisplayAccountInfoType)listBox_Accounts.SelectedItem)?.AccountInfo;

            if (ai != null)
            {
                accountViewModel.TransactionHistory.Clear();

                textBlock_AccountDetails.Text = JsonConvert.SerializeObject(new JS_AccountReply(ai), Common.JSON_SERIALIZER_SETTINGS);

                List<TransactionContent> history;

                if (node.nodeState.Persistent.TransactionStore.FetchTransactionHistory(out history, ai.PublicKey, 0, 0) ==
                    DBResponse.FetchSuccess)
                {
                    foreach (var tc in history)
                    {
                        var dtct = new DisplayTransactionContentType(tc);

                        dtct.IsSource = tc.IsSource(ai.PublicKey);

                        accountViewModel.TransactionHistory.Add(dtct);
                    }
                }

                label_AccountDetails.Content = "Account History: " + history.Count;
            }
        }        

        private void listBox_AccountHistory_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var tc = ((DisplayTransactionContentType)listBox_AccountHistory.SelectedItem)?.TransactionContent;

            if (tc != null)
            {
                textBlock_AccountDetails.Text = JsonConvert.SerializeObject(tc, Common.JSON_SERIALIZER_SETTINGS);
            }
        }

        private void listBox_AccountHistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void listBox_Accounts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            accountsSelectionChanged();
        }
    }
}

