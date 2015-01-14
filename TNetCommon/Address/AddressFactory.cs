
/*
 *  Arpan Jati
 *  Original Sept 2014 | Convert to use Base58, 13th Jan 2015
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Diagnostics.Contracts;

namespace TNetD.Address
{

    /// <summary>
    /// Generates addresses from PublicKey, UserName and NetworkType
    /// </summary>
    public static class AddressFactory
    {
        /// <summary>
        /// Returns the 
        /// H = SHA512, 
        /// Address Format : Address = NetType || AccountType || {[H(H(PK) || PK || NAME)], Take first 20 bytes}
        /// </summary>
        public static byte[] GetAddress(byte[] PublicKey, string UserName, byte NetworkType = (byte)15, byte AccountType = (byte)32)
        {
            //Contract.Requires<ArgumentException>(PublicKey != null);
            //Contract.Requires<ArgumentException>(UserName != null);
            //Contract.Requires<ArgumentException>(PublicKey.Length == 32, "Public key length must be 32 bytes.");
            //Contract.Requires<ArgumentException>(UserName.Length < 64, "Username length should be less than 64.");

            byte[] NAME = Utils.Encoding88591.GetBytes(UserName);

            byte[] Hpk = (new SHA512Managed()).ComputeHash(PublicKey);

            byte[] Hpk__PK__NAME = Hpk.Concat(PublicKey).Concat(NAME).ToArray();

            byte[] H_Hpk__PK__NAME = (new SHA512Managed()).ComputeHash(Hpk__PK__NAME).Take(20).ToArray();

            byte[] Address_PH = new byte[22];

            Address_PH[0] = NetworkType;
            Address_PH[1] = AccountType;
            Array.Copy(H_Hpk__PK__NAME, 0, Address_PH, 2, 20);

            /* byte[] CheckSum = (new SHA512Managed()).ComputeHash(Address_PH, 0, 22).Take(4).ToArray();
             Array.Copy(CheckSum, 0, Address_PH, 22, 4);*/

            return Address_PH;
        }

       /* private static string GetAddressString_Internal(byte[] Address)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((char)Address[0]);
            sb.Append((char)Address[1]);

            sb.Append(Convert.ToBase64String(Address.Skip(2).ToArray()));

            return sb.ToString();
        }*/

        public static string GetAddressString(byte[] Address )
        {
            if (Address.Length != 22)
            {
                throw new ArgumentException("Invalid Address Length. Must be 22 bytes");
            }
            else
            {
                return Base58Encoding.EncodeWithCheckSum(Address);
            }            
        }

        /*
        public static bool ValidateAddress(byte[] Address)
        {
            if (Address.Length != 26)
                return false;

            byte[] CheckSum = (new SHA512Managed()).ComputeHash(Address, 0, 22).Take(4).ToArray();

            return Utils.ByteArrayEquals(CheckSum, 0, Address, 22, 4);
        }*/
    }
}



