
//  @Author: Arpan Jati | Stephan Verbuecheln
//  @Date: June 2015 | Sept 2015

using TNetD.Protocol;
using System.Collections.Generic;
using Chaos.NaCl;
using System.Collections;

namespace TNetD.Consensus
{
    class Ballot : ISerializableBase, ISignableBase, IEnumerable<Hash>
    {
        public LedgerCloseSequence LedgerCloseSequence;

        public int TransactionCount
        {
            get { return TransactionIds?.Count ?? 0; }
        }

        private SortedSet<Hash> TransactionIds;

        /// <summary>
        /// Public key of the signer.
        /// </summary>
        public Hash PublicKey;

        /// <summary>
        /// Signature of the Ballot
        /// </summary>
        public Hash Signature;

        public long Timestamp;

        // Messed up constructor !
        public Ballot() : this(new LedgerCloseSequence())
        {

        }

        public Ballot(LedgerCloseSequence ledgerCloseSequence)
        {
            TransactionIds = new SortedSet<Hash>();
            Reset();
            LedgerCloseSequence = ledgerCloseSequence;
        }

        /// <summary>
        /// Checks if the ballot contains a transaction.
        /// </summary>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        public bool Contains(Hash transactionID)
        {
            return TransactionIds.Contains(transactionID);
        }

        public bool Add(Hash TransactionID)
        {
            if (!TransactionIds.Contains(TransactionID))
            {
                TransactionIds.Add(TransactionID);
            }

            return false;
        }

        public int AddRange(IEnumerable<Hash> transactionIDs)
        {
            int success = 0;

            foreach(var txid in transactionIDs)
            {
                success += Add(txid) ? 1 : 0;
            }

            return success;
        }

        public void Reset()
        {
            Reset(new LedgerCloseSequence());
        }

        public void Reset(LedgerCloseSequence ledgerCloseSequence)
        {
            TransactionIds.Clear();
            PublicKey = new Hash();
            Signature = new Hash();
            Timestamp = 0;
            LedgerCloseSequence = ledgerCloseSequence;
        }

        #region Serialization

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();

            foreach (Hash txId in TransactionIds)
            {
                PDTs.Add(ProtocolPackager.Pack(txId, 0));
            }

            PDTs.Add(ProtocolPackager.Pack(LedgerCloseSequence.Serialize(), 1));
            PDTs.Add(ProtocolPackager.PackVarint(Timestamp, 2));
            PDTs.Add(ProtocolPackager.Pack(PublicKey, 3));
            PDTs.Add(ProtocolPackager.Pack(Signature, 4));

            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            Reset();

            foreach (var PDT in ProtocolPackager.UnPackRaw(data))
            {
                switch (PDT.NameType)
                {
                    case 0:
                        Hash txID;
                        if (ProtocolPackager.UnpackHash(PDT, 0, out txID))
                        {
                            TransactionIds.Add(txID);
                        }
                        break;

                    case 1:
                        byte[] _data;
                        if (ProtocolPackager.UnpackByteVector(PDT, 1, out _data)) {
                            LedgerCloseSequence.Deserialize(_data);
                        }
                        break;

                    case 2:
                        ProtocolPackager.UnpackVarint(PDT, 2, ref Timestamp);
                        break;

                    case 3:
                        ProtocolPackager.UnpackHash(PDT, 3, out PublicKey);
                        break;

                    case 4:
                        ProtocolPackager.UnpackHash(PDT, 4, out Signature);
                        break;
                }
            }
        }

        #endregion

        #region Signatures

        /// <summary>
        /// Returns data to be signed.
        /// </summary>
        /// <returns></returns>
        public byte[] GetSignatureData()
        {
            List<byte> data = new List<byte>();

            foreach (Hash transaction in TransactionIds)
            {
                data.AddRange(transaction.Hex);
            }

            data.AddRange(Conversions.Int64ToVector(LedgerCloseSequence.Sequence));
            data.AddRange(LedgerCloseSequence.Hash.Hex);
            data.AddRange(Conversions.Int64ToVector(Timestamp));
            data.AddRange(PublicKey.Hex);

            return data.ToArray();
        }

        public void UpdateSignature(byte[] signature)
        {
            this.Signature = new Hash(signature);
        }

        public bool VerifySignature(Hash publicKey)
        {
            if (publicKey.Hex.Length != Common.KEYLEN_PUBLIC) return false;

            if (Signature.Hex.Length != Common.KEYLEN_SIGNATURE) return false;

            return Ed25519.Verify(Signature.Hex, GetSignatureData(), publicKey.Hex);
        }

        public IEnumerator<Hash> GetEnumerator()
        {
            return ((IEnumerable<Hash>)TransactionIds).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Hash>)TransactionIds).GetEnumerator();
        }

        #endregion

    }
}
