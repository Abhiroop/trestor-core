
//  @Author: Stephan Verbuecheln
//  @Date: June 2015 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.Consensus
{
    class MergeResponseMsg : ISerializableBase
    {
        public SortedSet<Hash> transactions;

        public MergeResponseMsg()
        {
            transactions = new SortedSet<Hash>();
        }

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[transactions.Count];
            int i = 0;
            foreach (Hash transaction in transactions)
            {
                PDTs[i] = ProtocolPackager.Pack(transaction, 0);
                i++;
            }
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(data);
            foreach (ProtocolDataType p in PDTs)
            {
                Hash t = new Hash();
                ProtocolPackager.UnpackHash(p, 0, out t);
                transactions.Add(t);
            }
        }
    }
}
