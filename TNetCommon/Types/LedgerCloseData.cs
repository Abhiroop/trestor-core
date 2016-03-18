
// @Author : Arpan Jati
// @Date: March 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD
{
    public class LedgerCloseData : ISerializableBase
    {
        // DBUtils.ExecuteNonQuery("CREATE TABLE LedgerInfo (SequenceNumber INTEGER PRIMARY KEY AUTOINCREMENT, 
        //         LedgerHash BLOB, Transactions INTEGER, CloseTime INTEGER);", sqliteConnection);

        public long SequenceNumber;
        public byte[] LedgerHash;
        public long Transactions;
        public long TotalTransactions;
        public long CloseTime;

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[5];

            int cnt = 0;

            PDTs[cnt++] = (ProtocolPackager.Pack(SequenceNumber, 0));
            PDTs[cnt++] = (ProtocolPackager.Pack(LedgerHash, 1));
            PDTs[cnt++] = (ProtocolPackager.Pack(Transactions, 2));
            PDTs[cnt++] = (ProtocolPackager.Pack(TotalTransactions, 3));
            PDTs[cnt++] = (ProtocolPackager.Pack(CloseTime, 4));

            if (cnt != PDTs.Length) throw new Exception("Invalid pack entries");

            return ProtocolPackager.PackRaw(PDTs);
        }
        
        public void Deserialize(byte[] data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(data);
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
                            ProtocolPackager.UnpackByteVector(PDT, 1, out LedgerHash);
                        }
                        break;

                    case 2:
                        {
                            ProtocolPackager.UnpackInt64(PDT, 2, ref Transactions);
                        }
                        break;

                    case 3:
                        {
                            ProtocolPackager.UnpackInt64(PDT, 3, ref TotalTransactions);
                        }
                        break;

                    case 4:
                        {
                            ProtocolPackager.UnpackInt64(PDT, 4, ref CloseTime);
                        }
                        break;
                }
            }
        }
        
    }
}
