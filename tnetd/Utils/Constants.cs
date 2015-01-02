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
        
        /// <summary>
        /// Depth at which hash tree leaves are stored.
        /// Its highly critical to tree hash generation and synchronisation.
        /// Cannot be changed once the network starts working.
        /// </summary>
        public static readonly int HashTree_NodeListDepth = 4;

        public static readonly int GlobalNodes = 100;

        public static readonly int Connections_PerNode_Max = 20;
        public static readonly int Connections_PerNode_Min = 5;

        /// <summary>
        /// This value is the minimum percentage of positive packets needed for a transaction.
        /// </summary>
        public static readonly int Consensus_MinLimit = 90;

        /// <summary>
        /// This is the percentage at which the node begins to vote for a packet.
        /// </summary>
        public static readonly int Consensus_BeginForwarding = 50;


        public static readonly int PacketLimitPerSecond = 10;
        public static readonly int DataLimitPerSecond = 512 * 1024; // Bytes per second.

        //public static List<Node> GlobalNodeList = new List<Node>();
     
        /// ////////////////////////
       
        public static readonly long FIN_MIN_BALANCE = 1500;
        public static readonly int VALIDATOR_COUNT = 5;
        public static readonly int SIM_REFRESH_MS = 50;
        public static readonly int SIM_REFRESH_MS_SIM = 50;

        public static readonly int CONS_TRUSTED_VALIDATOR_THRESHOLD_PERC = 50;
        public static readonly int CONS_VOTING_ACCEPTANCE_THRESHOLD_PERC = 75;

        public static readonly int SYNC_LEAF_COUNT_THRESHOLD = 200;

        ///////////////////////////

        public static readonly int Network_UpdateFrequencyMS = 100;
        public static readonly int Network_DefaultListenPort = 2014;

        public static readonly Verbosity NetworkVerbosity = Verbosity.ExtraInfo;

        public static bool ApplicationRunning = true;

        public static int PREFS_APP_TCP_BUFFER_SIZE = 4096;// 512 * 1024;
                
        public static byte TransportVersion = 1;
        public static byte ProtocolVersion = 1;

        public static int Difficulty = 0; // 17;
              
        // ////////////////////// Counters ////////////////

        public static int GlobalReceivedPackets = 0;
        public static int GlobalReceivedBytes = 0;
        public static int GlobalSentPackets = 0;
        public static int GlobalSentBytes = 0;
        
        /////////////////   Server Constants

        internal static byte[] Auth_NodePublic = HexUtil.GetBytes("55C28B6F43C8CEAA905546843F803581D39F95B0227C5AED1704B2A8A592E827");

        /// <summary>
        /// Server Private !! Keep this secret at all cost.
        /// </summary>
        internal static byte[] Auth_NodePrivateExpanded = HexUtil.GetBytes("95BDC35192B65B3E8873F9A0E13185AE255059AE32405C9488766E523206B73055C28B6F43C8CEAA905546843F803581D39F95B0227C5AED1704B2A8A592E827");

        /// <summary>
        /// Server Private !! Keep this secret at all cost.
        /// </summary>
        internal static byte[] Auth_NodeRandom_Private = HexUtil.GetBytes("95BDC35192B65B3E8873F9A0E13185AE255059AE32405C9488766E523206B730");

        //////////////////////////

        public static long SERVER_GLOBAL_PACKETS = 0;
        public static long SERVER_GLOBAL_DATA_PACKETS = 0;
        public static long SERVER_GLOBAL_INVALID_PACKETS = 0;

        public static long SERVER_GLOBAL_AUTH_PACKETS = 0;
        public static long SERVER_GLOBAL_AUTH_USERS = 0;
        public static long SERVER_GLOBAL_CONNS = 0;
        public static long SERVER_GLOBAL_TOTAL_ACC_CONNS = 0;
        public static long SERVER_GLOBAL_CHATS_PER_SEC = 0;

        // /////////////////// Node Data //////////////////////

        public static readonly int Node_UpdateFrequencyMS = 100;

        public static readonly string File_TrustedNodes = "TrustedNodes.ini";

        
    }
}
