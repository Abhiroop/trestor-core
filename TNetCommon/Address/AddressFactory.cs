/*
 * @author: Arpan Jati
 *  @date:  Original Sept 2014
 *          13th Jan 2015: Switched to use Base58
 *          15th Jan 2015: (Added NetworkType and AccountType in Hash + Added static methods)
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
    public enum NetworkType { MainNet = 14, TestNet = 29 };
    public enum AccountType
    {
        MainGenesis = 201, MainValidator = 234, MainNormal = 217,
        TestGenesis = 25, TestValidator = 59, TestNormal = 40
    };

    /// <summary>
    /// Generate addresses from PublicKey and UserName
    /// </summary>
    public class AddressFactory
    {
        NetworkType _NetworkType = NetworkType.MainNet;
        AccountType _AccountType = AccountType.MainNormal;

        public AddressFactory()
        {

        }

        public AddressFactory(NetworkType networkType, AccountType accountType)
        {
            this._NetworkType = networkType;
            this._AccountType = accountType;
        }

        public NetworkType NetworkType
        {
            get { return _NetworkType; }
            set { _NetworkType = value; }
        }

        public AccountType AccountType
        {
            get { return _AccountType; }
            set { _AccountType = value; }
        }

        /// <summary>
        /// Returns the 
        /// H = SHA512, 
        /// Address Format : Address = NetType || AccountType || {[H(H(PK) || PK || NAME || NetType || AccountType)], Take first 20 bytes}
        /// </summary>
        public static byte[] GetAddress(byte[] PublicKey, string UserName, NetworkType networkType, AccountType accountType)
        {
            //Contract.Requires<ArgumentException>(PublicKey != null);
            //Contract.Requires<ArgumentException>(UserName != null);
            //Contract.Requires<ArgumentException>(PublicKey.Length == 32, "Public key length must be 32 bytes.");
            //Contract.Requires<ArgumentException>(UserName.Length < 64, "Username length should be less than 64.");

            if (networkType == NetworkType.MainNet)
            {
                if (accountType == AccountType.TestGenesis ||
                    accountType == AccountType.TestValidator ||
                    accountType == AccountType.TestNormal)
                {
                    throw new ArgumentException("Invalid AccountType for the provided NetworkType.");
                }
            }

            byte[] NAME = Utils.Encoding88591.GetBytes(UserName);

            byte[] Hpk = (new SHA512Cng()).ComputeHash(PublicKey);

            ///////////////

            byte[] N_A_Type = new byte[2];
            N_A_Type[0] = (byte)networkType;
            N_A_Type[1] = (byte)accountType;

            ///////////////

            byte[] Hpk__PK__NAME = Hpk.Concat(PublicKey).Concat(NAME).Concat(N_A_Type).ToArray();

            byte[] H_Hpk__PK__NAME = (new SHA512Cng()).ComputeHash(Hpk__PK__NAME).Take(20).ToArray();

            byte[] Address_PH = new byte[22];

            Address_PH[0] = N_A_Type[0];
            Address_PH[1] = N_A_Type[1];
            Array.Copy(H_Hpk__PK__NAME, 0, Address_PH, 2, 20);

            /* byte[] CheckSum = (new SHA512Managed()).ComputeHash(Address_PH, 0, 22).Take(4).ToArray();
             Array.Copy(CheckSum, 0, Address_PH, 22, 4);*/

            return Address_PH;
        }

        public byte[] GetAddress(byte[] PublicKey, string UserName)
        {
            return GetAddress(PublicKey, UserName, _NetworkType, _AccountType);
        }

        /// <summary>
        /// Returns true if the address, is consistent with the provided UserName and PublicKey.
        /// </summary>
        /// <param name="Address">Address without checksum.</param>
        /// <param name="PublicKey">32 byte Public Key</param>
        /// <param name="UserName">UserName / can be zero length.</param>
        /// <returns></returns>
        public bool VerfiyAddress(byte[] Address, byte[] PublicKey, string UserName)
        {
            if (Address.ByteArrayEquals(GetAddress(PublicKey, UserName)))
            {
                return true;
            }

            return false;
        }

        public bool VerfiyAddress(string Address, byte[] PublicKey, string UserName)
        {
            if (Address == Base58Encoding.EncodeWithCheckSum(GetAddress(PublicKey, UserName)))
            {
                return true;
            }

            return false;
        }

        public static bool VerfiyAddress(string Address, byte[] PublicKey, string UserName, NetworkType networkType, AccountType accountType)
        {
            if (Address == Base58Encoding.EncodeWithCheckSum(GetAddress(PublicKey, UserName, networkType, accountType)))
            {
                return true;
            }

            return false;
        }

        public static string GetAddressString(byte[] Address)
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

    }
}



