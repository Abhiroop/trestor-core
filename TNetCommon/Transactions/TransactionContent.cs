
// @Author : Arpan Jati
// @Date: 23th Dec 2014

using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.Transactions
{
    /// <summary>
    /// A single transaction, contains all the sources and destinations.
    /// It is important that all the signees agree to all the transactions in the request.
    /// Summation of Amount in sources and destinations must be a non-zero exact match.
    /// </summary>
    public class TransactionContent : ISerializableBase
    {
        Hash intTransactionID;

        // TODO: Make these PROPERTIES
        public List<TransactionEntity> Sources;
        public List<TransactionEntity> Destinations;
        public List<byte[]> Signatures;
        public long Timestamp;

        void Init()
        {
            Sources = new List<TransactionEntity>();
            Destinations = new List<TransactionEntity>();
            Signatures = new List<byte[]>();
            Timestamp = 0;

            intTransactionID = new Hash();
        }
        
        public TransactionContent(TransactionEntity[] Sources, long Timestamp, TransactionEntity[] Destinations, List<byte[]> Signatures)
        {
            this.Destinations = Destinations.ToList();
            this.Timestamp = Timestamp;
            this.Sources = Sources.ToList();
            this.Signatures = Signatures;

            UpdateIntHash();
        }

        public TransactionContent(TransactionEntity[] Sources, long Timestamp, TransactionEntity[] Destinations)
        {
            this.Destinations = Destinations.ToList();
            this.Timestamp = Timestamp;
            this.Sources = Sources.ToList();
        }

        /// <summary>
        /// This is the Transaction ID. Unique identifier for a transaction.
        /// </summary>
        /// <returns></returns>
        public Hash TransactionID
        {
            get { return intTransactionID; }
        }

        /// <summary>
        /// Manually set the signatures. This also updates the TransactionID after hashing.
        /// </summary>
        /// <param name="Signatures"></param>
        public void SetSignatures(List<byte[]> Signatures)
        {
            this.Signatures = Signatures;

            UpdateIntHash();
        }

        public TransactionContent()
        {
            Init();
        }

        byte[] ConfigData = { 0, 0, 0, 0, 0, 0, 0, 0 };

        /// <summary>
        /// Returns the transaction data which needs to be signed by
        /// all the individual sources.
        /// </summary>
        /// <returns></returns>
        public byte[] GetTransactionData()
        {
            List<byte> _data = new List<byte>();

            // Adding configuration Data

            _data.AddRange(ConfigData);

            // Adding Sources
            for (int i = 0; i < (int)Sources.Count; i++)
            {
                TransactionEntity ts = Sources[i];
                _data.AddRange(ts.PublicKey);
                _data.AddRange(Conversions.Int64ToVector(ts.Amount));
            }

            // Adding Destinations
            for (int i = 0; i < (int)Destinations.Count; i++)
            {
                TransactionEntity td = Destinations[i];
                _data.AddRange(td.PublicKey);
                _data.AddRange(Conversions.Int64ToVector(td.Amount));
            }

            // Adding Timestamp
            _data.AddRange(Conversions.Int64ToVector(Timestamp));

            return _data.ToArray();
        }

        /*void TransactionContent.UpdateAndSignContent(vector<TransactionEntity> _Sources, int64_t _Timestamp, vector<TransactionEntity> _Destinations, vector<Hash> _ExpandedPrivateKeys)
        {
        Destinations = _Destinations;
        Timestamp = _Timestamp;
        Sources = _Sources;

        byte temp_signature[64];
        Hash getTranData = GetTransactionData();
        ed25519_sign(temp_signature, getTranData.data(), getTranData.size(), PublicKey_Source.data(), _ExpandedPrivateKeys.data());
        Signature = Hash(temp_signature, temp_signature + 64);

        UpdateIntHash();
        }*/

        bool IsSource(Hash SourcePublicKey)
        {
            for (int i = 0; i < (int)Sources.Count; i++)
            {
                TransactionEntity TE = Sources[i];

                if (TE.PublicKey == SourcePublicKey.Hex)
                    return true;
            }

            return false;
        }

        bool IsDestination(Hash DestinationPublicKey)
        {
            for (int i = 0; i < (int)Destinations.Count; i++)
            {
                TransactionEntity TE = Destinations[i];

                if (TE.PublicKey == DestinationPublicKey.Hex)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks the general integrity of the transaction. 
        /// Does not gurantee that the signatures are valid.
        /// </summary>
        /// <returns></returns>
        public bool IntegrityCheck()
        {
            long incoming = 0;
            long outgoing = 0;

            if (Sources.Count != Signatures.Count)
                return false;

            for (int i = 0; i < (int)Sources.Count; i++)
            {
                if (Sources[i].Amount <= 0)
                    return false;

                incoming += Sources[i].Amount;
            }

            for (int i = 0; i < (int)Destinations.Count; i++)
            {
                if (Destinations[i].Amount <= 0)
                    return false;

                outgoing += Destinations[i].Amount;
            }

            if ((incoming == outgoing) &&
                (Sources.Count > 0) &&
                (Destinations.Count > 0))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Verifies all the signatures for all the sources.
        /// </summary>
        /// <returns></returns>
        public bool VerifySignature()
        {
            if (!IntegrityCheck())
            {
                return false;
            }

            byte[] transactionData = GetTransactionData();

            // Adding Sources

            int PassedSignatures = 0;

            for (int i = 0; i < (int)Sources.Count; i++)
            {
                TransactionEntity ts = Sources[i];

                bool good = Ed25519.Verify(Signatures[i], transactionData, Sources[i].PublicKey);

                if (good)
                {
                    PassedSignatures++;
                }
                else
                {
                    return false;
                }
            }

            if (PassedSignatures == Sources.Count)
            {
                return true;
            }


            return false;
        }

        byte[] GetTransactionDataAndSignature()
        {
            // Data Hash Format : [Config Data][Sources][Destinations][Timestamp][Signatures]

            List<byte> _data = new List<byte>();

            byte[] tranData = GetTransactionData();
            _data.AddRange(tranData);

            foreach (byte[] sig in Signatures)
            {
                _data.AddRange(sig);
            }

            return _data.ToArray();
        }

        private void UpdateIntHash()
        {
            byte[] tranDataSig = GetTransactionDataAndSignature();
            byte[] output = (new SHA512Managed()).ComputeHash(tranDataSig).Take(32).ToArray();

            intTransactionID = new Hash(output);
        }

        ///////////////////////////////////////////////////////////

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[1 + Sources.Count + Destinations.Count + Signatures.Count];

            int cnt = 0;

            PDTs[cnt++] = (ProtocolPackager.Pack(Timestamp, 0));

            foreach (TransactionEntity ts in Sources)
            {
                PDTs[cnt++] = (ProtocolPackager.Pack(ts.Serialize(), 1));
            }

            foreach (TransactionEntity td in Destinations)
            {
                PDTs[cnt++] = (ProtocolPackager.Pack(td.Serialize(), 2));
            }

            foreach (byte[] te in Signatures)
            {
                PDTs[cnt++] = (ProtocolPackager.Pack(te, 3));
            }

            if (cnt != PDTs.Length) throw new Exception("Invalid pack entries");

            return ProtocolPackager.PackRaw(PDTs);
        }

        /////////////////////////

        public void Deserialize(byte[] Data)
        {
            Init();

            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);
            int cnt = 0;

            while (cnt < (int)PDTs.Count)
            {
                ProtocolDataType PDT = PDTs[cnt++];

                switch (PDT.NameType)
                {
                    case 0:

                        ProtocolPackager.UnpackInt64(PDT, 0, ref Timestamp);

                        break;

                    case 1:
                        {
                            byte[] tempVector = new byte[0];
                            ProtocolPackager.UnpackByteVector(PDT, 1, ref tempVector);
                            if (tempVector.Length > 0)
                            {
                                TransactionEntity tsk = new TransactionEntity();
                                tsk.Deserialize(tempVector);
                                Sources.Add(tsk);
                            }
                        }

                        break;

                    case 2:
                        {
                            byte[] tempVector = new byte[0];
                            ProtocolPackager.UnpackByteVector(PDT, 2, ref tempVector);
                            if (tempVector.Length > 0)
                            {
                                TransactionEntity tsk = new TransactionEntity();
                                tsk.Deserialize(tempVector);
                                Destinations.Add(tsk);
                            }
                        }

                        break;

                    case 3:
                        {
                            byte[] tempVector = new byte[0];
                            ProtocolPackager.UnpackByteVector(PDT, 3, ref tempVector);
                            if (tempVector.Length > 0)
                            {
                                Signatures.Add(tempVector);
                            }
                        }

                        break;
                }
            }

            // Update the internal Hash of the object.
            UpdateIntHash();
        }

        


       

    }
}
