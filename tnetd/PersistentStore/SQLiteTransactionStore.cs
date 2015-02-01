
// @Author: Arpan Jati
// @Date: 6-7 Jan / 2015 | 15 Jan 2015
// 31 Jan 2015 : Oops !! Added Transactions

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Nodes;
using TNetD.Transactions;

namespace TNetD.PersistentStore
{
    class SQLiteTransactionStore : IPersistentTransactionStore
    {
        SQLiteConnection sqliteConnection = default(SQLiteConnection);

        public SQLiteTransactionStore(NodeConfig config)
        {
            sqliteConnection = new SQLiteConnection("Data Source=" + config.Path_TransactionDB + ";Version=3;");
            sqliteConnection.Open();

            VerifyTables();
        }

        /// <summary>
        /// Returns true if a transaction exists in the persistent database.
        /// </summary>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        public bool Exists(Hash transactionID)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM Transactions WHERE TransactionID = @transactionID;", sqliteConnection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@transactionID", transactionID));
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public DBResponse FetchTransaction(out TransactionContent transactionContent, Hash transactionID)
        {
            DBResponse response = DBResponse.FetchFailed;

            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM Transactions WHERE TransactionID = @transactionID;", sqliteConnection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@transactionID", transactionID.Hex));
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    transactionContent = default(TransactionContent);

                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            Hash _transactionID = new Hash((byte[])reader[0]);
                            byte[] SerializedContent = (byte[])reader[1];

