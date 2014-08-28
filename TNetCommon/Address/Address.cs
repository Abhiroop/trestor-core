using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace TNetCommon.Address
{
    class Address
    {
        byte[] PublicKey;
        string UserName;
        int NetworkType = 1;

        public byte[] GetAddress
        {
            get
            {
                byte[] joined = PublicKey.Concat(Encoding.GetEncoding(28591).GetBytes(UserName)).ToArray();


                return (new SHA512Managed()).ComputeHash(joined).Take(32).ToArray();
            }
        }

        public Address(byte[] PublicKey, string UserName, int NetworkType=1)
        {
            this.PublicKey = PublicKey;
            this.UserName = UserName;
            this.NetworkType = NetworkType;
        }
    }
}
