using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Json.REST;

namespace TNetD
{
    public static class Common
    {
        public static readonly int LEN_TRANSACTION_ID = 32;
        public static readonly int KEYLEN_PUBLIC = 32;
        public static readonly int KEYLEN_PRIVATE = 32;
        public static readonly int KEYLEN_SIGNATURE = 64;


        public static readonly int NETWORK_Min_Transaction_Fee = 100000; // ~ 0.1 US Cent, approx, initial value.
        
        public static JsonSerializerSettings jss = new JsonSerializerSettings();
        
        /// <summary>
        ///  TODO: BAD/ REMOVE STATIC METHOD
        /// </summary>
        public static void Initialize()
        {
            jss.Converters.Add(new BytesToHexConverter());
            jss.Formatting = Formatting.Indented;
        } 

    }


}
