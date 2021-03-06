﻿
using System;
using System.Collections.Generic;
using TNetD.Address;
using TNetD.Ledgers;
using TNetD.Transactions;

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

        public static readonly int PacketLimitPerSecond = 10;
        public static readonly int DataLimitPerSecond = 512 * 1024; // Bytes per second.

        /// <summary>
        ///  Minimum 10 trest balance.
        ///  [CRITICAL: FIX ME !!!]
        /// </summary>
        public static readonly long FIN_MIN_BALANCE = 50; // 10000000; [FIXME FIXME FIXME FIXME FIXME FIXME FIXME FIXME FIXME FIXME FIXME]

        /// <summary>
        /// Maximum amount of TRE's per transaction entity. 20,00,00,000 Trest
        /// </summary>
        public static readonly long FIN_MAX_TRE_PER_TX = 200000000000000;

        /// <summary>
        /// Maximum amount of TRE's per transaction entity. 5,00,00,000 Trest
        /// </summary>
        public static readonly long FIN_MAX_TRE_PER_TX_ENTITY = 50000000000000;


        public static readonly long DB_HISTORY_LIMIT = 1000;
        public static readonly long DB_HISTORY_TX_LIMIT = 100000; // TODO: SET TO GOOD LIMITS

        public static readonly int VALIDATOR_COUNT = 5;
        public static readonly int SIM_REFRESH_MS = 50;
        public static readonly int SIM_REFRESH_MS_SIM = 50;

        // SYNC 

        public static readonly int VOTE_MIN_SYNC_NODES = 3;


        // VOTING CONSTANTS

        /// <summary>
        /// Minimim number of voters needed excluding us. Example: for the 5 node system, the value should be 4.
        /// </summary>
        public static readonly int VOTE_MIN_VOTERS = 4;

        /// <summary>
        /// 50%. If a transaction is voted for by more than this percentage of voters,
        /// but, we dont have it, fetch it.
        /// This should be a rare case and the previous merge stage should have distributed the
        /// transactions properly.
        /// </summary>
        public static readonly int CONS_VOTE_STAGE_FETCH_THRESHOLD_PERC = 50;

        /// <summary>
        /// 80%. This is the percentage of voters, who should agree to the set to be accepted in the ledger.
        /// </summary>
        public static readonly int CONS_FINAL_VOTING_THRESHOLD_PERC = 80;

        /// <summary>
        /// Number of Tre's per Genesis Account. Currently equivalent to 100,000 USD (1 T = 1 US Cent).
        /// </summary>
        public static readonly long FIN_TRE_PER_GENESIS_ACCOUNT = 1000000000000000;

        ///////////////////////////////////////////////////////   1000000000000000 = 100 Billion * [10^6] / 100 Genesis Accounts

        public static readonly int LATENCY_MAX_ELEMENTS = 10;

        public static readonly int Network_UpdateFrequencyMS = 80;
        public static readonly int Network_ConnectionUpdateFrequencyMS = 2000;
        public static readonly int Network_DefaultListenPort = 2015;

        public static readonly Verbosity NetworkVerbosity = Verbosity.ExtraInfo;

        public static bool ApplicationRunning = true;

        public static int PREFS_APP_TCP_BUFFER_SIZE = 4096; // 512 * 1024;

        public static long PREFS_MAX_RPC_POST_CONTENT_LENGTH = 50 * 1024; // 50 KiB 

        public static byte TransportVersion = 1;
        public static byte ProtocolVersion = 1;

        public static int Difficulty = 15;

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


        /// <summary>
        /// Minimum Level to be displayed.
        /// </summary>
        public static readonly DisplayType DebugLevel = DisplayType.Debug;

        /// <summary>
        /// Number of pending work proofs, before new entries are rejected.
        /// </summary>
        public static readonly int WorkProofQueueLength = 50000;


        public static List<AccountInfo> GetGenesisData()
        {
            List<AccountInfo> aiData = new List<AccountInfo>();

            string[] Accs = Common.NETWORK_TYPE == NetworkType.MainNet ? GenesisRawData.MainNet : GenesisRawData.TestNet;

            foreach (string acc in Accs)
            {
                AccountIdentifier AI = new AccountIdentifier();
                AI.Deserialize(Convert.FromBase64String(acc));

                AccountInfo ai = new AccountInfo(new Hash(AI.PublicKey), Constants.FIN_TRE_PER_GENESIS_ACCOUNT);

                ai.NetworkType = AI.AddressData.NetworkType;
                ai.AccountType = AI.AddressData.AccountType;

                ai.AccountState = AccountState.Normal;
                ai.LastTransactionTime = 0;
                ai.Name = AI.Name;

                aiData.Add(ai);
            }

            return aiData;
        }



    }
}
