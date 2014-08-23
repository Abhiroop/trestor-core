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
using TNetWallet.WalletOperations;

namespace TNetWallet
{
    /// <summary>
    /// Interaction logic for SendMoney.xaml
    /// </summary>
    public partial class SendMoney : Page
    {
        Network nw = default(Network);

        public SendMoney()
        {
            InitializeComponent();
            nw = new Network("127.0.0.1");
            nw.PacketReceived += nw_PacketReceived;
            nw.ConnectionError += nw_ConnectionError;
        }

        void nw_ConnectionError()
        {
            rtb_Status.AppendText("\n Connection ERROR");
        }

        void nw_PacketReceived(string Type, byte[] Data)
        {
            rtb_Status.AppendText("\n : " + Type);
        }

        private void button_Send_Click(object sender, RoutedEventArgs e)
        {
            byte[] SP_KEY = { 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 4, 5, 6, 7 };
            byte[] REC_KEY = { 1, 1, 2, 3, 4, 5, 6, 7, 1, 1, 2, 3, 4, 5, 6, 7, 1, 1, 2, 3, 4, 5, 6, 7, 1, 1, 2, 3, 4, 5, 6, 7 };

            Transactions t = new Transactions(SP_KEY, REC_KEY, 5);

            nw.SendPacket("TRAN|" + t.Packet);
        }


    }
}
