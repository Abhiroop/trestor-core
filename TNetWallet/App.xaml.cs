using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TNetWallet.CryptoUtility;
using TNetWallet.WalletOperations;



namespace TNetWallet
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // ////////////// PAGES

        public static HomeScreen HomeScreen = new HomeScreen();
        public static LoginPage LoginPage = new LoginPage();
        public static RegisterPage RegisterPage = new RegisterPage();
        public static SendMoney SendMoney = new SendMoney();
        public static RegistrationConfirm RegistrationConfirm = new RegistrationConfirm();
        public static TransactionHistoryPage TransactionHistoryPage = new TransactionHistoryPage();


        public static bool IsAnyBodyHome = false;

        public static UserPage UserPage = new UserPage();
        // ////////////// .PAGES

        public static UserAccessController UserAccessController = new UserAccessController();

        public static Network Network = default(Network);

        public static void InitNetwork()
        {
            Network = new Network("127.0.0.1");
            Network.PacketReceived += nw_PacketReceived;
            Network.ConnectionError += nw_ConnectionError;
        }

        static void nw_ConnectionError()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    ((MainWindow)App.Current.MainWindow).SetNetworkStatus(false);
                }));
            }
            catch { }
        }

        static void nw_PacketReceived(string Type, byte[] Data)
        {
            /*this.Dispatcher.Invoke(new Action(() =>
            {
                if ("TRX_RESP" == Type)
                {
                    textBlock_Status.Text = Encoding.UTF8.GetString(Data);

                    nw.SendCommand(SP_KEY, "BAL", new byte[0]);

                  // var paragraph = new Paragraph();
                  //   paragraph.Inlines.Add(new Run("\n : " + Encoding.UTF8.GetString(Data)));
                   //  rtb_Status.Document.Blocks.Add(paragraph);

                    //rtb_Status.AppendText();//Convert.FromBase64String(Encoding.UTF8.GetString(Data)));
                }
                else if ("BAL_RESP" == Type)
                {
                    //textBlock_Status.Text = "Balance: " + Encoding.UTF8.GetString(Data);

                    textBlock_TotalMoney.Text = "Balance: " + Encoding.UTF8.GetString(Data);

                    try
                    {
                        long Money = long.Parse(Encoding.UTF8.GetString(Data));
                        moneyBar.TotalMoney = Money;
                    }
                    catch { }

                }
            }));*/
        }
        
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Network.NetworkAlive = false;
            Network.ManagementThread.Abort();
        }

        private void Application_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            App.InitNetwork();
        }



    }
}
