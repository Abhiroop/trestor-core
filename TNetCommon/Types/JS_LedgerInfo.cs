
// @Author : Arpan Jati
// @Date: 13th Feb 2015

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using TNetD.Protocol;

namespace TNetD.Json.JS_Structs
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
            var PDTs = new List<ProtocolDataType>();
            
            PDTs.Add(ProtocolPackager.Pack(Hash, 0));
            PDTs.Add(ProtocolPackager.Pack(SequenceNumber, 1));
            PDTs.Add(ProtocolPackager.Pack(CloseTimeLong, 2));
            PDTs.Add(ProtocolPackager.Pack(Transactions, 3));
            PDTs.Add(ProtocolPackager.Pack(TotalTransactions, 4));
            
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] Data)
        {
            var PDTs = ProtocolPackager.UnPackRaw(Data);
            
            foreach(var PDT in PDTs)
            {
                switch (PDT.NameType)
                {
                    case 0:
                        ProtocolPackager.UnpackByteVector(PDT, 0, out Hash);
                        break;

                    case 1:
                        ProtocolPackager.UnpackInt64(PDT, 1, ref SequenceNumber);
                        break;

                    case 2:
                        ProtocolPackager.UnpackInt64(PDT, 2, ref CloseTimeLong);
                        CloseTime = DateTime.FromFileTimeUtc(CloseTimeLong);
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
