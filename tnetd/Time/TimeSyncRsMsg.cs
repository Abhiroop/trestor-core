using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;


namespace TNetD.Time
{
    class TimeSyncRsMsg
    {
        public long senderTime;
        public long responderTime;

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[2];
            PDTs[0] = (ProtocolPackager.Pack(senderTime, 0));
            PDTs[1] = (ProtocolPackager.Pack(responderTime, 1));
            return ProtocolPackager.PackRaw(PDTs);
        }


        public void Deserialize(byte[] Data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);
            ProtocolPackager.UnpackInt64(PDTs[0], 0, ref senderTime);
            ProtocolPackager.UnpackInt64(PDTs[1], 1, ref senderTime);
        }
    }
}
