﻿using System;
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
        public static long GetLatestTransactionTime()
        {
            SQLiteConnection sqlite_conn;
            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader;
            sqlite_conn = new SQLiteConnection(Constants.ConnectionString);
            sqlite_conn.Open();
            sqlite_cmd = sqlite_conn.CreateCommand();

            sqlite_cmd.CommandText = "SELECT MAX(Time) FROM TransactionHistory";
            sqlite_datareader = sqlite_cmd.ExecuteReader();

            long outMaxTime = 0;
            while (sqlite_datareader.Read())
            {
                outMaxTime = (long)sqlite_datareader[0];
            }

            sqlite_datareader.Close();
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

            sqlite_conn = new SQLiteConnection(Constants.ConnectionString);

            sqlite_conn.Open();
            sqlite_cmd = sqlite_conn.CreateCommand();

            TransactionDataSource tds = new TransactionDataSource(incomingTransactionHistory);



            foreach (TransactionHistoryType td in tds)
            {
                try
                {
                    sqlite_cmd.CommandText =
                        "INSERT INTO TransactionHistory (ID, Sender, Receiver, Amount, Time, IsSuccess) VALUES (@u1, @u2, u3, u4, u5, u6);";

                    sqlite_cmd.Parameters.Add(new SQLiteParameter("@u1", td.ID));
                    sqlite_cmd.Parameters.Add(new SQLiteParameter("@u2", td.Sender));
                    sqlite_cmd.Parameters.Add(new SQLiteParameter("@u3", td.Receiver));
                    sqlite_cmd.Parameters.Add(new SQLiteParameter("@u4", td.Amount));
                    sqlite_cmd.Parameters.Add(new SQLiteParameter("@u5", td.Time));
                    sqlite_cmd.Parameters.Add(new SQLiteParameter("@u6", td.IsSuccess));

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
