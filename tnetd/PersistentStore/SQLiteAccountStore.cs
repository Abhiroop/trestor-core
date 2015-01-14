﻿
//
// @Author: Arpan Jati
// @Date: 1-2-3-6 Jan 2015 | 15 Jan 2015
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
            sqliteConnection = new SQLiteConnection("Data Source=" + config.Path_AccountDB + ";Version=3;");
            sqliteConnection.Open();

            VerifyTables();
        }

        public bool AccountExists(Hash publicKey)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM Ledger WHERE PublicKey = @publicKey", sqliteConnection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@publicKey", publicKey.Hex));
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        
        public Tuple<DBResponse, long> FetchAllAccounts(AccountFetchEventHandler accountFetch)
        {
            DBResponse response = DBResponse.FetchFailed;

            long Records = 0;

            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM Ledger", sqliteConnection))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {                    
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Hash _publicKey = new Hash((byte[])reader[0]);
                            string userName = (string)reader[1];
                            long balance = (long)reader[2];
                            long accountState = (long)reader[3];
                            long lastTransactionTime = (long)reader[4];

                            if(accountFetch != null)
                            {
                                AccountInfo accountInfo = new AccountInfo(_publicKey, balance, userName, (AccountState)accountState, lastTransactionTime);
                                accountFetch(accountInfo);
                            }
                            
                            response = DBResponse.FetchSuccess;

                            Records++;
                        }
                    }
                }
            }

            return new Tuple<DBResponse, long>(response, Records);
        }


        public DBResponse FetchAccount(out AccountInfo accountInfo, Hash publicKey)
        {
            DBResponse response = DBResponse.FetchFailed;

            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM Ledger WHERE PublicKey = @publicKey", sqliteConnection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@publicKey", publicKey.Hex));
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    accountInfo = default(AccountInfo);

                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            Hash _publicKey = new Hash((byte[])reader[0]);
                            string userName = (string)reader[1];
                            long balance = (long)reader[2];
                            long accountState = (long)reader[3];
                            long lastTransactionTime = (long)reader[4];

                            if (_publicKey == publicKey)
                            {
                                accountInfo = new AccountInfo(_publicKey, balance, userName, (AccountState)accountState, lastTransactionTime);
                                response = DBResponse.FetchSuccess;
                            }
                        }
                    }
                }
            }

            return response;
        }

        public int AddUpdateBatch(List<AccountInfo> accountInfoData)
        {
            int Successes = 0;
            SQLiteTransaction st = sqliteConnection.BeginTransaction();

            foreach (AccountInfo ai in accountInfoData)
            {
                DBResponse resp = AddUpdate(ai);
                if ((resp == DBResponse.InsertSuccess) || (resp == DBResponse.UpdateSuccess))
                {
                    Successes++;
                }
            }

            st.Commit();
            return Successes;
        }

        public DBResponse AddUpdate(AccountInfo accountInfo)
        {
            bool doUpdate = false;

            using (SQLiteCommand cmd = new SQLiteCommand("SELECT PublicKey FROM Ledger WHERE PublicKey = @publicKey", sqliteConnection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@publicKey", accountInfo.PublicKey.Hex));
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        doUpdate = true; // Perform update as the entry already exists.
                    }
                }
            }

            DBResponse response = DBResponse.Exception;

            if (doUpdate)
            {
                // /////////////  Perform the UPDATE  ///////////////

                using (SQLiteCommand cmd = new SQLiteCommand("UPDATE Ledger SET UserName = @userName, Balance = @balance, AccountState = @accountState, LastTransaction = @transactionTime WHERE PublicKey = @publicKey;", sqliteConnection))
                {
                    cmd.Parameters.Add(new SQLiteParameter("@publicKey", accountInfo.PublicKey.Hex));
                    cmd.Parameters.Add(new SQLiteParameter("@userName", accountInfo.Name));
                    cmd.Parameters.Add(new SQLiteParameter("@balance", accountInfo.Money));
                    cmd.Parameters.Add(new SQLiteParameter("@accountState", (byte)accountInfo.AccountState));
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
            }
            else
            {
                // /////////////  Perform the INSERT  ///////////////

                using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO Ledger VALUES(@publicKey, @userName, @balance, @accountState, @transactionTime);", sqliteConnection))
                {
                    cmd.Parameters.Add(new SQLiteParameter("@publicKey", accountInfo.PublicKey.Hex));
                    cmd.Parameters.Add(new SQLiteParameter("@userName", accountInfo.Name));
                    cmd.Parameters.Add(new SQLiteParameter("@balance", accountInfo.Money));
                    cmd.Parameters.Add(new SQLiteParameter("@accountState", (byte)accountInfo.AccountState));
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
            }

            return response;
        }

        private void VerifyTables()
        {
            if (sqliteConnection.State == System.Data.ConnectionState.Open)
            {
                if (!DBUtils.TableExists("Ledger", sqliteConnection))
                {
                    DBUtils.ExecuteNonQuery("CREATE TABLE Ledger (PublicKey BLOB PRIMARY KEY, UserName TEXT, Balance INTEGER, AccountState INTEGER, LastTransaction INTEGER);", sqliteConnection);
                }

                if (!DBUtils.TableExists("LedgerInfo", sqliteConnection))
                {
                    DBUtils.ExecuteNonQuery("CREATE TABLE LedgerInfo (LedgerHash BLOB PRIMARY KEY, LastLedgerHash BLOB, LCLTime INTEGER, SequenceNumber INTEGER);", sqliteConnection);
                }
            }
        }

        public DBResponse Delete(Hash publicKey)
        {
            DBResponse response = DBResponse.DeleteFailed;

            using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Ledger WHERE (PublicKey = @publicKey);", sqliteConnection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@publicKey", publicKey.Hex));

                if (cmd.ExecuteNonQuery() == 1) // There should be a single entry for a PublicKey.
                {
                    response = DBResponse.DeleteSuccess;
                }
            }

            return response;
        }

        public Tuple<DBResponse, long> DeleteEverything()
        {
            DBResponse response = DBResponse.DeleteFailed;
            int removed = 0;

            using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Ledger;", sqliteConnection))
            {
                removed = cmd.ExecuteNonQuery();
                if (removed > 0) // There should be atleast single entry for a PublicKey.
                {
                    response = DBResponse.DeleteSuccess;
                }
            }

            return new Tuple<DBResponse, long>(response, removed);
        }


    }
}
