using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TNetD
{
    static class Constants
    {
        public static  RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

        public static Random random = new Random();

        public static int GlobalNodes = 100;

        public static int Connections_PerNode_Max = 20;
        public static int Connections_PerNode_Min = 5;

        /// <summary>
        /// This value is the minimum percentage of positive packets needed for a transaction.
        /// </summary>
        public static int Consensus_MinLimit = 90;

        /// <summary>
        /// This is the percentage at which the node begins to vote for a packet.
        /// </summary>
        public static int Consensus_BeginForwarding = 50;


        public static int PacketLimitPerSecond = 10;
        public static int DataLimitPerSecond = 512 * 1024; // Bytes per second.

        public static List<Node> GlobalNodeList = new List<Node>();
        
        



    }
}