                            if (_transactionID == transactionID) // Proper row returned.
                            {
                                transactionContent = new TransactionContent();
                                transactionContent.Deserialize(SerializedContent);

                                if (transactionContent.TransactionID == transactionID) // De-Serialization Suceeded.
                                {
                                    response = DBResponse.FetchSuccess;
                                }
                                else
                                {
                                    response = DBResponse.NonDBError;
                                }
                            }
                        }
                    }

                }
            }

            return response;
        }

        // FEATURE: PUT OFFSET SUPPORT FOR LARGE HISTORY SYNC

        /// <summary>
        /// Fetches transaction history from the database. 
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="publicKey">Public Key of Account</param>
        /// <param name="TimeStamp">The time after which tranactions are needed</param>
        /// <param name="Limit">Max result count, 0 means all (Bounded by system limit.)</param>
        /// <returns></returns>
        public DBResponse FetchTransactionHistory(out List<TransactionContent> transactions, Hash publicKey, long timeStamp, int Limit)
        {
            DBResponse response = DBResponse.Exception;

            string LIMIT_CLAUSE = "";

            if (Limit > 0 && Limit < Constants.DB_HISTORY_LIMIT)
            {
                LIMIT_CLAUSE = "LIMIT " + Limit;
            }
            else
            {
                LIMIT_CLAUSE = "LIMIT " + Constants.DB_HISTORY_LIMIT;
            }

            // TransactionHistory (TransactionID BLOB, PublicKey BLOB, TimeStamp Integer)

            using (SQLiteCommand cmd = new SQLiteCommand("SELECT TransactionID FROM TransactionHistory WHERE (PublicKey = @publicKey AND TimeStamp >= @timeStamp) ORDER BY TimeStamp DESC " + LIMIT_CLAUSE + ";", sqliteConnection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@publicKey", publicKey.Hex));
                cmd.Parameters.Add(new SQLiteParameter("@timeStamp", timeStamp));

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    transactions = new List<TransactionContent>();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Hash transactionID = new Hash((byte[])reader[0]);

                            TransactionContent transactionContent;
                            if (FetchTransaction(out transactionContent, transactionID) == DBResponse.FetchSuccess)
                            {
                                transactions.Add(transactionContent);
                            }
                        }
                    }

                }
            }

            return response;
        }

        //CRITICAL: HANDLE CASE WHEN ENTRY ALREADY EXISTS GRACEFULLY

        public int AddUpdateBatch(Dictionary<Hash, TransactionContent> accountInfoData)
        {
            int Successes = 0;
            SQLiteTransaction st = sqliteConnection.BeginTransaction();

            foreach (KeyValuePair<Hash, TransactionContent> kvp in accountInfoData)
            {
                DBResponse resp = AddUpdate(kvp.Value, st);
                if ((resp == DBResponse.InsertSuccess) || (resp == DBResponse.UpdateSuccess))
                {
                    Successes++;
                }
            }

            st.Commit();
                        
            return Successes;
        }

        /// <summary>
        /// Add a transaction to the transaction store. 
        /// Currently no method to revoke transactions.
        /// Do nor call directly in a loop, use BeginTransaction / Commit.
        /// </summary>
        /// <param name="transactionContent"></param>
        /// <returns></returns>
        public DBResponse AddUpdate(TransactionContent transactionContent, SQLiteTransaction transaction)
        {
            bool doUpdate = false;

            using (SQLiteCommand cmd = new SQLiteCommand("SELECT TransactionID FROM Transactions WHERE TransactionID = @transactionID;", sqliteConnection, transaction))
            {
                cmd.Parameters.Add(new SQLiteParameter("@transactionID", transactionContent.TransactionID.Hex));
                SQLiteDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    doUpdate = true; // Perform update as the entry already exists.
                }
            }

            DBResponse response = DBResponse.Exception;

            if (doUpdate)
            {
                // /////////////  Perform the UPDATE  ///////////////

                // WELL ITS IMPLEMENTED BUT NOT NEEDED.
                // Not emplemet because of the unnecessary added complexity of handling the history table. 

                throw new NotImplementedException("Unsupported right now. Technically not needed. Transactions need to be only added and fetched.");

                /* using (SQLiteCommand cmd = new SQLiteCommand("UPDATE Transactions SET SerializedContent = @serializedContent WHERE TransactionID = @transactionID;", sqliteConnection))
                 {
                     cmd.Parameters.Add(new SQLiteParameter("@transactionID", transactionContent.TransactionID.Hex));
                     cmd.Parameters.Add(new SQLiteParameter("@serializedContent", transactionContent.Serialize()));

                     if (cmd.ExecuteNonQuery() != 1)
                     {
                         response = DBResponse.UpdateFailed;
                     }
                     else
                     {
                         response = DBResponse.UpdateSuccess;
                     }
                 }*/
            }
            else
            {
                ///////////////  Perform the INSERT  ///////////////

                using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO Transactions VALUES(@transactionID, @serializedContent);", sqliteConnection))
                {
                    cmd.Parameters.Add(new SQLiteParameter("@transactionID", transactionContent.TransactionID.Hex));
                    cmd.Parameters.Add(new SQLiteParameter("@serializedContent", transactionContent.Serialize()));

                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        response = DBResponse.InsertFailed;
                    }
                    else
                    {
                        response = DBResponse.InsertSuccess;
                    }
                }

                if (response == DBResponse.InsertSuccess)
                {
                    // Add to the History Table

                    foreach (TransactionEntity entity in transactionContent.Sources)
                    {
                        InsertToHistoryTable(transactionContent.TransactionID, transactionContent.Timestamp, entity, transaction);
                    }

                    foreach (TransactionEntity entity in transactionContent.Destinations)
                    {
                        InsertToHistoryTable(transactionContent.TransactionID, transactionContent.Timestamp, entity, transaction);
                    }
                }
            }

            return response;
        }

        private DBResponse InsertToHistoryTable(Hash transactionID, long timeStamp, TransactionEntity entity, SQLiteTransaction transaction)
        {
            DBResponse response = DBResponse.Exception;

            using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO TransactionHistory VALUES(@transactionID, @publicKey, @timeStamp);", sqliteConnection, transaction))
            {
                cmd.Parameters.Add(new SQLiteParameter("@transactionID", transactionID.Hex));
                cmd.Parameters.Add(new SQLiteParameter("@publicKey", entity.PublicKey));
                cmd.Parameters.Add(new SQLiteParameter("@timeStamp", timeStamp));

                if (cmd.ExecuteNonQuery() != 1)
                {
                    response = DBResponse.InsertFailed;
                }
                else
                {
                    response = DBResponse.InsertSuccess;
                }
            }

            return response;
        }

        /// <summary>
        /// Verify if the Transaction table exists. Recreate if it does not exist.
        /// </summary>
        private void VerifyTables()
        {
            if (sqliteConnection.State == System.Data.ConnectionState.Open)
            {
                if (!DBUtils.TableExists("Transactions", sqliteConnection))
                {
                    DBUtils.ExecuteNonQuery("CREATE TABLE Transactions (TransactionID BLOB PRIMARY KEY, SerializedContent BLOB);", sqliteConnection);
                }

                ///  Extra Table for Transaction History : OPTIONAL FOR NODES
                if (!DBUtils.TableExists("TransactionHistory", sqliteConnection))
                {
                    DBUtils.ExecuteNonQuery("CREATE TABLE TransactionHistory (TransactionID BLOB, PublicKey BLOB, TimeStamp Integer);", sqliteConnection);
                    // Make an index table to make history lookups much faster.
                    DBUtils.ExecuteNonQuery("CREATE INDEX Idx1 ON TransactionHistory(PublicKey, TimeStamp);", sqliteConnection);
                }
            }
        }

        /// <summary>
        /// Deletes the transaction from the Persistent Transaction Store
        /// AS USUAL: ONLY FOR TESTING. MUST NOT BE USED IN PRACTICE.
        /// </summary>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        public DBResponse Delete(Hash transactionID)
        {
            DBResponse response = DBResponse.DeleteFailed;
            using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Transactions WHERE (TransactionID = @transactionID);", sqliteConnection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@transactionID", transactionID.Hex));

                // There should be a single entry for a TransactionID.
                if (cmd.ExecuteNonQuery() == 1)
                {
                    response = DBResponse.DeleteSuccess;
                }
            }
            return response;
        }

        /// <summary>
        /// Deletes everything in the DB. Returns 'DeleteFailed' if already empty.
        /// ONLY FOR TEST. DO NOT USE.
        /// </summary>
        /// <returns></returns>
        public Tuple<DBResponse, long> DeleteEverything()
        {
            DBResponse response = DBResponse.DeleteFailed;
            int removed = 0;

            using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Transactions;", sqliteConnection))
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
