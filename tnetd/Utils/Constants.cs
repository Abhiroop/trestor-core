﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TNetD.Json.REST;

namespace TNetD
{
    static class Constants
    {        
        /// <summary>
        /// Depth at which hash tree leaves are stored.
        /// Its highly critical to tree hash generation and synchronisation.
        /// Cannot be changed once the network starts working.
        /// </summary>
        public static readonly int HashTree_NodeListDepth = 8;
               
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

        /// <summary>
        ///  Minimum 10 trest balance.
        ///  [CRITICAL: FIX ME !!!]
        /// </summary>
        public static readonly long FIN_MIN_BALANCE = 10000; //10000000;

        public static readonly long DB_HISTORY_LIMIT = 1000;
        public static readonly long DB_HISTORY_TX_LIMIT = 100000; // TODO: SET TO GOOD LIMITS
       
        public static readonly int VALIDATOR_COUNT = 5;
        public static readonly int SIM_REFRESH_MS = 50;
        public static readonly int SIM_REFRESH_MS_SIM = 50;

        public static readonly int CONS_TRUSTED_VALIDATOR_THRESHOLD_PERC = 50;
        public static readonly int CONS_VOTING_ACCEPTANCE_THRESHOLD_PERC = 75;

        public static readonly int SYNC_LEAF_COUNT_THRESHOLD = 200;

        /// <summary>
        /// Number of Tre's per Genesis Account. Currently equivalent to 100,000 USD (1 T = 1 US Cent).
        /// </summary>
        public static readonly long FIN_TRE_PER_GENESIS_ACCOUNT = 1000000000000000;

        ///////////////////////////////////////////////////////   1000000000000000 = 100 Billion * [10^6] / 100 Genesis Accounts

        public static readonly int Network_UpdateFrequencyMS = 100;
        public static readonly int Network_DefaultListenPort = 2015;

        public static readonly Verbosity NetworkVerbosity = Verbosity.ExtraInfo;

        public static bool ApplicationRunning = true;

        public static int PREFS_APP_TCP_BUFFER_SIZE = 4096;// 512 * 1024;

        public static long PREFS_MAX_RPC_POST_CONTENT_LENGTH = 50*1024; // 50 KiB 
                
        public static byte TransportVersion = 1;
        public static byte ProtocolVersion = 1;

        public static int Difficulty = 8; // 17;
              
        // ////////////////////// Counters ////////////////

        public static int GlobalReceivedPackets = 0;
        public static int GlobalReceivedBytes = 0;
        public static int GlobalSentPackets = 0;
        public static int GlobalSentBytes = 0;
        

        public static long SERVER_GLOBAL_PACKETS = 0;
        public static long SERVER_GLOBAL_DATA_PACKETS = 0;
        public static long SERVER_GLOBAL_INVALID_PACKETS = 0;

        public static long SERVER_GLOBAL_AUTH_PACKETS = 0;
        public static long SERVER_GLOBAL_AUTH_USERS = 0;
        public static long SERVER_GLOBAL_CONNS = 0;
        public static long SERVER_GLOBAL_TOTAL_ACC_CONNS = 0;
        public static long SERVER_GLOBAL_CHATS_PER_SEC = 0;

        // /////////////////// Node Data //////////////////////

        /// <summary>
        /// Global Node event update rate.
        /// </summary>
        public static readonly int Node_UpdateFrequencyMS = 100;

        /// <summary>
        /// Timer Update rate for consensus.
        /// </summary>
        public static readonly int Node_UpdateFrequencyConsensusMS = 4000;

        /// <summary>
        /// Update rate for handling packets.
        /// </summary>
        public static readonly int Node_UpdateFrequencyPacketProcessMS = 500;

        public static readonly int Node_UpdateFrequencyLedgerSyncMS = 100;
        public static readonly int Node_UpdateFrequencyLedgerSyncMS_Root = 2000;

        public static readonly string File_TrustedNodes = "TrustedNodes.ini";
        
        /// <summary>
        /// Minimum Level to be displayed.
        /// </summary>
        public static readonly DisplayType DebugLevel = DisplayType.Info;
        
        /// <summary>
        /// Number of pending work proofs, before new entries are rejected.
        /// </summary>
        public static readonly int WorkProofQueueLength = 50000;
        
        
        
    }
}
