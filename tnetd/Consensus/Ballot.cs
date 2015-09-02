
//  @Author: Arpan Jati | Stephan Verbuecheln
//  @Date: June 2015 

using TNetD.Protocol;
using System.Collections.Generic;

namespace TNetD.Consensus
{
    class Ballot : ISerializableBase, ISignableBase
    {
        public long LedgerCloseSequence;

        public SortedSet<Hash> TransactionIds;

        /// <summary>
        /// Public key of the signer.
        /// </summary>
        public Hash PublicKey;

        /// <summary>
        /// Signature of the Ballot
        /// </summary>
        public Hash Signature;

        public long Timestamp;

        public Ballot(long ledgerCloseSequence)
        {
            Init();

            LedgerCloseSequence = ledgerCloseSequence;
        }

        public bool Add(Hash TransactionID)
        {
            if (!TransactionIds.Contains(TransactionID))
            {
                TransactionIds.Add(TransactionID);
            }

            return false;
        }

        void Init()
        {
            TransactionIds = new SortedSet<Hash>();
            PublicKey = new Hash();
            Signature = new Hash();
            Timestamp = 0;
            LedgerCloseSequence = 0;
        }
        
        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();

            PDTs.Add(ProtocolPackager.Pack(LedgerCloseSequence, 0));

            foreach (Hash txId in TransactionIds)
            {
                PDTs.Add(ProtocolPackager.Pack(txId, 1));
            }

            PDTs.Add(ProtocolPackager.Pack(PublicKey, 2));
            PDTs.Add(ProtocolPackager.Pack(Signature, 3));
            PDTs.Add(ProtocolPackager.PackVarint(Timestamp, 4));

            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] Data)
        {
            Init();

            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);
            int cnt = 0;

            while (cnt < (int)PDTs.Count)
            {
                ProtocolDataType PDT = PDTs[cnt++];

                switch (PDT.NameType)
                {
                    case 0:
                        ProtocolPackager.UnpackVarint(PDT, 0, ref LedgerCloseSequence);
                        break;

                    case 1:
                        Hash txID;
                        if (ProtocolPackager.UnpackHash(PDT, 1, out txID))
                        {
                            TransactionIds.Add(txID);
                        }
                        break;

                    case 2:
                        ProtocolPackager.UnpackHash(PDT, 2, out PublicKey);
                        break;

                    case 3:
                        ProtocolPackager.UnpackHash(PDT, 3, out Signature);
                        break;

                    case 4:
                        ProtocolPackager.UnpackVarint(PDT, 4, ref Timestamp);
                        break;
                }
            }
        }

        public byte[] GetSignatureData()
        {
            List<byte> data = new List<byte>();
            foreach (Hash transaction in TransactionIds)
            {
                data.AddRange(transaction.Hex);
            }
            data.AddRange(PublicKey.Hex);
            data.AddRange(Conversions.Int64ToVector(Timestamp));
            return data.ToArray();
        }

        public void UpdateSignature(byte[] signature)
        {
            this.Signature = new Hash(signature);
        }

    }
}
