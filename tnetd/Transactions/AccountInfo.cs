//
// @Author: Arpan Jati
// @Date: Dec 22, 2014
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TNetD;
using TNetD.Protocol;
using TNetD.Tree;

namespace TNetD.Transactions
{
    public enum AcountState { NORMAL, BLOCKED, DISABLED, PERMANENT_BLOCK, DELETED };

    /// <summary>
    /// Holds the data for an account, for the time being AccountID is the public key.
    /// </summary>
    class AccountInfo : LeafDataType
    {
        public Hash AccountID;
        public long Money;
        public string Name;
        public AcountState accountState;
        public long LastTransactionTime;

        public AccountInfo(Hash _AccountID, long _Money)
        {
            AccountID = _AccountID;
            Money = _Money;
            Name = "";
            accountState = 0;
            LastTransactionTime = 0;
        }

        public AccountInfo(Hash _AccountID, long _Money, string _Name, AcountState accountState, long _LastTransactionTime)
        {
            AccountID = _AccountID;
            Money = _Money;
            Name = _Name;
            this.accountState = accountState;
            LastTransactionTime = _LastTransactionTime;
        }

        override public Hash GetHash()
        {
            List<byte> data = new List<byte>();
            data.AddRange(AccountID.Hex);
            data.AddRange(Conversions.Int64ToVector(Money));
            data.AddRange(Conversions.Int64ToVector(LastTransactionTime));
            data.Add((byte)accountState);
            Hash h = new Hash(((new SHA512Managed()).ComputeHash(data.ToArray())).Take(32).ToArray());
            return h;
        }

        override public Hash GetID()
        {
            return AccountID;
        }

        public override byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            PDTs.Add(ProtocolPackager.Pack(AccountID, 0));
            PDTs.Add(ProtocolPackager.Pack(Money, 1));
            PDTs.Add(ProtocolPackager.Pack(Name, 2));
            PDTs.Add(ProtocolPackager.Pack((byte)accountState, 3));
            PDTs.Add(ProtocolPackager.Pack(LastTransactionTime, 4));
            return ProtocolPackager.PackRaw(PDTs);
        }

        public override void Deserialize(byte[] Data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);
            int cnt = 0;

            while (cnt < (int)PDTs.Count)
            {
                ProtocolDataType PDT = PDTs[cnt++];

                switch (PDT.NameType)
                {
                    case 0:
                        ProtocolPackager.UnpackHash(PDT, 0, ref AccountID);
                        break;

                    case 1:
                        ProtocolPackager.UnpackInt64(PDT, 1, ref Money);
                        break;

                    case 2:
                        ProtocolPackager.UnpackString(PDT, 2, ref Name);
                        break;

                    case 3:
                        byte account_state = 0;
                        ProtocolPackager.UnpackByte(PDT, 3, ref account_state);
                        accountState = (AcountState)account_state;
                        break;

                    case 4:
                        ProtocolPackager.UnpackInt64(PDT, 4, ref LastTransactionTime);
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


