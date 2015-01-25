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
    public static class Common
    {
        public static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

        public static Random random = new Random();

        public static readonly int LEN_TRANSACTION_ID = 32;
        public static readonly int KEYLEN_PUBLIC = 32;
        public static readonly int KEYLEN_PRIVATE = 32;
        public static readonly int KEYLEN_PRIVATE_EXPANDED = 64;
        public static readonly int KEYLEN_SIGNATURE = 64;
        
        public static readonly int NETWORK_Min_Transaction_Fee = 0;//100000; // ~ 0.1 US Cent, approx, initial value.
        
        public static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings();
        
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
