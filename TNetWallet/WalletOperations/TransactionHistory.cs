using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography; 

namespace TNetWallet.WalletOperations
{
    class TransactionHistory
    {

        public string getLatestTransactionTime()
        {
            SQLiteConnection sqlite_conn;
            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader;
            sqlite_conn =
                new SQLiteConnection(@"data source=..\..\db\db.dat; Version=3; New=True; Compress=True;");


            sqlite_conn.Open();
            sqlite_cmd = sqlite_conn.CreateCommand();

            sqlite_cmd.CommandText = "SELECT MAX(Time) FROM TransactionHistory";
            sqlite_datareader = sqlite_cmd.ExecuteReader();

            string outMaxTime = "";
            while (sqlite_datareader.Read())
            {
                outMaxTime = sqlite_datareader[0].ToString();
            }
            sqlite_conn.Close();

            return outMaxTime;
        }

        /// <summary>
        /// if success retun 1 otherwise 0
        /// </summary>
        /// <param name="incomingTransactionHistory"></param>
        /// <returns></returns>
        /// 
        public int pushTransactionHistoryToLocalDB(string incomingTransactionHistory)
        {
            SQLiteConnection sqlite_conn;
            SQLiteCommand sqlite_cmd;

            sqlite_conn = new SQLiteConnection(@"data source=..\..\db\db.dat; Version=3; New=True; Compress=True;");

            sqlite_conn.Open();
            sqlite_cmd = sqlite_conn.CreateCommand();

            TransactionDataSource tds = new TransactionDataSource(incomingTransactionHistory);



            foreach(TransactionData td in tds)
            {
                try
                {
                    sqlite_cmd.CommandText =
                        "INSERT INTO TransactionHistory (ID, Sender, Receiver, Amount, Time, IsSuccess) VALUES (@u1, @u2, u3, u4, u5, u6);";

                    sqlite_cmd.Parameters.Add(new SQLiteParameter("@u1", td.ID));
                    sqlite_cmd.Parameters.Add(new SQLiteParameter("@u1", td.Sender));
                    sqlite_cmd.Parameters.Add(new SQLiteParameter("@u1", td.Receiver));
                    sqlite_cmd.Parameters.Add(new SQLiteParameter("@u1", td.Amount));
                    sqlite_cmd.Parameters.Add(new SQLiteParameter("@u1", td.Time));
                    sqlite_cmd.Parameters.Add(new SQLiteParameter("@u1", td.IsSuccess));

                    sqlite_cmd.ExecuteNonQuery();
                }

                catch
                {
                    return 0;
                }
            }
            sqlite_conn.Close();
            return 1;
        }

    }
}
