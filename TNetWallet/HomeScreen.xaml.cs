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
    /// Interaction logic for HomeScreen.xaml
    /// </summary>
    public partial class HomeScreen : Page
    {
        public HomeScreen()
        {
            InitializeComponent();

            TransactionHistory th = new TransactionHistory();
            th.pushTransactionHistoryToLocalDB("");
            System.Console.WriteLine(th.getLatestTransactionTime());
        }

        private void button_CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }

        private void button_Login_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }
    }
}
