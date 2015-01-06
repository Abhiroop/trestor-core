﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Nodes;
using TNetD.Transactions;

namespace TNetD.PersistentStore
{
    class SQLiteTransactionStore //: ITransactionStore    
    {
        SQLiteConnection sqliteConnection = default(SQLiteConnection);

        public SQLiteTransactionStore(NodeConfig config)
        {
            sqliteConnection = new SQLiteConnection("Data Source=" + config.Path_TransactionDB + ";Version=3;");
            sqliteConnection.Open();

            VerifyTables();
        }      

        public bool Exists(Hash transactionID)
        {
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM Transactions WHERE TransactionID = @transactionID;", sqliteConnection);
            cmd.Parameters.Add(new SQLiteParameter("@transactionID", transactionID));
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                cmd.Dispose();
                return true;
            }
            cmd.Dispose();
            return false;
        }

        public DBResponse FetchTransaction(Hash transactionID, out TransactionContent transactionContent)
        {
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM Transactions WHERE TransactionID = @transactionID;", sqliteConnection);
            cmd.Parameters.Add(new SQLiteParameter("@transactionID", transactionID.Hex));
            SQLiteDataReader reader = cmd.ExecuteReader();

            DBResponse response = DBResponse.FetchFailed;

            transactionContent = default(TransactionContent);

            if (reader.HasRows)
            {
                if (reader.Read())
                {
                    Hash _transactionID = new Hash((byte[])reader[0]);
                    byte[] SerializedContent = (byte [])reader[1];
                    
                    if (_transactionID == transactionID) // Proper row returned.
                    {
                        transactionContent = new TransactionContent();
                        transactionContent.Deserialize(SerializedContent);

                        if(transactionContent.TransactionID == transactionID) // De-Serialization Suceeded.
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

            cmd.Dispose();

            return response;
        }

        public DBResponse AddUpdate(TransactionContent transactionContent)
        {

            SQLiteCommand cmd = new SQLiteCommand("SELECT TransactionID FROM Transactions WHERE TransactionID = @transactionID;", sqliteConnection);
            cmd.Parameters.Add(new SQLiteParameter("@transactionID", transactionContent.TransactionID.Hex));
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

                cmd = new SQLiteCommand("UPDATE Transactions SET SerializedContent = @serializedContent WHERE TransactionID = @transactionID;", sqliteConnection);

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
            }
            else
            {
                // /////////////  Perform the INSERT  ///////////////

                cmd = new SQLiteCommand("INSERT INTO Transactions VALUES(@transactionID, @serializedContent);", sqliteConnection);

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

            cmd.Dispose();

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
            }
        }

        /// <summary>
        /// Deletes the transaction from the Persistent Transaction Store
        /// As USUAL: ONLY FOR TESTING. MUST NOT BE USED IN PRACTICE.
        /// </summary>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        public DBResponse Delete(Hash transactionID)
        {
            SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Transactions WHERE (TransactionID = @transactionID);", sqliteConnection);
            cmd.Parameters.Add(new SQLiteParameter("@transactionID", transactionID.Hex));
            DBResponse response = DBResponse.DeleteFailed;

            // There should be a single entry for a TransactionID.
            if (cmd.ExecuteNonQuery() == 1) 
            {
                response = DBResponse.DeleteSuccess;
            }

            cmd.Dispose();
            return response;
        }

    }

}
