
//  @Author: Arpan Jati | Stephan Verbuecheln
//  @Date: June 2015 | Sept 2015

using TNetD.Protocol;
using System.Collections.Generic;
using Chaos.NaCl;
using System.Security.Cryptography;
using System.Linq;

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

        public Ballot()
        {
            Init();
            LedgerCloseSequence = 0;
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
            
            foreach (Hash txId in TransactionIds)
            {
                PDTs.Add(ProtocolPackager.Pack(txId, 0));
            }

            PDTs.Add(ProtocolPackager.Pack(LedgerCloseSequence, 1));
            PDTs.Add(ProtocolPackager.PackVarint(Timestamp, 2));
            PDTs.Add(ProtocolPackager.Pack(PublicKey, 3));
            PDTs.Add(ProtocolPackager.Pack(Signature, 4));            

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
                        if (ProtocolPackager.UnpackHash(PDT, 0, out txID))
                        {
                            TransactionIds.Add(txID);
                        }
                        break;

                    case 1:
                        ProtocolPackager.UnpackVarint(PDT, 1, ref LedgerCloseSequence);
                        break;

                    case 2:
                        ProtocolPackager.UnpackVarint(PDT, 2, ref Timestamp);
                        break;

                    case 3:
                        ProtocolPackager.UnpackHash(PDT, 3, out PublicKey);
                        break;                 

                    case 4:
                        ProtocolPackager.UnpackHash(PDT, 4, out Signature);
                        break;
                }
            }

        }
        
        public byte[] GetHashData()
        {
            List<byte> data = new List<byte>();

            foreach (Hash transaction in TransactionIds)
            {
                data.AddRange(transaction.Hex);
            }

            data.AddRange(Conversions.Int64ToVector(LedgerCloseSequence));
            data.AddRange(PublicKey.Hex);

            return data.ToArray();
        }

        public byte[] GetSignatureData()
        {
            List<byte> data = new List<byte>();

            data.AddRange(GetHashData());
            data.AddRange(Conversions.Int64ToVector(Timestamp));

            return data.ToArray();
        }

        /// <summary>
        /// Everytime the Hash is recomputed.
        /// </summary>
        public Hash BallotDataHash
        {
            get
            {
                return new Hash((new SHA512Cng()).ComputeHash(GetHashData()).Take(32).ToArray() );
            }
        }

        public void UpdateSignature(byte[] signature)
        {
            this.Signature = new Hash(signature);
        }

        public bool VerifySignature(Hash publicKey)
        {
            if (publicKey.Hex.Length != 32) return false;

            return Ed25519.Verify(Signature.Hex, GetSignatureData(), publicKey.Hex);
        }


    }
}
