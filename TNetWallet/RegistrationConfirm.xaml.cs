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
    /// Interaction logic for RegistrationConfirm.xaml
    /// </summary>
    public partial class RegistrationConfirm : Page
    {
        
        public RegistrationConfirm()
        {
            InitializeComponent();
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            textblock_message.Text = App.RegisterPage.message + 
                "\nyour public key\n" + Convert.ToBase64String(App.PublicKeyManagement.PublicKey) +
                "\nyour private key \n" + Convert.ToBase64String(App.PublicKeyManagement.PrivateKey);
        }

        private void Button_login_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(App.LoginPage);
        }
    }
}
