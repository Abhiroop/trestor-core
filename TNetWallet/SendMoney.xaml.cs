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
using TNetD;
using TNetD.Protocol;
using TNetD.Transactions;
using TNetWallet.WalletOperations;

namespace TNetWallet
{
    /// <summary>
    /// Interaction logic for SendMoney.xaml
    /// </summary>
    public partial class SendMoney : Page
    {
       

        public SendMoney()
        {
            InitializeComponent();
           
        }
        
        byte[] SP_KEY = { 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 4, 5, 6, 7 };
        byte[] REC_KEY = { 1, 1, 2, 3, 4, 5, 6, 7, 1, 1, 2, 3, 4, 5, 6, 7, 1, 1, 2, 3, 4, 5, 6, 7, 1, 1, 2, 3, 4, 5, 6, 7 };

        public void TransactionResponse(string Response)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                textBlock_Status.Text = Response;        
            }));   
        }

        public void BalanceResponse(string Balance)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                textBlock_TotalMoney.Text = "Balance: " + Balance;

                try
                {
                    long Money = long.Parse(Balance);
                    moneyBar.TotalMoney = Money;
                }
                catch { }
            }));   
        }

        private void button_Send_Click(object sender, RoutedEventArgs e)
        {
           /* try
            {
                TransactionEntity sink = new TransactionEntity(REC_KEY, long.Parse(textBox_Money.Text));

                TransactionContent tc = new TransactionContent(SP_KEY, DateTime.UtcNow.ToUnixTime(),
                    new TransactionEntity[] { sink }, App.UserAccessController.PrivateKey);

                App.Network.SendCommand(SP_KEY, "TRX", tc.Serialize());
            }
            catch { }*/
        }
        
        byte[] GenerateBalancePacket(long Time)
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            PDTs.Add(ProtocolPackager.Pack(Time, 0));
            return ProtocolPackager.PackRaw(PDTs);
        }

        private void button_Refresh_Click(object sender, RoutedEventArgs e)
        {
            App.Network.SendCommand(SP_KEY, "BAL", GenerateBalancePacket(TransactionHistory.GetLatestTransactionTime()));
        }

        private void textBox_Money_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (moneyBar != null)
            {
                moneyBar.DestinationParts = new List<long>();
                moneyBar.InvalidateVisual();
            }

            if (moneyBar != null)
            {
                try
                {
                    moneyBar.DestinationParts.Add(long.Parse(textBox_Money.Text));
                    moneyBar.InvalidateVisual();
                }
                catch { }
            }

            //MessageBox.Show("" + textBox_Money.Text);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            App.Network.SendCommand(SP_KEY, "BAL", new byte[0]);
        }
    }
}
