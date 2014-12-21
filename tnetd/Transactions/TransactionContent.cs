using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TNetD.Ledgers;
using TNetD.Tree;

namespace TNetD.Transactions
{

    class TransactionContent : LeafDataType
    {
        public Hash PublicKey_Source;
        public long Timestamp;
        public TransactionSink[] Destinations;
        public byte[] Signature;

        public TransactionContent(Hash PublicKey_Source, long Timestamp, TransactionSink[] Destinations, byte[] Signature)
        {
            this.Destinations = Destinations;
            this.Timestamp = Timestamp;
            this.PublicKey_Source = PublicKey_Source;
            this.Signature = Signature;
        }

        public TransactionContent()
        {

        }

        private byte[] GetTransactionData()
        {
            List<byte> _data = new List<byte>();

            // Data Hash Format : PK_SRC, TS, DESTS[PK_Sink,Amount]

            _data.AddRange(this.PublicKey_Source.Hex);
            _data.AddRange(BitConverter.GetBytes(this.Timestamp));

            foreach (TransactionSink ts in this.Destinations)
            {
                _data.AddRange(ts.PublicKey_Sink.Hex);
                _data.AddRange(BitConverter.GetBytes(ts.Amount));
            }

            return _data.ToArray();
        }

        public void UpdateAndSignContent(Hash PublicKey_Source, long Timestamp,
           TransactionSink[] Destinations, byte[] ExpandedPrivateKey)
        {
            this.Destinations = Destinations;
            this.Timestamp = Timestamp;
            this.PublicKey_Source = PublicKey_Source;

            this.Signature = Ed25519.Sign(GetTransactionData(), ExpandedPrivateKey);
        }

        override public Hash GetHash()
        {
            return new Hash((new SHA256Managed()).ComputeHash(GetTransactionDataAndSignature()));
        }

        override public Hash GetID()
        {
            return GetHash();
        }

        public byte[] GetTransactionDataAndSignature()
        {
            List<byte> _data = new List<byte>();
            // Data Hash Format : PK_SRC, TS, DESTS[PK_Sink,Amount], SIG
            _data.AddRange(GetTransactionData());
            _data.AddRange(this.Signature);
            return _data.ToArray();
        }



        public override byte[] Serialize()
        {
            return new byte[0];
        }
        public override void Deserialize(byte[] Data)
        {

        }
    }
}
