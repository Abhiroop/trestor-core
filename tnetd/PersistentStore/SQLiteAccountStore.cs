
//
// @Author: Arpan Jati
// @Date: Jan 1-2-3, 2015
//

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using TNetD.Nodes;
using TNetD.Transactions;

namespace TNetD.PersistentStore
{
    /// <summary>
    /// Stores account nodes in an SQLite database.
    /// </summary>
    class SQLiteAccountStore : IPersistentAccountStore
    {
        SQLiteConnection sqliteConnection = default(SQLiteConnection);

        public SQLiteAccountStore(NodeConfig config)
        {
            sqliteConnection = new SQLiteConnection("Data Source=" + config.DBPath + ";Version=3;");
            sqliteConnection.Open();

            VerifyTables();
        }

        bool TableExists(string tableName)
        {
            bool exists = false;
            SQLiteCommand cmd = new SQLiteCommand("select name from sqlite_master where type='table' and name=@name", sqliteConnection);
            cmd.Parameters.Add(new SQLiteParameter("@name", tableName));
            SQLiteDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                exists = true;
            }

            cmd.Dispose();
            return exists;
        }

        public bool AccountExists(Hash publicKey)
        {
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM Ledger WHERE PublicKey = @publicKey", sqliteConnection);
            cmd.Parameters.Add(new SQLiteParameter("@publicKey", publicKey));
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                cmd.Dispose();
                return true;
            }
            cmd.Dispose();
            return false;
        }

        public DBResponse FetchAccount(Hash publicKey, out AccountInfo accountInfo)
        {
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM Ledger WHERE PublicKey = @publicKey", sqliteConnection);
            cmd.Parameters.Add(new SQLiteParameter("@publicKey", publicKey));
            SQLiteDataReader reader = cmd.ExecuteReader();

            DBResponse response = DBResponse.FetchFailed;

            accountInfo = default(AccountInfo);

            if (reader.HasRows)
            {
                if (reader.Read())
                {
                    Hash _publicKey = new Hash((byte[])reader[0]);
                    string userName = (string)reader[1];
                    long balance = (long)reader[2];
                    byte accountState = (byte)reader[3];
                    long lastTransactionTime = (long)reader[4];

                    if (_publicKey == publicKey)
                    {
                        accountInfo = new AccountInfo(_publicKey, balance, userName, (TNetD.Transactions.AcountState)accountState, lastTransactionTime);
                        response = DBResponse.FetchSuccess;
                    }
                }
            }

            cmd.Dispose();

            return response;
        }

        public DBResponse AddUpdate(AccountInfo accountInfo)
        {
            SQLiteCommand cmd = new SQLiteCommand("SELECT PublicKey FROM Ledger WHERE PublicKey = @publicKey", sqliteConnection);
            cmd.Parameters.Add(new SQLiteParameter("@publicKey", accountInfo.PublicKey.Hex));
            SQLiteDataReader reader = cmd.ExecuteReader();

            bool doUpdate = false;

            if (reader.HasRows)
            {
                doUpdate = true; // Perform update as the entry already exists.
            }

            cmd.Dispose();

            DBResponse response = DBResponse.Exception;

            if (doUpdate)
            {
                // /////////////  Perform the UPDATE  ///////////////

                cmd = new SQLiteCommand("UPDATE Ledger SET UserName = @userName, Balance = @balance, AccountState = @accountState, LastTransaction = @transactionTime WHERE PublicKey = @publicKey; ", sqliteConnection);

                cmd.Parameters.Add(new SQLiteParameter("@publicKey", accountInfo.PublicKey.Hex));
                cmd.Parameters.Add(new SQLiteParameter("@userName", accountInfo.Name));
                cmd.Parameters.Add(new SQLiteParameter("@balance", accountInfo.Money));
                cmd.Parameters.Add(new SQLiteParameter("@accountState", accountInfo.AccountState));
                cmd.Parameters.Add(new SQLiteParameter("@transactionTime", accountInfo.LastTransactionTime));

                if (cmd.ExecuteNonQuery() != 1)
                {
                    response = DBResponse.UpdateFailed;
                }
                else
                {
                    response = DBResponse.UpdateSuccess;
                }
            }
            else
            {
                // /////////////  Perform the INSERT  ///////////////

                cmd = new SQLiteCommand("INSERT INTO Ledger VALUES(@publicKey, @userName, @balance, @accountState, @transactionTime);", sqliteConnection);

                cmd.Parameters.Add(new SQLiteParameter("@publicKey", accountInfo.PublicKey.Hex));
                cmd.Parameters.Add(new SQLiteParameter("@userName", accountInfo.Name));
                cmd.Parameters.Add(new SQLiteParameter("@balance", accountInfo.Money));
                cmd.Parameters.Add(new SQLiteParameter("@accountState", accountInfo.AccountState));
                cmd.Parameters.Add(new SQLiteParameter("@transactionTime", accountInfo.LastTransactionTime));

                if (cmd.ExecuteNonQuery() != 1)
                {
                    response = DBResponse.InsertFailed;
                }
                else
                {
                    response = DBResponse.InsertSuccess;
                }
            }

            cmd.Dispose();

            return response;
        }

        int ExecuteNonQuery(string Query)
        {
            int reply = -1;
            SQLiteCommand cmd = new SQLiteCommand(Query, sqliteConnection);
            reply = cmd.ExecuteNonQuery();
            cmd.Dispose();
            return reply;
        }

        private void VerifyTables()
        {
            if (sqliteConnection.State == System.Data.ConnectionState.Open)
            {
                if (!TableExists("Ledger"))
                {
                    ExecuteNonQuery("CREATE TABLE Ledger (PublicKey BLOB PRIMARY KEY, UserName TEXT, Balance INTEGER, AccountState INTEGER, LastTransaction INTEGER);");
                }

                if (!TableExists("LedgerInfo"))
                {
                    ExecuteNonQuery("CREATE TABLE LedgerInfo (LedgerHash BLOB PRIMARY KEY, LastLedgerHash BLOB, LCLTime INTEGER, SequenceNumber INTEGER);");
                }
            }
        }

        public DBResponse Delete(Hash publicKey)
        {
            SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Ledger WHERE (PublicKey = @publicKey)", sqliteConnection);
            cmd.Parameters.Add(new SQLiteParameter("@publicKey", publicKey.Hex));
            DBResponse response = DBResponse.DeleteFailed;
            
            if (cmd.ExecuteNonQuery() == 1) // There should be a single entry for a PublicKey.
            {
                response = DBResponse.DeleteSuccess;
            }

            cmd.Dispose();
            return response;
        }



    }
}
