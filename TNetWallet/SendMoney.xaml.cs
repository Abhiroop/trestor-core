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
            textBlock_Status.Text= "Connection ERROR";
        }

        void nw_PacketReceived(string Type, byte[] Data)
        {
            textBlock_Status.Dispatcher.Invoke(new Action(() =>
            {

                if ("TRANS_RESP" == Type)
                {

                    textBlock_Status.Text = Encoding.UTF8.GetString(Data);

                   /* var paragraph = new Paragraph();
                    paragraph.Inlines.Add(new Run("\n : " + Encoding.UTF8.GetString(Data)));
                    rtb_Status.Document.Blocks.Add(paragraph);*/
                 
                    //rtb_Status.AppendText();//Convert.FromBase64String(Encoding.UTF8.GetString(Data)));
                }

            }));   
        }

        private void button_Send_Click(object sender, RoutedEventArgs e)
        {
            byte[] SP_KEY = { 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 4, 5, 6, 7 };
            byte[] REC_KEY = { 1, 1, 2, 3, 4, 5, 6, 7, 1, 1, 2, 3, 4, 5, 6, 7, 1, 1, 2, 3, 4, 5, 6, 7, 1, 1, 2, 3, 4, 5, 6, 7 };

            Transactions t = new Transactions(SP_KEY, REC_KEY, int.Parse(textBox_Money.Text));

            nw.SendPacket("TRAN|" + t.Packet);
        }


    }
}
