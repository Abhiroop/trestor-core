
//  @Author: Arpan Jati
//  @Date: 5th September 2015 

using TNetD.Protocol;
using System.Collections.Generic;

namespace TNetD.Consensus
{
    class VoteConfirmRequest : ISerializableBase
    {
        public Hash PublicKey;
        public long LedgerCloseSequence;

        public VoteConfirmRequest()
        {
            Init();
        }

        public void Init()
        {
            PublicKey = new Hash();
            LedgerCloseSequence = 0;
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            PDTs.Add(ProtocolPackager.Pack(PublicKey, 0));
            PDTs.Add(ProtocolPackager.PackVarint(LedgerCloseSequence, 1));
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

                    case 2:
                        ProtocolPackager.UnpackVarint(PDT, 1, ref LedgerCloseSequence);
                        break;

                }
            }
        }

    }
}
