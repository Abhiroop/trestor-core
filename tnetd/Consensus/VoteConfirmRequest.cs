
//  @Author: Arpan Jati
//  @Date: June 2015 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.Consensus
{
    class VoteConfirmRequest : ISerializableBase
    {
        public Hash PublicKey;
        public Hash BallotHash;
        public long LedgerCloseSequence;
        
        public VoteConfirmRequest()
        {
            Init();
        }

        public void Init()
        {
            PublicKey = new Hash();
            PublicKey = new Hash();
            LedgerCloseSequence = 0;
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            PDTs.Add(ProtocolPackager.Pack(PublicKey, 0));
            PDTs.Add(ProtocolPackager.Pack(BallotHash, 1));
            PDTs.Add(ProtocolPackager.Pack(LedgerCloseSequence, 2));
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
                        ProtocolPackager.UnpackHash(PDT, 0, out PublicKey);                        
                        break;

                    case 1:
                        ProtocolPackager.UnpackHash(PDT, 1, out BallotHash);
                        break;

                    case 2:
                        ProtocolPackager.UnpackInt64(PDT, 2, ref LedgerCloseSequence);
                        break;
                }
            }
        }
        

    }
}
