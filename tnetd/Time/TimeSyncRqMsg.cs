using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;


namespace TNetD.Time
{
    class TimeSyncRqMsg : ISerializableBase
    {

        public long senderTime;



        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[1];
            PDTs[0] = (ProtocolPackager.Pack(senderTime, 0));
            return ProtocolPackager.PackRaw(PDTs);
        }


        public void Deserialize(byte[] Data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);
            ProtocolPackager.UnpackInt64(PDTs[0], 0, ref senderTime);
        }



    }
}

    
