
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
    class NodeInfoEntity
    {
        public Hash NodeHash;
        public long LeafCount;
        public Hash AddressNibbles;

        public NodeInfoEntity()
        {
            Init();
        }
        
        public NodeInfoEntity(ListTreeNode node)
        {
            NodeHash = node.Hash;
            LeafCount = node.LeafCount;
            AddressNibbles = node.addressNibbles;
        }

        public NodeInfoEntity(NodeDataResponse ndr)
        {
            NodeHash = ndr.NodeHash;
            LeafCount = ndr.LeafCount;
            AddressNibbles = ndr.AddressNibbles;
        }

        private void Init()
        {
            NodeHash = new Hash();
            LeafCount = 0;
            AddressNibbles = new Hash();
        }
        
        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();

            PDTs.Add(ProtocolPackager.Pack(NodeHash, 0));
            PDTs.Add(ProtocolPackager.Pack(AddressNibbles, 1));
            PDTs.Add(ProtocolPackager.PackVarint(LeafCount, 2));

            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] Data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);
            int cnt = 0;

            Init();

            while (cnt < (int)PDTs.Count)
            {
                ProtocolDataType PDT = PDTs[cnt++];

                if (PDT.NameType == 0)
                {
                    ProtocolPackager.UnpackHash(PDT, 0, out NodeHash);
                }
                if (PDT.NameType == 1)
                {
                    ProtocolPackager.UnpackHash(PDT, 1, out AddressNibbles);
                }
                else if (PDT.NameType == 2)
                {
                    ProtocolPackager.UnpackVarint(PDT, 2, ref LeafCount);
                }
            }
        }
    }
}
