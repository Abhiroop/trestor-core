
// @Author : Arpan Jati
// @Date: 21th Feb 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.SyncFramework.Packets
{
    class TransactionSyncQueryResponse : ISerializableBase
    {
        /// <summary>
        /// LedgerSequence
        /// </summary>
        public long LedgerSequence;

        /// <summary>
        /// TransactionCount
        /// </summary>
        public long TransactionCount;

        public TransactionSyncQueryResponse()
        {
            LedgerSequence = 0;
            TransactionCount = 0;
        }

        public TransactionSyncQueryResponse(long ledgerSequence, long transactionCount)
        {
            LedgerSequence = ledgerSequence;
            TransactionCount = transactionCount;
        }

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[2];

            int cnt = 0;

            PDTs[cnt++] = (ProtocolPackager.Pack(LedgerSequence, 0));

            PDTs[cnt++] = (ProtocolPackager.Pack(TransactionCount, 1));

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
                            ProtocolPackager.UnpackInt64(PDT, 0, ref LedgerSequence);
                        }
                        break;

                    case 1:
                        {
                            ProtocolPackager.UnpackInt64(PDT, 1, ref TransactionCount);
                        }
                        break;
                }
            }
        }

    }
}
