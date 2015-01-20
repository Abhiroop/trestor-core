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
        Hash internalHash = new Hash();

        Hash publicKey;
        long money;
        string name;
        AccountState accountState;
        AccountType accountType;
        NetworkType networkType;
        long lastTransactionTime;

        ///////////////////////////////////

        /// <summary>
        /// 32 bytes long public key
        /// </summary>
        public Hash PublicKey
        {
            get { return publicKey; }
            set
            {
                publicKey = value;
                updateInternalHash();
            }
        }

        /// <summary>
        /// Amount of money in 10^6 Trests
        /// </summary>
        public long Money
        {
            get { return money; }
            set
            {
                money = value;
                updateInternalHash();
            }
        }

        /// <summary>
        /// User Name [Optional]
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                updateInternalHash();
            }
        }

        /// <summary>
        /// Current state of the account
        /// </summary>
        public AccountState AccountState
        {
            get { return accountState; }
            set
            {
                accountState = value;
                updateInternalHash();
            }
        }

        /// <summary>
        /// Account Type : Genesis, Validator, Network
        /// </summary>
        public AccountType AccountType
        {
            get { return accountType; }
            set
            {
                accountType = value;
                updateInternalHash();
            }
        }

        /// <summary>
        /// NetworkType : Main / Test
        /// </summary>
        public NetworkType NetworkType
        {
            get { return networkType; }
            set
            {
                networkType = value;
                updateInternalHash();
            }
        }

        /// <summary>
        /// Last Transaction Time in UTC / FileTime
        /// </summary>
        public long LastTransactionTime
        {
            get { return lastTransactionTime; }
            set
            {
                lastTransactionTime = value;
                updateInternalHash();
            }
        }

        public AccountInfo(Hash publicKey, long money)
        {
            this.publicKey = publicKey;
            this.money = money;
            this.name = "";
            this.accountState = AccountState.Normal;
            this.networkType = NetworkType.MainNet;
            this.accountType = AccountType.MainNormal;
            this.lastTransactionTime = 0;

            updateInternalHash();
        }

        public AccountInfo(Hash publicKey, long money, string name, AccountState accountState,
            NetworkType networkType, AccountType accountType, long lastTransactionTime)
        {
            this.publicKey = publicKey;
            this.money = money;
            this.name = name;
            this.accountState = accountState;
            this.networkType = networkType;
            this.accountType = accountType;
            this.lastTransactionTime = lastTransactionTime;

            updateInternalHash();
        }

        private void updateInternalHash()
        {
            List<byte> data = new List<byte>();
            data.AddRange(publicKey.Hex);
            data.AddRange(Conversions.Int64ToVector(money));
            data.AddRange(Encoding.GetEncoding(28591).GetBytes(name));
            data.Add((byte)accountState);
            data.Add((byte)networkType);
            data.Add((byte)accountType);
            data.AddRange(Conversions.Int64ToVector(lastTransactionTime));

            internalHash = new Hash(((new SHA512Cng()).ComputeHash(data.ToArray())).Take(32).ToArray());
        }

        override public Hash GetHash()
        {
            return internalHash;
        }

        override public Hash GetID()
        {
            return publicKey;
        }

        /// <summary>
        /// Calculates the address for the AccountInfo, performs recalculation of the hashes.
        /// A bit expensive, still quite fast.
        /// </summary>
        /// <returns></returns>
        public string GetAddress()
        {
            return AddressFactory.GetAddressString(AddressFactory.GetAddress(publicKey.Hex,
                name, networkType, accountType));
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            PDTs.Add(ProtocolPackager.Pack(publicKey, 0));
            PDTs.Add(ProtocolPackager.Pack(money, 1));
            PDTs.Add(ProtocolPackager.Pack(name, 2));
            PDTs.Add(ProtocolPackager.Pack((byte)accountState, 3));
            PDTs.Add(ProtocolPackager.Pack((byte)networkType, 4));
            PDTs.Add(ProtocolPackager.Pack((byte)accountType, 5));
            PDTs.Add(ProtocolPackager.Pack(lastTransactionTime, 6));
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
                        ProtocolPackager.UnpackHash(PDT, 0, out publicKey);
                        break;

                    case 1:
                        ProtocolPackager.UnpackInt64(PDT, 1, ref money);
                        break;

                    case 2:
                        ProtocolPackager.UnpackString(PDT, 2, ref name);
                        break;

                    case 3:
                        byte _accountState = 0;
                        ProtocolPackager.UnpackByte(PDT, 3, ref _accountState);
                        accountState = (AccountState)_accountState;
                        break;

                    case 4:
                        byte _networkType = 0;
                        ProtocolPackager.UnpackByte(PDT, 4, ref _networkType);
                        networkType = (NetworkType)_networkType;
                        break;

                    case 5:
                        byte _accountType = 0;
                        ProtocolPackager.UnpackByte(PDT, 5, ref _accountType);
                        accountType = (AccountType)_accountType;
                        break;

                    case 6:
                        ProtocolPackager.UnpackInt64(PDT, 6, ref lastTransactionTime);
                        break;
                }
            }
        }

        public static long CalculateTotalMoney(LeafDataType[] ais)
        {
            long _money = 0;
            foreach (LeafDataType ai in ais)
            {
                _money += ((AccountInfo)ai).money;
            }

            return _money;
        }


    }
}


