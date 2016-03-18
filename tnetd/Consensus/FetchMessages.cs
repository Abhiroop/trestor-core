
//  @Author: Stephan Verbuecheln
//  @Date: June 2015 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Transactions;
using TNetD.Protocol;

namespace TNetD.Consensus
{
    class FetchRequestMsg : ISerializableBase
    {
        public HashSet<Hash> IDs;

        public FetchRequestMsg()
        {
            IDs = new HashSet<Hash>();
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            foreach (Hash id in IDs)
            {
                PDTs.Add(ProtocolPackager.Pack(id, 0));
            }
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(data);
            foreach (ProtocolDataType pdt in PDTs)
            {
                Hash id = new Hash();
                ProtocolPackager.UnpackHash(pdt, 0, out id);
                IDs.Add(id);
            }
        }
    }
    
    class FetchResponseMsg : ISerializableBase
    {
        public Dictionary<Hash, TransactionContent> transactions;

        public FetchResponseMsg()
        {
            transactions = new Dictionary<Hash, TransactionContent>();
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            foreach (KeyValuePair<Hash, TransactionContent> transaction in transactions)
            {
                PDTs.Add(ProtocolPackager.Pack(transaction.Value.Serialize(), 0));
            }
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            transactions.Clear();
            
            foreach (var PDT in ProtocolPackager.UnPackRaw(data))
            {                
                byte[] tcdata;
                if (ProtocolPackager.UnpackByteVector(PDT, 0, out tcdata))
                {
                    TransactionContent tc = new TransactionContent();
                    tc.Deserialize(tcdata);
                    transactions.Add(tc.TransactionID, tc);
                }
            }
        }
    }
}
