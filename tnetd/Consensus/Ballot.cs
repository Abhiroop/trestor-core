using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.Consensus
{
    class Ballot : ISerializableBase
    {
        SortedSet<Hash> TransactionIds = new SortedSet<Hash>();
               
        /// <summary>
        /// Public key of the signer.
        /// </summary>
        public Hash PublicKey;

        /// <summary>
        /// Signature of the Ballot
        /// </summary>
        public Hash Signature;

        public long Timestamp;

        public bool Add(Hash TransactionID)
        {
            if (!TransactionIds.Contains(TransactionID))
            {
                TransactionIds.Add(TransactionID);
            }

            return false;
        }

        public Ballot()
        {

        }

        public byte [] Serialize()
        {
            return new byte[0];
        }

        public void Deserialize(byte[] data)
        {

        }


    }
}
