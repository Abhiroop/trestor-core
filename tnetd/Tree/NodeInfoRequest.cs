
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
        public List<Hash> RequestedNodesAdresses;

        public NodeInfoRequest()
        {
            Init();
        }

        void Init()
        {
            TotalRequestedNodes = 0;
            RequestedNodesAdresses = new List<Hash>();
        }

        public void Add(Hash entry)
        {
            RequestedNodesAdresses.Add(entry);
            TotalRequestedNodes++;
        }
        
        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();

            PDTs.Add(ProtocolPackager.PackVarint(TotalRequestedNodes, 0));

            foreach (Hash nie in RequestedNodesAdresses)
            {
                PDTs.Add(ProtocolPackager.Pack(nie, 1));
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
                    Hash address;
                    if (ProtocolPackager.UnpackHash(PDT, 1, out address))
                    {                        
                        RequestedNodesAdresses.Add(address);
                    }
                }                
            }
        }


    }
}
