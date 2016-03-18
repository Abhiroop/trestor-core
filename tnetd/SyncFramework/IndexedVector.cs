
// @Author : Arpan Jati
// @Date: 10th June 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.SyncFramework
{
    class IndexedVector : ISerializableBase
    {
        public long Index;
        public byte[] Vector;

        public IndexedVector(long index, byte[] vector)
        {
            Index = index;
            Vector = vector;
        }

        public IndexedVector()
        {
            Vector = new byte[0];
            Index = 0;
        }

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[2];

            int cnt = 0;

            PDTs[cnt++] = (ProtocolPackager.PackVarint(Index, 0));
            PDTs[cnt++] = (ProtocolPackager.Pack(Vector, 1));

            if (cnt != PDTs.Length) throw new Exception("Invalid pack entries");

            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] Data)
        {
            foreach (var PDT in ProtocolPackager.UnPackRaw(Data))
            {
                switch (PDT.NameType)
                {
                    case 0:
                        {
                            ProtocolPackager.UnpackVarint(PDT, 0, ref Index);
                        }
                        break;

                    case 1:
                        {
                            ProtocolPackager.UnpackByteVector(PDT, 1, out Vector);
                        }
                        break;
                }
            }
        }
    }


}
