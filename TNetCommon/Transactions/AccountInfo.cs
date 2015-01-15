//
// @Author: Arpan Jati
// @Date: Dec 22, 2014 | 1 Jan, 2015
// 15th Jan 2015 : +AccountType +NetworkType + GetAddress()
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TNetD;
using TNetD.Address;
using TNetD.Protocol;
using TNetD.Tree;

namespace TNetD.Transactions
{
    public enum AccountState { Normal = 0, Blocked = 1, Disabled = 2, PermanentBlock = 3, Deleted = 4 };

    /// <summary>
    /// Holds the data for an account.
    /// </summary>
    public class AccountInfo : LeafDataType, ISerializableBase
    {
        /// <summary>
        /// 32 bytes long public key
        /// </summary>
        public Hash PublicKey;

        /// <summary>
        /// Amount of money in 10^6 Trests
        /// </summary>
        public long Money;

        /// <summary>
        /// User Name [Optional]
        /// </summary>
        public string Name;

        /// <summary>
        /// Current state of the account
        /// </summary>
        public AccountState AccountState;

        /// <summary>
        /// Account Type : Genesis, Validator, Network
        /// </summary>
        public AccountType AccountType;

        /// <summary>
        /// NetworkType : Main / Test
        /// </summary>
        public NetworkType NetworkType;

        /// <summary>
        /// Last Transaction Time in UTC / FileTime
        /// </summary>
        public long LastTransactionTime;

        public AccountInfo(Hash publicKey, long money)
        {
            PublicKey = publicKey;
            Money = money;
            Name = "";
            AccountState = AccountState.Normal;
            NetworkType = NetworkType.MainNet;
            AccountType = AccountType.MainNormal;
            LastTransactionTime = 0;
        }

        public AccountInfo(Hash publicKey, long money, string name, AccountState accountState, 
            NetworkType networkType, AccountType accountType,  long lastTransactionTime)
        {
            PublicKey = publicKey;
            Money = money;
            Name = name;
            AccountState = accountState;
            NetworkType = networkType;
            AccountType = accountType;
            LastTransactionTime = lastTransactionTime;
        }

        override public Hash GetHash()
        {
            List<byte> data = new List<byte>();
            data.AddRange(PublicKey.Hex);
            data.AddRange(Conversions.Int64ToVector(Money));
            data.AddRange(Encoding.GetEncoding(28591).GetBytes(Name));
            data.Add((byte)AccountState);
            data.Add((byte)NetworkType);
            data.Add((byte)AccountType);
            data.AddRange(Conversions.Int64ToVector(LastTransactionTime));
           
            return new Hash(((new SHA512Cng()).ComputeHash(data.ToArray())).Take(32).ToArray());
        }

        override public Hash GetID()
        {
            return PublicKey;
        }

        /// <summary>
        /// Calculates the address for the AccountInfo, performs recalculation of the hashes.
        /// A bit expensive, still quite fast.
        /// </summary>
        /// <returns></returns>
        public string GetAddress()
        {
            return AddressFactory.GetAddressString(AddressFactory.GetAddress(PublicKey.Hex,
                Name, NetworkType, AccountType));
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            PDTs.Add(ProtocolPackager.Pack(PublicKey, 0));
            PDTs.Add(ProtocolPackager.Pack(Money, 1));
            PDTs.Add(ProtocolPackager.Pack(Name, 2));
            PDTs.Add(ProtocolPackager.Pack((byte)AccountState, 3));
            PDTs.Add(ProtocolPackager.Pack((byte)NetworkType, 4));
            PDTs.Add(ProtocolPackager.Pack((byte)AccountType, 5));
            PDTs.Add(ProtocolPackager.Pack(LastTransactionTime, 6));
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] Data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);
            int cnt = 0;

            while (cnt < (int)PDTs.Count)
            {
                ProtocolDataType PDT = PDTs[cnt++];

                switch (PDT.NameType)
                {
                    case 0:
                        ProtocolPackager.UnpackHash(PDT, 0, out PublicKey);
                        break;

                    case 1:
                        ProtocolPackager.UnpackInt64(PDT, 1, ref Money);
                        break;

                    case 2:
                        ProtocolPackager.UnpackString(PDT, 2, ref Name);
                        break;

                    case 3:
                        byte accountState = 0;
                        ProtocolPackager.UnpackByte(PDT, 3, ref accountState);
                        AccountState = (AccountState)accountState;
                        break;

                    case 4:
                        byte networkType = 0;
                        ProtocolPackager.UnpackByte(PDT, 4, ref networkType);
                        NetworkType = (NetworkType)networkType;
                        break;

                    case 5:
                        byte accountType = 0;
                        ProtocolPackager.UnpackByte(PDT, 5, ref accountType);
                        AccountType = (AccountType)accountType;
                        break;

                    case 6:
                        ProtocolPackager.UnpackInt64(PDT, 6, ref LastTransactionTime);
                        break;
                }
            }
        }

        public static long CalculateTotalMoney(LeafDataType[] ais)
        {
            long _money = 0;
            foreach (LeafDataType ai in ais)
            {
                _money += ((AccountInfo)ai).Money;
            }

            return _money;
        }


    }
}


