
// @Author : Arpan Jati
// @Date: 12th Feb 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;
using TNetD.Transactions;

namespace TNetD.SyncFramework.Packets
{
    class TransactionContentSet : ISerializableBase
    {
        public long SequenceNumber;
        public List<TransactionContent> TxContent = new List<TransactionContent>();

        public TransactionContentSet(long sequenceNumber, TransactionContent transactionContent)
        {
            TxContent.Add(transactionContent);
            SequenceNumber = sequenceNumber;
        }

        public TransactionContentSet(long sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
        }

        public TransactionContentSet()
        {
            SequenceNumber = 0;
        }

        public void Add(TransactionContent transactionContent)
        {
            TxContent.Add(transactionContent);
        }

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[TxContent.Count];

            int cnt = 0;

            PDTs[cnt++] = (ProtocolPackager.Pack(SequenceNumber, 0));

            foreach (TransactionContent ts in TxContent)
            {
                PDTs[cnt++] = (ProtocolPackager.Pack(ts.Serialize(), 1));
            }

            if (cnt != PDTs.Length) throw new Exception("Invalid pack entries");

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
                        {
                            ProtocolPackager.UnpackInt64(PDT, 0, ref SequenceNumber);
                        }
                        break;

                    case 1:
                        {
                            byte[] tempSource = new byte[0];
                            ProtocolPackager.UnpackByteVector(PDT, 1, ref tempSource);
                            if (tempSource.Length > 0)
                            {
                                TransactionContent tsk = new TransactionContent();
                                tsk.Deserialize(tempSource);
                                TxContent.Add(tsk);
                            }
                        }
                        break;
                }
            }
        }
    }
}

