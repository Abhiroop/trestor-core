
// @Author : Arpan Jati
// @Date: 23th Dec 2014 | 12 Jan 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;
using TNetD;

namespace TNetD.Transactions
{
    public class TransactionEntity : ISerializableBase
    {
        byte[] _publicKey;
        long _value;

        public byte[] PublicKey 
        {
            get { return _publicKey; }
            set { _publicKey = value;  }
        }

        public long Value 
        {
            get { return _value; }
            set { _value = value; }
        }

        public TransactionEntity()
        {
            _publicKey = new byte[0];
            _value = 0;
        }

        public TransactionEntity(byte[] PublicKey, long Amount)
        {
            this._publicKey = PublicKey;
            this._value = Amount;
        }

        public byte[] Serialize()
        {
            ProtocolDataType [] PDTs = new ProtocolDataType[2];
            PDTs[0] = (ProtocolPackager.Pack(_publicKey, 0));
            PDTs[1] = (ProtocolPackager.Pack(_value, 1));
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] Data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);
            int cnt = 0;

            while (cnt < (int)PDTs.Count)
            {
                ProtocolDataType PDT = PDTs[cnt++];

                switch (PDT.NameType)
                {
                    case 0:
                        ProtocolPackager.UnpackByteVector_s(PDT, 0, Common.KEYLEN_PUBLIC, ref _publicKey);
                        break;

                    case 1:
                        ProtocolPackager.UnpackInt64(PDT, 1, ref _value);
                        break;
                }
            }

        }
    }
}
