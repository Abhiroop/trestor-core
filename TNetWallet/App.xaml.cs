using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TNetWallet.CryptoUtility;



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
    
        

    }
}
