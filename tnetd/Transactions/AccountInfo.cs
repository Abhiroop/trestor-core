using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TNetD.Protocol;
using TNetD.Tree;

namespace TNetD.Transactions
{
    /// <summary>
    /// Holds the data for an account, for the time being AccountID is the public key.
    /// </summary>
    class AccountInfo : LeafDataType
    {
        public Hash AccountID;
        public long Money;
        public string Name;
        public byte BlockState;
        public long LastTransactionTime;

        public AccountInfo(Hash _AccountID, long _Money)
        {
            AccountID = _AccountID;
            Money = _Money;
            Name = "";
            BlockState = 0;
            LastTransactionTime = 0;
        }

        public AccountInfo(Hash _AccountID, long _Money, string _Name, byte _BlockState, long _LastTransactionTime)
        {
            AccountID = _AccountID;
            Money = _Money;
            Name = _Name;
            BlockState = _BlockState;
            LastTransactionTime = _LastTransactionTime;
        }
       
        override public Hash GetHash()
        {
            List<byte> data = new List<byte>();
            data.AddRange(AccountID.Hex);
            data.AddRange(BitConverter.GetBytes(Money));
            Hash h = new Hash(((new SHA512Managed()).ComputeHash(data.ToArray())).Take(32).ToArray());
            return h;
        }

        override public Hash GetID()
        {
            return AccountID;
        }

        public override byte[] Serialize()
        {
            return new byte [0];
        }

        public override void Deserialize(byte[] Data)
        {

        }
    }


   
}
