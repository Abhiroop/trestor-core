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

        public void setUser(string user)
        {
            text_username.Text = user.Replace("@trestor.com", "");
            text_password.clearPassword();
            textbloc_login_status.Text = "";
        }

        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            string u = text_username.Text.ToLower().Trim();
            string userName = text_username.Text.ToLower().Trim() + "@trestor.com";
            string passWord = text_password.Text;

            string message;
            int succ = App.UserAccessController.userLogin(userName, passWord, out message);

            textbloc_login_status.Text = message;

            MainWindow mw = (MainWindow)App.Current.MainWindow;



            if (succ == 1)
            {
                mw.SetUserName(u);
                App.IsAnyBodyHome = true;
                NavigationService.Navigate(App.SendMoney);
            }

            else
            {
                //text_username.Text = "";
                text_password.clearPassword();
            }
        }
    }
}
