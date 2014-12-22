
// @Author : Arpan Jati
// @Date: 23th Dec 2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;
using TNetD;

namespace TNetD.Transactions
{
    public class TransactionEntity : SerializableBase
    {
        public byte[] PublicKey;
        public long Amount;

        public TransactionEntity()
        {
            PublicKey = new byte[0];
            Amount = 0;
        }

        public TransactionEntity(byte[] PublicKey, long Amount)
        {
            this.PublicKey = PublicKey;
            this.Amount = Amount;
        }

        public override byte[] Serialize()
        {
            ProtocolDataType [] PDTs = new ProtocolDataType[2];
            PDTs[0] = (ProtocolPackager.Pack(PublicKey, 0));
            PDTs[1] =(ProtocolPackager.Pack(Amount, 1));
            return ProtocolPackager.PackRaw(PDTs);
        }

        public override void Deserialize(byte[] Data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);
            int cnt = 0;

            while (cnt < (int)PDTs.Count)
            {
                ProtocolDataType PDT = PDTs[cnt++];

                switch (PDT.NameType)
                {
                    case 0:
                        ProtocolPackager.UnpackByteVector_s(PDT, 0, Common.KEYLEN_PUBLIC, ref PublicKey);
                        break;

                    case 1:
                        ProtocolPackager.UnpackInt64(PDT, 1, ref Amount);
                        break;
                }
            }

        }
    }
}
