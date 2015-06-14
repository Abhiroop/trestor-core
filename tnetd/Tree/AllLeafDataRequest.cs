
// @Author : Arpan Jati
// @Date: 12th June 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.Tree
{
    /// <summary>
    /// Contains a Node below which all accounts are requested.
    /// </summary>
    class AllLeafDataRequest
    {
        public long TotalRequestedLeaves;
        public Hash AddressNibbles;

        public AllLeafDataRequest()
        {
            this.TotalRequestedLeaves = 0;
            this.AddressNibbles = new Hash();
        }

        public AllLeafDataRequest(NodeDataEntity ndr)
        {
            this.TotalRequestedLeaves = ndr.LeafCount;
            this.AddressNibbles = ndr.AddressNibbles;
        }

        public AllLeafDataRequest(long totalRequestedLeaves, Hash addressNibbles)
        {
            this.TotalRequestedLeaves = totalRequestedLeaves;
            this.AddressNibbles = addressNibbles;
        }  

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            PDTs.Add(ProtocolPackager.PackVarint(TotalRequestedLeaves, 0));
            PDTs.Add(ProtocolPackager.Pack(AddressNibbles, 1));
            return ProtocolPackager.PackRaw(PDTs);
        }                 

        public void Deserialize(byte[] Data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);
            int cnt = 0;

            while (cnt < (int)PDTs.Count)
            {
                ProtocolDataType PDT = PDTs[cnt++];

                if (PDT.NameType == 0)
                {
                    ProtocolPackager.UnpackVarint(PDT, 0, ref TotalRequestedLeaves);
                }
                if (PDT.NameType == 1)
                {
                    ProtocolPackager.UnpackHash(PDT, 1, out AddressNibbles);
                }               
            }
        }
    }
}
