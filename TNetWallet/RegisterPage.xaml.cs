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
using TNetCommon.Address;
using TNetWallet.CryptoUtility;

namespace TNetWallet
{
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    /// 


    public partial class RegisterPage : Page
    {
        public string message;

        public static bool validusername(string username)
        {
            if (username.ToLower().Contains("@trestor.com"))
                return false;


            foreach (char c in username)
            {
                if (char.IsLetterOrDigit(c))
                    continue;
                else if (c == '.')
                    continue;

                else
                    return false;
            }

            return true;
        }

        public RegisterPage()
        {
            InitializeComponent();
        }

        void PassHandler()
        {
            Color cc = textBox_Password.PasswordQuality;

            passQualityEllipse.Fill = new SolidColorBrush(cc);

            Image_PassMatch.Visibility = (textBox_Password.Text.Length > 0 &&
                textBox_Password.Text == textBox_RepeatPassword.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        GeneratedKeyPairData kpData = new GeneratedKeyPairData();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string userName = textBox_UserName.Text.ToLower().Trim();
            string passWord = textBox_Password.Text;
            string pass_retype = textBox_RepeatPassword.Text;
            bool isSame = (passWord == pass_retype);

            if (!isSame)
            {
                textbox_passcheck.Text = "Passwords are not same";
                textBox_UserName.Text = "";
            }

            else if (!validusername(userName))
            {
                textbox_passcheck.Text = "Use only alphanumeric username, . is allowed";
                textBox_UserName.Text = "";
            }

            else if (userName.Length < 4)
            {
            }

            else
            {
                userName += "@trestor.com";
                int succ = App.UserAccessController.newUserRegistration(kpData, userName, passWord, out message);

                textbox_passcheck.Text = message;

                if (succ == 1)
                {
                    //send username, address, public key and signature to the server
                    NavigationService.Navigate(App.RegistrationConfirm);
                    //NavigationService.Navigate(App.LoginPage);
                }
            }
        }

        private void textBox_UserName_LostFocus(object sender, RoutedEventArgs e)
        {
            string UserName = textBox_UserName.Text.ToLower().Trim();

            if (UserName.Length < 4)
                username_checker.Text = "Minimum length is 3";

            else if (!validusername(UserName))

                username_checker.Text = "Invalid Username";


            else
            {
                UserName += "@trestor.com";
                if (App.UserAccessController.UserExistsLocal(UserName))
                {
                    username_checker.Text = "UserName already exists";
                    textBox_UserName.Text = "";
                }
                else
                {
                    username_checker.Text = "";
                    //send to server

                }
            }
        }

        void UpdateAccountInfo()
        {
            textBox_AccountInfo.Text = "Public Key: \n" + Convert.ToBase64String(kpData.publicKey) +
                "\nPrivate Key: \n" + Convert.ToBase64String(kpData.privateKey.Take(32).ToArray());

            // Account can have empty user or standard users

            string userName = textBox_UserName.Text.ToLower().Trim();

            if ((userName.Length == 0) || (validusername(userName) && (userName.Length > 3)))
            {
                textBox_AccountInfo.Text += "\nAddress: \n" +
                   AddressFactory.GetAddressString(AddressFactory.GetAddress(kpData.publicKey, userName), true);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Image_PassMatch.Visibility = System.Windows.Visibility.Hidden;
            kpData = new GeneratedKeyPairData();

            UpdateAccountInfo();
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            kpData = new GeneratedKeyPairData();

            UpdateAccountInfo();
        }

        private void textBox_UserName_TextChanged()
        {
            UpdateAccountInfo();
        }
    }
}
