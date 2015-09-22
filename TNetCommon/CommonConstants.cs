
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
        public static Random NORMAL_RNG = new Random();
        public static RNGCryptoServiceProvider SECURE_RNG = new RNGCryptoServiceProvider();

        public static readonly Encoding Encoding28591 = Encoding.GetEncoding(28591);        

        public static readonly int LEN_TRANSACTION_ID = 32;
        public static readonly int KEYLEN_PUBLIC = 32;
        public static readonly int KEYLEN_PRIVATE = 32;
        public static readonly int KEYLEN_PRIVATE_EXPANDED = 64;
        public static readonly int KEYLEN_SIGNATURE = 64;

        // /////////////////////////////  CRITICAL SETTINGS  //////////////////////////////////

        public static readonly bool TRANSACTION_FEE_ENABLED = false;

        public static readonly int TRANSACTION_STALE_TIMER_MINUTES = 20; // Deviation of 1 minute. // Critical Fix
        
        public static readonly int NETWORK_MIN_TRANSACTION_FEE = 0; //100000; // ~ 0.1 US Cent, approx, initial value.

        public static readonly int NETWORK_TRANSACTION_SRCDEST_MIN_COUNT = 1; // Atleast a singel source and destination

        public static readonly int NETWORK_MIN_TRANSACTION_TOTAL_VALUE = 1; // Minimum transaction of 1 trest.

        // /////////////////////////////////////////////////////////////////////////////////////

        public static readonly int TRANSACTION_STATUS_PERSIST_SECONDS = 60;

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

        public static JsonSerializerSettings JSON_SERIALIZER_SETTINGS = new JsonSerializerSettings();

        public static readonly int UI_TextBox_Max_Length = 20000;

        public static readonly NetworkType NETWORK_TYPE = Address.NetworkType.TestNet;
        public static readonly NodeOperationType NODE_OPERATION_TYPE = NodeOperationType.Distributed;

        public static readonly string RPC_HOST = "localhost";// "localhost";

        public static readonly bool GLOBAL_VERBOSE_DEBUGGING_ENABLED = true;
        public static readonly bool GLOBAL_EXCEPTION_STACKTRACE_DISPLAY_ENABLED = true;

        /// <summary>
        /// Minimum allowable Account Name Length
        /// </summary>
        public static readonly int PREF_MIN_NAME_LENGTH = 2;

        /// <summary>
        /// Maximum allowable name length.
        /// </summary>
        public static readonly int PREF_MAX_NAME_LENGTH = 20;

        /// <summary>
        ///  TODO: BAD/ REMOVE STATIC METHOD
        /// </summary>
        public static void Initialize()
        {
            JSON_SERIALIZER_SETTINGS.Converters.Add(new BytesToHexConverter());
            JSON_SERIALIZER_SETTINGS.Formatting = Formatting.Indented;
        }

    }


}
