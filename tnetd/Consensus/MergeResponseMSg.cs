
//  @Author: Stephan Verbuecheln | Arpan Jati
//  @Date: June 2015 | Sept 2015

using System.Collections;
using System.Collections.Generic;
using TNetD.Protocol;

namespace TNetD.Consensus
{
    /// <summary>
    /// The message can be enumerated to get the transactions.
    /// </summary>
    class MergeResponseMsg : ISerializableBase, IEnumerable<Hash>
    {
        private SortedSet<Hash> transactions;
        
        public LedgerCloseSequence LedgerCloseSequence;

        public MergeResponseMsg()
        {
            transactions = new SortedSet<Hash>();
            LedgerCloseSequence = new LedgerCloseSequence();
        }

        public void AddTransaction(Hash transactionID)
        {
            if(!transactions.Contains(transactionID))
            {
                transactions.Add(transactionID);
            }
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            foreach (Hash transaction in transactions)
            {
                PDTs.Add(ProtocolPackager.Pack(transaction, 0));
            }

            PDTs.Add(ProtocolPackager.Pack(LedgerCloseSequence, 1));

            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            transactions.Clear();
            LedgerCloseSequence = new LedgerCloseSequence();

            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(data);

            foreach (var PDT in PDTs)
            {
                switch (PDT.NameType)
                {
                    case 0:
                        Hash t;
                        if (ProtocolPackager.UnpackHash(PDT, 0, out t))
                        {
                            transactions.Add(t);
                        }
                        break;

                    case 1:
                        byte[] _data = null;
                        if (ProtocolPackager.UnpackByteVector(PDT, 1, ref _data))
                        {
                            LedgerCloseSequence.Deserialize(_data);
                        }

                        break;
                }
            }
        }

        public IEnumerator<Hash> GetEnumerator()
        {
            return ((IEnumerable<Hash>)transactions).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Hash>)transactions).GetEnumerator();
        }
    }
}
