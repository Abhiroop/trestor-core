
//  @Author: Stephan Verbuecheln
//  @Date: June 2015 

using System.Collections.Generic;
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
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            foreach (Hash transaction in transactions)
            {
                PDTs.Add(ProtocolPackager.Pack(transaction, 0));
            }
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            transactions.Clear();

            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(data);
            foreach (ProtocolDataType pdt in PDTs)
            {
                Hash t = new Hash();
                ProtocolPackager.UnpackHash(pdt, 0, out t);
                transactions.Add(t);
            }
        }
    }
}
