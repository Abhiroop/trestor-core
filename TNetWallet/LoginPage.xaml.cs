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

namespace TNetWallet
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            string userName = text_username.Text;
            string passWord = text_password.Text;

            string message;
            int succ = App.PublicKeyManagement.userLogin(userName, passWord, out message);

            textbloc_login_status.Text = message;

            if (succ == 1)
                NavigationService.Navigate(App.SendMoney);

            else
            {
                text_username.Text = "";
            }
        }
    }
}
