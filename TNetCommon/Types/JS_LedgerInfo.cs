
// @Author : Arpan Jati
// @Date: 13th Feb 2015

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Json.JS_Structs;
using TNetD.Protocol;

namespace TNetD.Types
{
    public class JS_LedgerInfo : JS_Response, ISerializableBase
    {
        public byte[] Hash;
        public long SequenceNumber = 0;
        public DateTime CloseTime = DateTime.UtcNow;

        [JsonIgnore]
        public long CloseTimeLong = 0;
        public long Transactions = 0;
        public long TotalTransactions = 0;

        public JS_LedgerInfo()
        {
            Hash = new byte[0];
        }

        public JS_LedgerInfo(LedgerCloseData ledgerCloseData)
        {
            Hash = ledgerCloseData.LedgerHash;
            TotalTransactions = ledgerCloseData.TotalTransactions;
            Transactions = ledgerCloseData.Transactions;
            CloseTimeLong = ledgerCloseData.CloseTime;
            CloseTime = DateTime.FromFileTimeUtc(ledgerCloseData.CloseTime);
            SequenceNumber = ledgerCloseData.SequenceNumber;
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[5];

            int cnt = 0;

            PDTs[cnt++] = ProtocolPackager.Pack(Hash, 0);
            PDTs[cnt++] = ProtocolPackager.Pack(SequenceNumber, 1);
            PDTs[cnt++] = ProtocolPackager.Pack(CloseTimeLong, 2);
            PDTs[cnt++] = ProtocolPackager.Pack(Transactions, 3);
            PDTs[cnt++] = ProtocolPackager.Pack(TotalTransactions, 4);

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
                        ProtocolPackager.UnpackByteVector(PDT, 0, ref Hash);
                        break;

                    case 1:
                        ProtocolPackager.UnpackInt64(PDT, 1, ref SequenceNumber);
                        break;

                    case 2:
                        ProtocolPackager.UnpackInt64(PDT, 2, ref CloseTimeLong);
                        break;

                    case 3:
                        ProtocolPackager.UnpackInt64(PDT, 3, ref Transactions);
                        break;

                    case 4:
                        ProtocolPackager.UnpackInt64(PDT, 4, ref TotalTransactions);
                        break;
                }
            }
        }
    }

}
