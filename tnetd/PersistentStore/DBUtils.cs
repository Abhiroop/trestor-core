/*
 @Author: Arpan Jati
 @Date: Jan 2015
 */

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.PersistentStore
{
    public enum DBResponse { InsertSuccess, UpdateSuccess, DeleteSuccess, InsertFailed, UpdateFailed, 
        DeleteFailed, FetchSuccess, FetchFailed, NonDBError, Exception, NothingDone, NoRowsReturned }
    
    class DBUtils
    {
        public static bool TableExists(string tableName, SQLiteConnection sqliteConnection)
        {
            bool exists = false;
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name=@name", sqliteConnection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@name", tableName));
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        exists = true;
                    }
                }
            }
            return exists;
        }

        public static int ExecuteNonQuery(string Query, SQLiteConnection sqliteConnection)
        {
            int reply = -1;
            using (SQLiteCommand cmd = new SQLiteCommand(Query, sqliteConnection))
            {
                reply = cmd.ExecuteNonQuery();
            }
            return reply;
        }
    }
}
