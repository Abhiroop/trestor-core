using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetWallet
{
    static class Constants
    {
        /// <summary>
        /// Connection string for the Local SQLITE database.
        /// </summary>
        public static string ConnectionString = @"data source=..\..\db\db.dat; Version=3; New=True; Compress=True;";

        /// <summary>
        /// Wait ReconnectInterval milliseconds before re-attempting connection.
        /// </summary>
        public static int ReconnectInterval = 15000;

    }
}
