
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
    class RootData : ISerializableBase
    {
        public Hash RootHash;
        public long LeafCount;
        public NodeData[] Children;
        public LedgerCloseData LedgerCloseData;

        public RootData(ListTreeNode root, LedgerCloseData ledgerCloseData)
        {
            RootHash = root.Hash;
            LeafCount = root.LeafCount;
            LedgerCloseData = ledgerCloseData;
            Children = new NodeData[16];
            for (int i = 0; i < 16; i++)
            {
                ListTreeNode LTN = root.Children[i];
                if(LTN != null) Children[i] = new NodeData(LTN);
            }
        }

        public RootData()
        {
            Init(); // Seems redundant.
        }

        private void Init()
        {
            RootHash = new Hash();
            LeafCount = 0;
            LedgerCloseData = new LedgerCloseData();
            Children = new NodeData[16];
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();

            PDTs.Add(ProtocolPackager.Pack(RootHash, 0));
            PDTs.Add(ProtocolPackager.PackVarint(LeafCount, 1));
            PDTs.Add(ProtocolPackager.Pack(LedgerCloseData.Serialize(), 2));

            for (int i = 0; i < 16; i++)
            {
                if (Children[i] != null)
                {
                    PDTs.Add(ProtocolPackager.Pack(Children[i].Serialize(), (byte)(i + 10)));
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
                    RootHash = new Hash(_data0);
                }
                else if (PDT.NameType == 1)
                {
                    ProtocolPackager.UnpackVarint(PDT, 1, ref LeafCount);
                }
                else if (PDT.NameType == 2)
                {
                    byte[] _data1 = new byte[0];
                    if (ProtocolPackager.UnpackByteVector(PDT, 2, ref _data1))
                    {
                        LedgerCloseData.Deserialize(_data1);
                    }
                }
                else if ((PDT.NameType >= 10) && (PDT.NameType <= 26))
                {
                    byte[] _data2 = new byte[0];
                    if(ProtocolPackager.UnpackByteVector_s(PDT, PDT.NameType, 32, ref _data2))
                    {
                        NodeData nd = new NodeData();
                        nd.Deserialize(_data2);
                        Children[PDT.NameType - 10] = nd;
                    }
                }
            }
        }
    }



}
