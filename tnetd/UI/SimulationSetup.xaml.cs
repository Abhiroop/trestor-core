using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TNetD.Nodes;

namespace TNetD.UI
{
    /// <summary>
    /// Interaction logic for SimulationSetup.xaml
    /// </summary>
    public partial class SimulationSetup : Window
    {

        List<Node> nodelist = new List<Node>();

        public SimulationSetup()
        {
            InitializeComponent();
        }



        private void full_conn_Checked(object sender, RoutedEventArgs e)
        {
            this.num_peers.Text = ((int)Int32.Parse(this.num_pcs.Text) * (int)Int32.Parse(this.num_nodes.Text) - 1).ToString();
            this.num_peers.IsEnabled = false;
        }



        private void full_conn_Unchecked(object sender, RoutedEventArgs e)
        {
            this.num_peers.IsEnabled = true;
        }


        private void CreateNode(int id)
        {
            Node nd = new Node(id);
            //nd.BeginBackgroundLoad();
            nd.StopNode();
            nodelist.Add(nd);
        }

        // TODO: eliminate tiny bias
        private HashSet<int> select_peers(int num, int max)
        {
            byte[] randombytes = new byte[4];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

            HashSet<int> output = new HashSet<int>();

            while (output.Count < num)
            {
                rng.GetBytes(randombytes);
                uint i = (uint)(randombytes[3] << 24 + randombytes[2] << 16 + randombytes[1] << 8 + randombytes[0]);

                output.Add((int)(i % max));
            }
            return output;
        }

        private void button_generate_Click(object sender, RoutedEventArgs e)
        {
            // convert inputs
            int pcs = Int32.Parse(this.num_pcs.Text);
            int nodes = Int32.Parse(this.num_nodes.Text);
            int peers = Int32.Parse(this.num_peers.Text);
            string[] ips = this.ip_addr.Text.Split(',');

            // basic checks
            if (ips.Length != pcs)
            {
                information.Text = "Error: Address count does not match number of PCs.";
                return;
            }
            if (peers >= pcs * nodes)
            {
                information.Text = "Error: More peers requested than nodes available.";
                return;
            }

            // generate node folders
            for (int pc = 0; pc < pcs; pc++)
            {
                for (int node = 0; node < nodes; node++)
                {
                    information.Text = "Generating Node " + node + " for PC " + ips[pc];
                    CreateNode(pc * nodes + node);
                }
            }

            // generate connections and trustlists
            for (int pc = 0; pc < pcs; pc++)
            {
                for (int node = 0; node < nodes; node++)
                {
                    int index = node + nodes * pc;
                    HashSet<int> peerlist = select_peers(peers, nodes * pcs);

                    FileStream file = new FileStream("NODE_" + index + "\\TrustedNodes.ini", FileMode.Create);
                    StreamWriter trustednodes = new StreamWriter(file, Encoding.UTF8);

                    foreach (int nidx in peerlist)
                    {
                        Node n = nodelist[nidx];
                        trustednodes.WriteLine(n.PublicKey + " "
                            + ips[nidx / nodes] + " "
                            + n.nodeConfig.ListenPortProtocol + " "
                            + "Node_" + n.nodeConfig.NodeID);
                    }
                    trustednodes.Flush();
                    trustednodes.Close();
                    file.Close();
                }
            }
        }

        private void num_pcs_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
