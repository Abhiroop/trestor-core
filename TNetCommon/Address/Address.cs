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
        string Username;

        public byte[] GetAddress
        {
            get
            {
                byte[] joined = PublicKey.Concat(Encoding.GetEncoding(28591).GetBytes(Username)).ToArray();
                return (new SHA512Managed()).ComputeHash(joined).Take(32).ToArray();
            }
        }

        public Address(byte[] PublicKey, string Username)
        {
            this.PublicKey = PublicKey;
            this.Username = Username;
        }
    }
}
