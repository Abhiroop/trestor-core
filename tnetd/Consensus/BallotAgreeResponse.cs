
//  @Author: Arpan Jati
//  @Date: 24 June 2015 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.Consensus
{
    class BallotAgreeResponse : ISerializableBase
    {
        
        public Hash SignerPublicKey;
        public Hash BallotHash;
        public long LedgerCloseSequence;
        public Hash Signature;

        public BallotAgreeResponse()
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
            PDTs.Add(ProtocolPackager.Pack(Signature, 2));
            PDTs.Add(ProtocolPackager.Pack(LedgerCloseSequence, 3));
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] Data)
        {
            Init();

            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);            

            foreach(ProtocolDataType PDT in PDTs)
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
                        ProtocolPackager.UnpackHash(PDT, 2, out Signature);
                        break;

                    case 3:
                        ProtocolPackager.UnpackInt64(PDT, 3, ref LedgerCloseSequence);
                        break;
                }
            }
        }

       // public 

    }
}
