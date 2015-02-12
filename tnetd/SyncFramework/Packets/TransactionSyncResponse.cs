
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
    class TransactionSyncResponse : ISerializableBase
    {
        public List<TransactionContentSet> TransactionContents = new List<TransactionContentSet>();

        public TransactionSyncResponse()
        {

        }

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[TransactionContents.Count];

            int cnt = 0;

            foreach (TransactionContentSet ts in TransactionContents)
            {
                PDTs[cnt++] = (ProtocolPackager.Pack(ts.Serialize(), 0));
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
                            byte[] tempSource = new byte[0];
                            ProtocolPackager.UnpackByteVector(PDT, 0, ref tempSource);
                            if (tempSource.Length > 0)
                            {
                                TransactionContentSet tsk = new TransactionContentSet();
                                tsk.Deserialize(tempSource);
                                TransactionContents.Add(tsk);
                            }
                        }
                        break;
                }
            }
        }
    }
}
