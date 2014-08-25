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
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }
        
        void PassHandler()
        {
            Color cc = textBox_Password.PasswordQuality;

            passQualityEllipse.Fill = new SolidColorBrush(cc);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string userName = textBox_UserName.Text;
            string passWord = textBox_Password.Text;

            int succ = App.PublicKeyManagement.newUserRegistration(userName, passWord);

            if(succ == 1)
            {
                NavigationService.Navigate(App.LoginPage);
            }
        }
    }
}
