
//  @Author: Arpan Jati | Stephan Verbuecheln
//  @Date: June 2015 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;
using Chaos.NaCl;

namespace TNetD.Consensus
{
    class Ballot : ISerializableBase, ISignableBase
    {
        SortedSet<Hash> TransactionIds;
        
        /// <summary>
        /// Public key of the signer.
        /// </summary>
        public Hash PublicKey;

        /// <summary>
        /// Signature of the Ballot
        /// </summary>
        public Hash Signature;

        public long Timestamp;

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
        }

        public Ballot()
        {
            Init();
        }

        public byte [] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();

            foreach(Hash txId in TransactionIds)
            {
                PDTs.Add(ProtocolPackager.Pack(txId, 0));
            }
            
            PDTs.Add(ProtocolPackager.Pack(PublicKey, 1));
            PDTs.Add(ProtocolPackager.Pack(Signature, 2));
            PDTs.Add(ProtocolPackager.PackVarint(Timestamp, 2));

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
                        Hash txID;
                        if(ProtocolPackager.UnpackHash(PDT, 0, out txID))
                        {
                            TransactionIds.Add(txID);
                        }
                        break;

                    case 1:
                        ProtocolPackager.UnpackHash(PDT, 1, out PublicKey);
                        break;

                    case 2:
                        ProtocolPackager.UnpackHash(PDT, 2, out Signature);
                        break;

                    case 3:
                        ProtocolPackager.UnpackVarint(PDT, 3, ref Timestamp);
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
