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

        public static readonly int TransactionStaleTimer_Minutes = 20; // Deviation of 1 minute. // CRITICAL FIX

        public static readonly int TransactionStatus_Persist_Seconds = 60;
        
        public static readonly int NETWORK_Min_Transaction_Fee = 0;//100000; // ~ 0.1 US Cent, approx, initial value.

        public static readonly int NETWORK_Min_Transaction_Value_SrcDest = 1;
        public static readonly int NETWORK_Min_Transaction_TotalValue = 1;

        public static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings();
        
        public static readonly int UI_TextBox_Max_Length = 20000;
        
        public static readonly NetworkType NetworkType = Address.NetworkType.MainNet;
        
        public static readonly string RpcHost =  "+" ; //"localhost";
        
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
