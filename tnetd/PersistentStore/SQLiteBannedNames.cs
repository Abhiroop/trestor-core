
//
// @Author: Arpan Jati
// @Date: 1-2-3-6 Jan 2015
// 15 Jan 2015 : Adding : NetworkType / AccountType
// 22 Jan 2015 : IPersistentAccountStore.BatchFetch
// 31 Jan 2015 : Oops !! Added Transactions

// TODO: ASYNC/AWAIT

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
    /// <summary>
    /// Stores account nodes in an SQLite database.
    /// </summary>
    class SQLiteBannedNames
    {
        SQLiteConnection sqliteConnection = default(SQLiteConnection);

        public SQLiteBannedNames(NodeConfig config)
        {
            sqliteConnection = new SQLiteConnection("Data Source=" + config.Path_BannedNamesDB + ";Version=3;");
            sqliteConnection.Open();

            VerifyTables();
        }

        ////////////////////////
        /// <summary>
        /// Contains no input validations. Make sure input is good.
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public bool Contains(string UserName)
        {
            bool Found = false;
            //accountInfo = default(AccountInfo);

            //if ((UserName.Length >= Constants.Pref_MinNameLength) && (UserName.Length <= Constants.Pref_MaxNameLength))
            {
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM BannedNames WHERE Name = @name;", sqliteConnection))
                {
                    cmd.Parameters.Add(new SQLiteParameter("@name", UserName));
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            Found = true;
                        }
                    }
                }
            }

            return Found;
        }

        //////////////////////////////////////////

        public int AddUpdateBatch(IEnumerable<string> names)
        {
            int Successes = 0;
            SQLiteTransaction st = sqliteConnection.BeginTransaction();

            foreach (string name in names)
            {
                DBResponse resp = AddUpdate(name, st);
                if ((resp == DBResponse.InsertSuccess) || (resp == DBResponse.UpdateSuccess))
                {
                    Successes++;
                }
            }

            st.Commit();
            return Successes;
        }

        // //////////////////////////////////////

        public DBResponse AddUpdate(string name)
        {
            SQLiteTransaction st = sqliteConnection.BeginTransaction();
            DBResponse resp = AddUpdate(name, st);
            st.Commit();
            return resp;
        }

        public DBResponse AddUpdate(string name, DbTransaction transaction)
        {
            bool doUpdate = false;

            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM BannedNames WHERE Name = @name;",
                sqliteConnection, (SQLiteTransaction)transaction))
            {
                cmd.Parameters.Add(new SQLiteParameter("@name", name));
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

                //throw new Exception("UPDATE NOT ALLOWED");

            }
            else
            {
                // /////////////  Perform the INSERT  ///////////////

                using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO BannedNames VALUES(@name);",
                    sqliteConnection, (SQLiteTransaction)transaction))
                {
                    cmd.Parameters.Add(new SQLiteParameter("@name", name));

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
                if (!DBUtils.TableExists("BannedNames", sqliteConnection))
                {
                    DBUtils.ExecuteNonQuery("CREATE TABLE BannedNames (Name BLOB PRIMARY KEY);", sqliteConnection);
                }
            }
        }

        public DBResponse Delete(string name)
        {
            DBResponse response = DBResponse.DeleteFailed;

            using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM BannedNames WHERE (Name = @name);", sqliteConnection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@name", name));

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

            using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM BannedNames;", sqliteConnection))
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
