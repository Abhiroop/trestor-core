
// @Author : Arpan Jati
// @Date: 10th June 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.Tree
{  
    class NodeDataResponseMessage : ISerializableBase
    {
        public Hash NodeHash;
        public long LeafCount;
        public Hash[] Children;

        public NodeDataResponseMessage(ListTreeNode node)
        {
            NodeHash = node.Hash;
            LeafCount = node.LeafCount;

            Children = new Hash[16];
            for (int i = 0; i < 16; i++)
            {
                ListTreeNode LTN = node.Children[i];
                Children[i] = LTN.Hash;
            }
        }

        public NodeDataResponseMessage()
        {
            Init(); // Seems redundant.
        }

        private void Init()
        {
            NodeHash = new Hash();
            LeafCount = 0;
            Children = new Hash[16];
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();

            PDTs.Add(ProtocolPackager.Pack(NodeHash, 0));
            PDTs.Add(ProtocolPackager.PackVarint(LeafCount, 1));

            for (int i = 0; i < 16; i++)
            {
                if (Children[i].Hex.Length == 16)
                {
                    PDTs.Add(ProtocolPackager.Pack(Children[i], (byte)(i + 10)));
                }
            }

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
                    byte[] _data0 = new byte[0];
                    ProtocolPackager.UnpackByteVector(PDT, 0, ref _data0);
                    NodeHash = new Hash(_data0);
                }
                else if (PDT.NameType == 1)
                {
                    ProtocolPackager.UnpackVarint(PDT, 1, ref LeafCount);
                }       
                else if ((PDT.NameType >= 10) && (PDT.NameType <= 26))
                {
                    byte[] _data2 = new byte[0];
                    if (ProtocolPackager.UnpackByteVector_s(PDT, PDT.NameType, 32, ref _data2))
                    {
                        Children[PDT.NameType - 10] = new Hash(_data2);
                    }
                }
            }
        }


    }
}
