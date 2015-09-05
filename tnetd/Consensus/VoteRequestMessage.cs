
//  @Author: Arpan Jati
//  @Date: June 2015 

using System.Collections.Generic;
using TNetD.Protocol;

namespace TNetD.Consensus
{
    class VoteRequestMessage : ISerializableBase
    {
        public long LedgerCloseSequence;

        public VoteRequestMessage()
        {
            LedgerCloseSequence = 0;
        }

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[1];
            PDTs[0] = ProtocolPackager.PackVarint(LedgerCloseSequence, 0);
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(data);

            if (PDTs.Count == 1)
            {
                ProtocolPackager.UnpackVarint(PDTs[0], 0, ref LedgerCloseSequence);               
            }
        }
    }
}
