using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.Transaction
{
    public class TransactionEntity
    {
        public byte[] PublicKey_Sink;
        public long Amount;

        public TransactionEntity(byte[] PublicKey_Sink, long Amount)
        {
            this.PublicKey_Sink = PublicKey_Sink;
            this.Amount = Amount;
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
	        PDTs.Add(ProtocolPackager.Pack(PublicKey_Sink, 0));
	        PDTs.Add(ProtocolPackager.Pack(Amount, 1));
	        return ProtocolPackager.PackRaw(PDTs);
        }
    }
}
