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
using System.Data.SQLite;
using TNetWallet.WalletOperations;

namespace TNetWallet
{
    /// <summary>
    /// Interaction logic for UserPage.xaml
    /// </summary>
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            SQLiteConnection sqlite_conn;
            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader;
            sqlite_conn = new SQLiteConnection(@"data source=..\..\db\db.dat; Version=3; New=True; Compress=True;");
            sqlite_conn.Open();
            sqlite_cmd = sqlite_conn.CreateCommand();

            sqlite_cmd.CommandText = "SELECT Username, LastLoginTime FROM AppUserTable";

            sqlite_datareader = sqlite_cmd.ExecuteReader();

            List<UserList> uList = new List<UserList>();

            while (sqlite_datareader.Read())
            {
                string LTime = "Not yet logged in";
                try
                {
                    var now = sqlite_datareader["LastLoginTime"];
                    DateTime dt = DateTime.FromFileTime((long)now);
                    string datePatt = @"d/M/yyyy hh:mm:ss tt";
                    LTime = dt.ToString(datePatt);
                }
                catch { }

                UserList userList =
                    new UserList(sqlite_datareader["Username"].ToString(), LTime);
                uList.Add(userList);
            }
            sqlite_datareader.Close();


            // ... Assign ItemsSource of DataGrid.
            var grid = sender as DataGrid;
            grid.ItemsSource = uList;
        }

        private void datagrid_UserGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid dg = datagrid_UserGrid;

            UserList ul = (UserList)dg.SelectedItem;

            string username = ul.UserName;

            App.LoginPage.setUser(ul.UserName);

            App.UserAccessController.logOut();

            ((MainWindow)App.Current.MainWindow).SetUserName("Welcome to Trestor Net");

            NavigationService.Navigate(App.LoginPage);


        }
    }
}
