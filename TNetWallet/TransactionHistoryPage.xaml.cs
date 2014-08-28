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
    /// Interaction logic for TransactionHistoryPage.xaml
    /// </summary>
    public partial class TransactionHistoryPage : Page
    {
        public TransactionHistoryPage()
        {
            InitializeComponent();
        }

        private void datagrid_TransactionHistory_Loaded(object sender, RoutedEventArgs e)
        {

            SQLiteConnection sqlite_conn;
            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader;
            sqlite_conn = new SQLiteConnection(@"data source=..\..\db\db.dat; Version=3; New=True; Compress=True;");
            sqlite_conn.Open();
            sqlite_cmd = sqlite_conn.CreateCommand();

            sqlite_cmd.CommandText 
                = "SELECT * FROM TransactionHistory WHERE ((Sender = @u1) OR (Receiver = @u1))";

            sqlite_cmd.Parameters.Add(new SQLiteParameter("@u1", App.UserAccessController.LoggedUser));

            sqlite_datareader = sqlite_cmd.ExecuteReader();

            List<TransactionHistoryType> uList = new List<TransactionHistoryType>();

            while (sqlite_datareader.Read())
            {
                 string ID = sqlite_datareader["Username"].ToString();

                 string Sender = sqlite_datareader["Sender"].ToString();

                 string Receiver = sqlite_datareader["Receiver"].ToString();

                 string Amount = sqlite_datareader["Amount"].ToString();

                 var T = sqlite_datareader["Time"];

                 DateTime dt = DateTime.FromFileTime((long)T);
                 string datePatt = @"d/M/yyyy hh:mm:ss tt";
                 String Time = dt.ToString(datePatt);

                string IsSuccess = 
                    (sqlite_datareader["IsSuccess"].ToString() == "1") ? "Success" : "Failure" ;


                TransactionHistoryType tranHistory =
                    new TransactionHistoryType(ID, Sender, Receiver, Amount, Time, IsSuccess);

                uList.Add(tranHistory);
            }

            sqlite_datareader.Close();
            sqlite_conn.Close();

            // ... Assign ItemsSource of DataGrid.
            var grid = sender as DataGrid;
            grid.ItemsSource = uList;

        }
    }
}
