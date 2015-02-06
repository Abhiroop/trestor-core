
//
// @Author: Arpan Jati
// @Date: 6 Feb 2015

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Address;
using TNetD.Nodes;
using TNetD.Transactions;

namespace TNetD.PersistentStore
{

    public class LedgerCloseData
    {
        // DBUtils.ExecuteNonQuery("CREATE TABLE LedgerInfo (SequenceNumber INTEGER PRIMARY KEY AUTOINCREMENT, 
        //         LedgerHash BLOB, Transactions INTEGER, CloseTime INTEGER);", sqliteConnection);

        public long SequenceNumber;
        public byte[] LedgerHash;
        public long Transactions;
        public long TotalTransactions;
        public long CloseTime;
    }

    /// <summary>
    /// Stores  Ledger Close History in an SQLite database.
    /// </summary>
    class SQLiteCloseHistory
    {
        SQLiteConnection sqliteConnection = default(SQLiteConnection);

        public SQLiteCloseHistory(NodeConfig config)
        {
            sqliteConnection = new SQLiteConnection("Data Source=" + config.Path_AccountDB + ";Version=3;");
            sqliteConnection.Open();

            VerifyTables();
        }

        public bool LCLExists(Hash ledgerHash)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM LedgerInfo WHERE LedgerHash = @ledgerHash;", sqliteConnection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@ledgerHash", ledgerHash.Hex));
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

        public bool LCLExists(long sequenceNumber)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM LedgerInfo WHERE SequenceNumber = @sequenceNumber;", sqliteConnection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@sequenceNumber", sequenceNumber));
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

        public DBResponse FetchLCL(out LedgerCloseData ledgerCloseData, Hash ledgerHash)
        {
            DBResponse response = DBResponse.FetchFailed;

            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM LedgerInfo WHERE LedgerHash = @ledgerHash;", sqliteConnection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@ledgerHash", ledgerHash.Hex));
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    ledgerCloseData = new LedgerCloseData();

                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            ledgerCloseData.SequenceNumber = (long)reader[0];
                            ledgerCloseData.LedgerHash = (byte[])reader[1];
                            ledgerCloseData.Transactions = (long)reader[2];
                            ledgerCloseData.TotalTransactions = (long)reader[3];
                            ledgerCloseData.CloseTime = (long)reader[4];
                            response = DBResponse.FetchSuccess;
                        }
                    }
                }
            }

            return response;
        }

        ////////////////////////

        public DBResponse FetchLCL(out LedgerCloseData ledgerCloseData, long sequenceNumber)
        {
            DBResponse response = DBResponse.FetchFailed;

            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM LedgerInfo WHERE SequenceNumber = @sequenceNumber;", sqliteConnection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@sequenceNumber", sequenceNumber));
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    ledgerCloseData = new LedgerCloseData();

                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            ledgerCloseData.SequenceNumber = (long)reader[0];
                            ledgerCloseData.LedgerHash = (byte[])reader[1];
                            ledgerCloseData.Transactions = (long)reader[2];
                            ledgerCloseData.TotalTransactions = (long)reader[3];
                            ledgerCloseData.CloseTime = (long)reader[4];
                            response = DBResponse.FetchSuccess;
                        }
                    }
                }
            }

            return response;
        }

        public int BatchFetch(out Dictionary<Hash, LedgerCloseData> lastClosedLedgers, IEnumerable<Hash> ledgerHashes)
        {
            lastClosedLedgers = new Dictionary<Hash, LedgerCloseData>();

            foreach (Hash ledgerHash in ledgerHashes)
            {
                LedgerCloseData ledgerCloseData;
                if (FetchLCL(out ledgerCloseData, ledgerHash) == DBResponse.FetchSuccess)
                {
                    lastClosedLedgers.Add(ledgerHash, ledgerCloseData);
                }
            }

            return lastClosedLedgers.Count();
        }

        //////////////////////////////////////////

        public int AddUpdateBatch(IEnumerable<LedgerCloseData> ledgerCloseData)
        {
            int Successes = 0;
            SQLiteTransaction st = sqliteConnection.BeginTransaction();

            foreach (LedgerCloseData lcd in ledgerCloseData)
            {
                DBResponse resp = AddUpdate(lcd, st);
                if ((resp == DBResponse.InsertSuccess) || (resp == DBResponse.UpdateSuccess))
                {
                    Successes++;
                }
            }

            st.Commit();
            return Successes;
        }

        // //////////////////////////////////////

        public DBResponse AddUpdate(LedgerCloseData ledgerCloseData)
        {
            SQLiteTransaction st = sqliteConnection.BeginTransaction();
            DBResponse resp = AddUpdate(ledgerCloseData, st);
            st.Commit();
            return resp;
        }

        public DBResponse AddUpdate(LedgerCloseData ledgerCloseData, DbTransaction transaction)
        {
            bool doUpdate = false;

            using (SQLiteCommand cmd = new SQLiteCommand("SELECT LedgerHash FROM LedgerInfo WHERE LedgerHash = @ledgerHash;",
                sqliteConnection, (SQLiteTransaction)transaction))
            {
                cmd.Parameters.Add(new SQLiteParameter("@ledgerHash", ledgerCloseData.LedgerHash));
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

                using (SQLiteCommand cmd = new SQLiteCommand("UPDATE LedgerInfo SET SequenceNumber = @sequenceNumber, Transactions = @transactions, TotalTransactions = @totalTransactions, CloseTime=@closeTime WHERE LedgerHash = @ledgerHash;",
                    sqliteConnection, (SQLiteTransaction)transaction))
                {
                    cmd.Parameters.Add(new SQLiteParameter("@sequenceNumber", ledgerCloseData.SequenceNumber));
                    cmd.Parameters.Add(new SQLiteParameter("@ledgerHash", ledgerCloseData.LedgerHash));
                    cmd.Parameters.Add(new SQLiteParameter("@transactions", ledgerCloseData.Transactions));
                    cmd.Parameters.Add(new SQLiteParameter("@totalTransactions", ledgerCloseData.TotalTransactions));
                    cmd.Parameters.Add(new SQLiteParameter("@closeTime", ledgerCloseData.CloseTime));

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

                using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO LedgerInfo VALUES(@sequenceNumber, @ledgerHash, @transactions, @totalTransactions, @closeTime);",
                    sqliteConnection, (SQLiteTransaction)transaction))
                {
                    cmd.Parameters.Add(new SQLiteParameter("@sequenceNumber", ledgerCloseData.SequenceNumber));
                    cmd.Parameters.Add(new SQLiteParameter("@ledgerHash", ledgerCloseData.LedgerHash));
                    cmd.Parameters.Add(new SQLiteParameter("@transactions", ledgerCloseData.Transactions));
                    cmd.Parameters.Add(new SQLiteParameter("@totalTransactions", ledgerCloseData.TotalTransactions));
                    cmd.Parameters.Add(new SQLiteParameter("@closeTime", ledgerCloseData.CloseTime));

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
                if (!DBUtils.TableExists("LedgerInfo", sqliteConnection))
                {
                    DBUtils.ExecuteNonQuery("CREATE TABLE LedgerInfo (SequenceNumber INTEGER PRIMARY KEY AUTOINCREMENT, LedgerHash BLOB, Transactions INTEGER, TotalTransactions INTEGER, CloseTime INTEGER);", sqliteConnection);
                }
            }
        }

        public DBResponse Delete(Hash ledgerHash)
        {
            DBResponse response = DBResponse.DeleteFailed;

            using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM LedgerInfo WHERE (LedgerHash = @ledgerHash);", sqliteConnection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@ledgerHash", ledgerHash.Hex));

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

            using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM LedgerInfo;", sqliteConnection))
            {
                removed = cmd.ExecuteNonQuery();
                if (removed > 0) // There should be at-least single entry for a PublicKey.
                {
                    response = DBResponse.DeleteSuccess;
                }
            }

            return new Tuple<DBResponse, long>(response, removed);
        }


        public bool GetLastRowData(out LedgerCloseData lastCloseData)
        {
            bool _found = false;
            lastCloseData = new LedgerCloseData();

            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM LedgerInfo ORDER BY SequenceNumber DESC LIMIT 1;", sqliteConnection))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            lastCloseData.SequenceNumber = (long)reader[0];
                            lastCloseData.LedgerHash = (byte[])reader[1];
                            lastCloseData.Transactions = (long)reader[2];
                            lastCloseData.TotalTransactions = (long)reader[3];
                            lastCloseData.CloseTime = (long)reader[4];
                            _found = true;
                        }
                    }
                }
            }

            return _found;
        }




    }
}
