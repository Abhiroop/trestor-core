
// @Author : Arpan Jati
// @Date: 10th June 2015 | March 18, 2016

using TNetD.Protocol;
using System.Collections.Generic;

namespace TNetD.Tree
{
    class RootDataResponse : ISerializableBase
    {
        public Hash RootHash;
        public long LeafCount;
        public NodeDataEntity[] Children;
        public LedgerCloseData LedgerCloseData;

        public RootDataResponse(ListTreeNode root, LedgerCloseData ledgerCloseData)
        {
            RootHash = root.Hash;
            LeafCount = root.LeafCount;
            LedgerCloseData = ledgerCloseData;
            Children = new NodeDataEntity[16];
            for (int i = 0; i < 16; i++)
            {
                ListTreeNode LTN = root.Children[i];
                if (LTN != null) Children[i] = new NodeDataEntity(LTN);
            }
        }

        public RootDataResponse()
        {
            Init(); // Seems redundant.
        }

        private void Init()
        {
            RootHash = new Hash();
            LeafCount = 0;
            LedgerCloseData = new LedgerCloseData();
            Children = new NodeDataEntity[16];
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
            Init();

            foreach (var PDT in ProtocolPackager.UnPackRaw(Data))
            {
                if ((PDT.NameType >= 10) && (PDT.NameType <= 26))
                {
                    byte[] _data2;
                    if (ProtocolPackager.UnpackByteVector(PDT, PDT.NameType, out _data2))
                    {
                        NodeDataEntity nd = new NodeDataEntity();
                        nd.Deserialize(_data2);
                        Children[PDT.NameType - 10] = nd;
                    }
                }
                else
                {
                    switch (PDT.NameType)
                    {
                        case 0:
                            byte[] _data0;
                            ProtocolPackager.UnpackByteVector(PDT, 0, out _data0);
                            RootHash = new Hash(_data0);
                            break;

                        case 1:
                            ProtocolPackager.UnpackVarint(PDT, 1, ref LeafCount);
                            break;

                        case 2:
                            byte[] _data1;
                            if (ProtocolPackager.UnpackByteVector(PDT, 2, out _data1))
                            {
                                LedgerCloseData.Deserialize(_data1);
                            }
                            break;
                    }
                }
            }
        }
    }
}
