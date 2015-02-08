﻿/*
 * @author: Arpan Jati
 *  @date:  Original Sept 2014
 *          13th Jan 2015: Switched to use Base58
 *          15th Jan 2015: (Added NetworkType and AccountType in Hash + Added static methods)
 *          26th Jan 2015: Address Verifications
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Diagnostics.Contracts;
using Chaos.NaCl;

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
        NetworkType _NetworkType;
        AccountType _AccountType;

        public AddressFactory()
        {
            _NetworkType = NetworkType.MainNet;
            _AccountType = AccountType.MainNormal;
        }

        public AddressFactory(NetworkType networkType, AccountType accountType)
        {
            _NetworkType = networkType;
            _AccountType = accountType;
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
        /// Creates a new account and returns a Tuple containing account information and SecretSeed
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static Tuple<AccountIdentifier, byte[]> CreateNewAccount(string Name = "")
        {
            byte[] PrivateSecretSeed = new byte[Common.KEYLEN_PRIVATE];

            Common.rngCsp.GetBytes(PrivateSecretSeed);

            byte[] PublicKey;
            byte[] SecretKeyExpanded;

            Ed25519.KeyPairFromSeed(out PublicKey, out SecretKeyExpanded, PrivateSecretSeed);

            byte[] Address = GetAddress(PublicKey, Name, Common.NetworkType,
                (Common.NetworkType == NetworkType.MainNet) ?
                AccountType.MainNormal : AccountType.TestNormal);

            string ADD = Base58Encoding.EncodeWithCheckSum(Address);

            return new Tuple<AccountIdentifier, byte[]>(new AccountIdentifier(PublicKey, Name, ADD), PrivateSecretSeed);
        }

        public static AccountIdentifier PrivateKeyToAccount(byte[] PrivateSecretSeed, string Name = "",
            NetworkType NetworkType = NetworkType.MainNet, AccountType AccountType = AccountType.MainNormal)
        {
            byte[] PublicKey;
            byte[] SecretKeyExpanded;
            Ed25519.KeyPairFromSeed(out PublicKey, out SecretKeyExpanded, PrivateSecretSeed);
            return PublicKeyToAccount(PublicKey, Name, NetworkType, AccountType);
        }

        public static AccountIdentifier PublicKeyToAccount(byte[] PublicKey, string Name = "",
            NetworkType NetworkType = NetworkType.MainNet, AccountType AccountType = AccountType.MainNormal)
        {
            byte[] Address = GetAddress(PublicKey, Name, NetworkType, AccountType);
            string ADD = Base58Encoding.EncodeWithCheckSum(Address);
            return new AccountIdentifier(PublicKey, Name, ADD);
        }

        public static AddressData DecodeAddressString(string Base58Address)
        {
            return new AddressData(Base58Address);
        }

        public static AccountIdentifier CreateAccountIdentifier(byte[] publicKey, string name, string addressDataString)
        {
            return new AccountIdentifier(publicKey, name, addressDataString);
        }
        
        /// <summary>
        /// Returns the 
        /// H = SHA512, 
        /// Address Format : Address = NetType || AccountType || [H(H(PK) || PK || NAME || NetType || AccountType)], Take first 20 bytes}
        /// </summary>
        public static byte[] GetAddress(byte[] PublicKey, string UserName, NetworkType networkType, AccountType accountType)
        {            
            if (!Utils.ValidateUserName(UserName)) throw new ArgumentException("Usernames should be lowercase and alphanumeric, _ is allowed");

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

            byte[] NA_Type = new byte[] { (byte)networkType, (byte)accountType };

            byte[] Hpk__PK__NAME = Hpk.Concat(PublicKey).Concat(NAME).Concat(NA_Type).ToArray();

            byte[] H_Hpk__PK__NAME = (new SHA512Cng()).ComputeHash(Hpk__PK__NAME).Take(20).ToArray();

            byte[] Address_PH = new byte[22];

            Address_PH[0] = NA_Type[0];
            Address_PH[1] = NA_Type[1];
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
            if (Address.ByteArrayEquals(GetAddress(PublicKey, UserName, (NetworkType)Address[0], (AccountType)Address[1])))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Verifies an address for proper PublicKey and UserName, uses the type from Address string.
        /// </summary>
        /// <param name="Address"></param>
        /// <param name="PublicKey"></param>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public static bool VerfiyAddress(string Address, byte[] PublicKey, string UserName)
        {
            AddressData address = new AddressData(Address);
                        
            if (!Utils.ValidateUserName(UserName)) return false;

            byte[] ExpectedAddress = GetAddress(PublicKey, UserName, address.NetworkType, address.AccountType);

            if (Utils.ByteArrayEquals(address.AddressBinary, ExpectedAddress))
            {
                return true;
            }

            return false;
        }

        public static bool VerfiyAddress(out AddressData addressData, string Address, byte[] PublicKey, string UserName)
        {
            addressData = new AddressData(Address);

            if (!Utils.ValidateUserName(UserName)) return false;

            byte[] ExpectedAddress = GetAddress(PublicKey, UserName, addressData.NetworkType, addressData.AccountType);

            if (Utils.ByteArrayEquals(addressData.AddressBinary, ExpectedAddress))
            {
                return true;
            }

            return false;
        }

        public static bool VerfiyAddress(string Address, byte[] PublicKey, string UserName, NetworkType networkType, AccountType accountType)
        {
            if (!Utils.ValidateUserName(UserName)) return false;

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



