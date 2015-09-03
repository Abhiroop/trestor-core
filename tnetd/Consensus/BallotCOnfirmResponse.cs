
//  @Author: Arpan Jati
//  @Date: June 2015 

using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.Consensus
{
    class BallotConfirmResponse : ISerializableBase, ISignableBase
    {

        public Hash SignerPublicKey;
        public Hash BallotHash;
        public long LedgerCloseSequence;

        public Hash Signature;

        public BallotConfirmResponse()
        {
            Init();
        }

        public void Init()
        {
            SignerPublicKey = new Hash();
            BallotHash = new Hash();
            Signature = new Hash();
            LedgerCloseSequence = 0;
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            PDTs.Add(ProtocolPackager.Pack(SignerPublicKey, 0));
            PDTs.Add(ProtocolPackager.Pack(BallotHash, 1));
            PDTs.Add(ProtocolPackager.Pack(LedgerCloseSequence, 2));
            PDTs.Add(ProtocolPackager.Pack(Signature, 3));

            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] Data)
        {
            Init();

            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);

            foreach (ProtocolDataType PDT in PDTs)
            {
                switch (PDT.NameType)
                {
                    case 0:
                        ProtocolPackager.UnpackHash(PDT, 0, out SignerPublicKey);
                        break;

                    case 1:
                        ProtocolPackager.UnpackHash(PDT, 1, out BallotHash);
                        break;

                    case 2:
                        ProtocolPackager.UnpackInt64(PDT, 2, ref LedgerCloseSequence);
                        break;

                    case 3:
                        ProtocolPackager.UnpackHash(PDT, 3, out Signature);
                        break;
                }
            }
        }

        public byte[] GetSignatureData()
        {
            List<byte> data = new List<byte>();
            data.AddRange(SignerPublicKey.Hex);
            data.AddRange(BallotHash.Hex);
            data.AddRange(Conversions.Int64ToVector(LedgerCloseSequence));
            return data.ToArray();
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
