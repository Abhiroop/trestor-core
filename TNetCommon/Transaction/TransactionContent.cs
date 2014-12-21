using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.Transaction
{
    public class TransactionContent
    {
        byte[] PublicKey_Source;
        long Timestamp;
        TransactionEntity[] Destinations;
        byte[] Signature;

        public TransactionContent(byte[] PublicKey_Source, long Timestamp, TransactionEntity[] Destinations, byte[] ExpandedPrivateKey)
        {
            this.Destinations = Destinations;
            this.Timestamp = Timestamp;
            this.PublicKey_Source = PublicKey_Source;
            byte[] TranData = GetTransactionData();
            Signature = Ed25519.Sign(TranData, ExpandedPrivateKey);
        }
        
        /// <summary>
        /// FORMAT : PUBLIC_KEY[32] + TIMESTAMP[8] + [ PK_SINK[32] + AMOUNT ]
        /// </summary>
        /// <returns></returns>
        public byte[] GetTransactionData()
        {
            List<byte> _data = new List<byte>();

            _data.AddRange(PublicKey_Source);
            _data.AddRange(Conversions.Int64ToVector(Timestamp));

            for (int i = 0; i < (int)Destinations.Length; i++)
            {
                TransactionEntity ts = Destinations[i];

                _data.AddRange(ts.PublicKey_Sink);
                _data.AddRange(Conversions.Int64ToVector(ts.Amount));
            }

            return _data.ToArray();
        }
              
        /// <summary>
        /// Serializes all the contents of the class, to be deserialized later.
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            PDTs.Add(ProtocolPackager.Pack(PublicKey_Source, 0));
            PDTs.Add(ProtocolPackager.Pack(Timestamp, 1));

            foreach (TransactionEntity it in Destinations)
            {
                PDTs.Add(ProtocolPackager.Pack(it.Serialize(), 2));
            }

            PDTs.Add(ProtocolPackager.Pack(Signature, 3));

            return ProtocolPackager.PackRaw(PDTs);
        }
    }
}
