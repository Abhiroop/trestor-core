
// @Author : Arpan Jati
// @Date: 13th June 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;
using TNetD.Transactions;

namespace TNetD.Tree
{
    /// <summary>
    /// This contains an array of requests for separate nodes.
    /// </summary>
    class LeafAccountDataResponse
    {
        public long LeafCount;

        /// <summary>
        /// Do not add elements directly.
        /// </summary>
        public List<AccountInfo> Leaves;

        public LeafAccountDataResponse()
        {
            Init();
        }

        void Init()
        {
            LeafCount = 0;
            Leaves = new List<AccountInfo>();
        }

        public void Add(AccountInfo entry)
        {
            Leaves.Add(entry);
            LeafCount++;
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();

            PDTs.Add(ProtocolPackager.PackVarint(LeafCount, 0));

            foreach (AccountInfo nie in Leaves)
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
                    ProtocolPackager.UnpackVarint(PDT, 0, ref LeafCount);
                }
                else if (PDT.NameType == 1)
                {
                    byte[] _bv = new byte[0];
                    if (ProtocolPackager.UnpackByteVector(PDT, 1, ref _bv))
                    {
                        AccountInfo nie = new AccountInfo();
                        nie.Deserialize(_bv);
                        Leaves.Add(nie);
                    }
                }
            }
        }


    }
}
