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
    public partial class MainWindow2 : Window
    {
        List<Node> nodes = new List<Node>();
        GlobalConfiguration globalConfiguration;
        
        public MainWindow2()
        {
            Common.Initialize();

            InitializeComponent();

            DisplayUtils.DisplayText += DisplayUtils_DisplayText;
            globalConfiguration = new GlobalConfiguration();


            Title += " | " + Common.NetworkType.ToString();
            
            //System.Timers.Timer tmr_UI = new System.Timers.Timer(100);
            //tmr_UI.Elapsed += tmr_UI_Elapsed;
            //tmr_UI.Start();
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

            AddNode(0);
            AddNode(1);
            AddNode(2);
            
            nodes[0].nodeState.NetworkTime += 200 * 10000;
            nodes[1].nodeState.NetworkTime += 500 * 10000;
            nodes[2].nodeState.NetworkTime += 300 * 10000;


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
