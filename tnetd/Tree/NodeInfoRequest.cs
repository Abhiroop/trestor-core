
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
    /// This contains an array of requests for separate nodes.
    /// </summary>
    class NodeInfoRequest
    {
        public long TotalRequestedNodes;
        
        /// <summary>
        /// Do not add elements directly.
        /// </summary>
        public List<NodeInfoEntity> RequestedNodes;

        public NodeInfoRequest()
        {
            Init();
        }

        void Init()
        {
            TotalRequestedNodes = 0;
            RequestedNodes = new List<NodeInfoEntity>();
        }
        
        public void Add(NodeInfoEntity entry)
        {
            RequestedNodes.Add(entry);
            TotalRequestedNodes += entry.LeafCount;
        }
        
        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();

            PDTs.Add(ProtocolPackager.PackVarint(TotalRequestedNodes, 0));

            foreach (NodeInfoEntity nie in RequestedNodes)
            {
                PDTs.Add(ProtocolPackager.Pack(nie.Serialize(), 1));
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
                    ProtocolPackager.UnpackVarint(PDT, 0, ref TotalRequestedNodes);
                }
                else if (PDT.NameType == 1)
                {
                    byte[] _bv = new byte[0];
                    if(ProtocolPackager.UnpackByteVector(PDT, 1, ref _bv))
                    {
                        NodeInfoEntity nie = new NodeInfoEntity();
                        nie.Deserialize(_bv);
                        RequestedNodes.Add(nie);
                    }
                }                
            }
        }


    }
}
