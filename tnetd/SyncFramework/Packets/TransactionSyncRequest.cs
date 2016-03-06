
// @Author : Arpan Jati
// @Date: 12th Feb 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.SyncFramework.Packets
{
    class TransactionSyncRequest : ISerializableBase
    {
        /// <summary>
        /// Inclusive sequence number.
        /// </summary>
        public long StartSequenceNumber;

        /// <summary>
        /// Number of elements.
        /// </summary>
        public long Length;

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[2];

            int cnt = 0;

            PDTs[cnt++] = (ProtocolPackager.Pack(StartSequenceNumber, 0));

            PDTs[cnt++] = (ProtocolPackager.Pack(Length, 1));

            if (cnt != PDTs.Length) throw new Exception("Invalid pack entries");

            return ProtocolPackager.PackRaw(PDTs);
        }


        public void Deserialize(byte[] Data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);
            int cnt = 0;

            while (cnt < (int)PDTs.Count)
            {
                ProtocolDataType PDT = PDTs[cnt++];

                switch (PDT.NameType)
                {
                    case 0:
                        {
                            ProtocolPackager.UnpackInt64(PDT, 0, ref StartSequenceNumber);
                        }
                        break;

                    case 1:
                        {
                            ProtocolPackager.UnpackInt64(PDT, 1, ref Length);
                        }
                        break;
                }
            }
        }



    }
}
