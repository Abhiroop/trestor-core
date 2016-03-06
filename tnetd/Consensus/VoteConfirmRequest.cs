
//  @Author: Arpan Jati
//  @Date: 5th September 2015 

using TNetD.Protocol;
using System.Collections.Generic;

namespace TNetD.Consensus
{
    class VoteConfirmRequest : ISerializableBase
    {
        public Hash PublicKey;
        public LedgerCloseSequence LedgerCloseSequence;

        public VoteConfirmRequest()
        {
            Init();
        }

        public void Init()
        {
            PublicKey = new Hash();
            LedgerCloseSequence = new LedgerCloseSequence();
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            PDTs.Add(ProtocolPackager.Pack(PublicKey, 0));
            PDTs.Add(ProtocolPackager.Pack(LedgerCloseSequence.Serialize(), 1));
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
                        byte[] _data = new byte[0];
                        if (ProtocolPackager.UnpackByteVector(PDT, 1, ref _data))
                        {
                            LedgerCloseSequence.Deserialize(_data);
                        }
                        break;

                }
            }
        }

    }
}
