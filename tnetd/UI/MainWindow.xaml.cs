using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using TNetD.Network.Networking;
using TNetD.Nodes;
using TNetD.PersistentStore;
using TNetD.Transactions;
using TNetD.Tree;

namespace TNetD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Node> nodes = new List<Node>();
        GlobalConfiguration globalConfiguration;

        public MainWindow()
        {
            InitializeComponent();

            DisplayUtils.DisplayText += DisplayUtils_DisplayText;
            globalConfiguration = new GlobalConfiguration();

            nodes.Add(new Node(0, globalConfiguration));
            nodes.Add(new Node(1, globalConfiguration));
        }

        void DisplayUtils_DisplayText(string Text, Color color, DisplayType type)
        {
            try
            {
                this.Dispatcher.Invoke(new Action(() => { textBlock_StatusLog.Text += Text + "\n"; }));
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

                Constants.rngCsp.GetBytes(N_H);

                accounts.Add(new Hash(N_H));

                long _taks = Constants.random.Next(0, 1000000000);

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
        }

        private void menuItem_Server_Start_Click(object sender, RoutedEventArgs e)
        {
            //nodes[0]
            GlobalConfiguration gc = new GlobalConfiguration();
            NodeConfig nc = new NodeConfig(0, gc);

            ITransactionStore transactionStore = new SQLiteTransactionStore(nc);

            SingleTransactionFactory stf = new SingleTransactionFactory(nc.PublicKey, nc.PublicKey, 458);

            byte[] tranxData = stf.GetTransactionData();

            byte[] signature = nc.SignDataWithPrivateKey(tranxData);

            TransactionContent transactionContent;

            bool TransOk =  stf.Create(signature, out transactionContent);

            if(TransOk)
            {
                DisplayUtils.Display("Transaction Valid: ");

                DBResponse dBResponse = transactionStore.AddUpdate(transactionContent);

                DisplayUtils.Display("dBResponse: " + dBResponse);

            }



        }

    }
}
