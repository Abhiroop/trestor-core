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
using System.Windows.Shapes;
using TNetD.Nodes;

namespace TNetD.UI
{
    /// <summary>
    /// Interaction logic for SimulationSetup.xaml
    /// </summary>
    public partial class SimulationSetup : Window
    {

        List<Node> nodes = new List<Node>();

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
            nd.BeginBackgroundLoad();
            nd.StopNode();
        }


        private void button_generate_Click(object sender, RoutedEventArgs e)
        {
            int pcs = Int32.Parse(this.num_pcs.Text);
            int nodes = Int32.Parse(this.num_nodes.Text);
            int peers = Int32.Parse(this.num_peers.Text);
            string[] ips = this.ip_addr.Text.Split(',');
            if (ips.Length != pcs)
            {
                ip_addr.Text = "Error, address count does not match. " + ips[0];
            }

            for (int pc = 0; pc < pcs; pc++)
            {
                System.IO.Directory.CreateDirectory(ips[pc]);
                for (int node = 0; node < nodes; node++)
                {
                    information.Text = "Generating Node " + node + " for PC " + ips[pc];
                    CreateNode(node);
                    System.IO.Directory.Move("NODE_" + node, ips[pc]);
                }
            }

        }
    }
}
