
// @Author : Arpan Jati
// @Date: Dec 2014

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TNetD.Address;
using TNetD.Json.REST;

namespace TNetD
{
    public enum NodeOperationType { Centralized, Distributed } 

    public static class Common
    {
        public static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

        public static readonly Encoding Encoding28591 = Encoding.GetEncoding(28591);

        public static Random random = new Random();

        public static readonly int LEN_TRANSACTION_ID = 32;
        public static readonly int KEYLEN_PUBLIC = 32;
        public static readonly int KEYLEN_PRIVATE = 32;
        public static readonly int KEYLEN_PRIVATE_EXPANDED = 64;
        public static readonly int KEYLEN_SIGNATURE = 64;

        public static readonly bool IsTransactionFeeEnabled = false;

        public static readonly int TransactionStaleTimer_Minutes = 20; // Deviation of 1 minute. // Critical Fix

        public static readonly int TransactionStatus_Persist_Seconds = 60;

        public static readonly int NETWORK_Min_Transaction_Fee = 0; //100000; // ~ 0.1 US Cent, approx, initial value.

        public static readonly int NETWORK_Min_Transaction_Value_SrcDest = 1;
        public static readonly int NETWORK_Min_Transaction_TotalValue = 1;

        /// <summary>
        /// Token length in bytes (8 => 2^64 possibilities)
        /// </summary>
        public static readonly int NETWORK_TOKEN_LENGTH = 8;

        public static readonly long LSYNC_MAX_PENDING_QUEUE_LENGTH = 2048; 
        public static readonly long LSYNC_MAX_ORDERED_NODES = 64;
        public static readonly long LSYNC_MAX_ORDERED_LEAVES = 1024;
        public static readonly long LSYNC_MAX_LEAVES_TO_FETCH = 64; // 4096 

        /// <summary>
        /// This is for requests over the network
        /// </summary>
        public static readonly long LSYNC_MAX_REQUESTED_NODES = 256;

        public static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings();

        public static readonly int UI_TextBox_Max_Length = 20000;

        public static readonly NetworkType NetworkType = Address.NetworkType.TestNet;
        public static readonly NodeOperationType NodeOperationType = NodeOperationType.Distributed;

        public static readonly string RpcHost = "localhost";// "localhost";

        // VOTING CONSTANTS

        public static readonly int VOTE_MIN_VOTERS = 5;

        /// <summary>
        /// 50%. If a transaction is voted for by more than this percentage of voters,
        /// but, we dont have it, fetch it.
        /// This should be a rare case and the previous merge stage should have distributed the
        /// transactions properly.
        /// </summary>
        public static readonly int VOTE_VOTE_STAGE_FETCH_THRESHOLD_PERC = 50;

        /// <summary>
        /// Minimum allowable Account Name Length
        /// </summary>
        public static readonly int Pref_MinNameLength = 2;

        /// <summary>
        /// Maximum allowable name length.
        /// </summary>
        public static readonly int Pref_MaxNameLength = 20;

        /// <summary>
        ///  TODO: BAD/ REMOVE STATIC METHOD
        /// </summary>
        public static void Initialize()
        {
            JsonSerializerSettings.Converters.Add(new BytesToHexConverter());
            JsonSerializerSettings.Formatting = Formatting.Indented;
        }

    }


}
