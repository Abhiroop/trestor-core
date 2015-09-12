
//  @Author: Arpan Jati
//  @Date: June 2015 

using System.Collections.Generic;
using TNetD.Protocol;

namespace TNetD.Consensus
{
    class VoteRequestMessage : ISerializableBase
    {
        public LedgerCloseSequence LedgerCloseSequence;

        public VoteRequestMessage()
        {
            LedgerCloseSequence = new LedgerCloseSequence(); ;
        }

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[1];
            PDTs[0] = ProtocolPackager.Pack(LedgerCloseSequence.Serialize(), 0);
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(data);

            if (PDTs.Count == 1)
            {
                byte[] _data = new byte[0];
                if (ProtocolPackager.UnpackByteVector(PDTs[0], 0, ref _data))
                {
                    LedgerCloseSequence.Deserialize(_data);
                }
            }
        }
    }
}
