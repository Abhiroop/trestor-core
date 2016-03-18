
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
    class NodeInfoResponse : ISerializableBase
    {
        public long TotalRequestedNodes;

        /// <summary>
        /// Do not add elements directly.
        /// </summary>
        public List<NodeDataEntity> RequestedNodes;

        public NodeInfoResponse()
        {
            Init();
        }

        public void Add(NodeDataEntity entry)
        {
            RequestedNodes.Add(entry);
            TotalRequestedNodes++;
        }

        private void Init()
        {
            TotalRequestedNodes = 0;
            RequestedNodes = new List<NodeDataEntity>();
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();

            PDTs.Add(ProtocolPackager.PackVarint(TotalRequestedNodes, 0));

            foreach (NodeDataEntity nie in RequestedNodes)
                PDTs.Add(ProtocolPackager.Pack(nie.Serialize(), 1));

            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] Data)
        {
            Init();

            foreach (var PDT in ProtocolPackager.UnPackRaw(Data))
            {
                if (PDT.NameType == 0)
                {
                    ProtocolPackager.UnpackVarint(PDT, 0, ref TotalRequestedNodes);
                }
                else if (PDT.NameType == 1)
                {
                    byte[] _data;
                    if (ProtocolPackager.UnpackByteVector(PDT, 1, out _data))
                    {
                        NodeDataEntity nie = new NodeDataEntity();
                        nie.Deserialize(_data);
                        RequestedNodes.Add(nie);
                    }
                }
            }
        }
    }
}
