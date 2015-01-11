
// @Author : Arpan Jati
// @Date: 23th Dec 2014 | 12th Jan 2015

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
    /// 
    /// [SOURCES = DEST + FEES]
    /// 
    /// </summary>
    public class TransactionContent : ISerializableBase
    {
        Hash intTransactionID;

        // TODO: Make these PROPERTIES
        public List<TransactionEntity> Sources { get; set; }

        public List<TransactionEntity> Destinations { get; set; }

        private long timestamp;

        public List<Hash> Signatures { get; set; }

        /// <summary>
        /// Transaction Fees.
        /// </summary>
        private long transactionFee;

        /// <summary>
        /// This is the Transaction ID. Unique identifier for a transaction.
        /// </summary>
        /// <returns></returns>
        public Hash TransactionID
        {
            get { return intTransactionID; }
        }

        public DateTime DateTime
        {
            get { return DateTime.FromFileTimeUtc(timestamp); }
        }

        public long Timestamp
        {
            get { return timestamp; }
        }

        public long TransactionFee
        {
            get { return transactionFee; }
        }

        public long Value
        {
            get 
            {
                long val = 0;
                foreach(TransactionEntity te in Sources)
                {
                    val += te.Value;
                }
                return val;
            }
        }
              
        void Init()
        {
            Sources = new List<TransactionEntity>();
            Destinations = new List<TransactionEntity>();
            Signatures = new List<Hash>();
            timestamp = 0;
            transactionFee = 0;

            intTransactionID = new Hash();
        }

        public TransactionContent(TransactionEntity[] Sources, TransactionEntity[] Destinations, long TransactionFee, List<Hash> Signatures, long Timestamp)
        {
            this.Destinations = Destinations.ToList();
            this.timestamp = Timestamp;
            this.Sources = Sources.ToList();
            this.Signatures = Signatures;
            this.transactionFee = TransactionFee;

            UpdateIntHash();
        }

        public TransactionContent(TransactionEntity[] Sources, TransactionEntity[] Destinations, long TransactionFee, long Timestamp)
        {
            this.Destinations = Destinations.ToList();            
            this.Sources = Sources.ToList();
            this.transactionFee = TransactionFee;
            this.timestamp = Timestamp;
        }
        
        /// <summary>
        /// Manually set the signatures. This also updates the TransactionID after hashing.
        /// </summary>
        /// <param name="Signatures"></param>
        public void SetSignatures(List<Hash> Signatures)
        {
            this.Signatures = Signatures;

            UpdateIntHash();
        }

        public TransactionContent()
        {
            Init();
        }

        /// <summary>
        /// First byte is transaction Version. 0 for initial version
        /// Second byte is transaction Type. 0 for initial version.
        /// Rest 6 bytes are reserved. 0 for initial version.
        /// </summary>
        byte[] ConfigData = { 0, 0, 0, 0, 0, 0, 0, 0 };

        /// <summary>
        /// Returns the transaction data which needs to be signed by
        /// all the individual sources.
        /// </summary>
        /// <returns></returns>
        public byte[] GetTransactionData()
        {
            List<byte> transactionData = new List<byte>();

            // Adding configuration Data
            transactionData.AddRange(ConfigData);

            // Adding Sources
            for (int i = 0; i < (int)Sources.Count; i++)
            {
                TransactionEntity ts = Sources[i];
                transactionData.AddRange(ts.PublicKey);
                transactionData.AddRange(Conversions.Int64ToVector(ts.Value));
            }

            // Adding Destinations
            for (int i = 0; i < (int)Destinations.Count; i++)
            {
                TransactionEntity td = Destinations[i];
                transactionData.AddRange(td.PublicKey);
                transactionData.AddRange(Conversions.Int64ToVector(td.Value));
            }

            // Adding Fee
            transactionData.AddRange(Conversions.Int64ToVector(transactionFee));

            // Adding Timestamp
            transactionData.AddRange(Conversions.Int64ToVector(timestamp));

            return transactionData.ToArray();
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
                if (Sources[i].Value <= 0)
                    return false;

                incoming += Sources[i].Value;
            }

            for (int i = 0; i < (int)Destinations.Count; i++)
            {
                if (Destinations[i].Value <= 0)
                    return false;

                outgoing += Destinations[i].Value;
            }

            outgoing += transactionFee;

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

                bool good = Ed25519.Verify(Signatures[i].Hex, transactionData, Sources[i].PublicKey);

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

            List<byte> tranDataSig = new List<byte>();

            byte[] tranData = GetTransactionData();
            tranDataSig.AddRange(tranData);

            foreach (Hash sig in Signatures)
            {
                tranDataSig.AddRange(sig.Hex);
            }

            return tranDataSig.ToArray();
        }

        private void UpdateIntHash()
        {
            byte[] tranDataSig = GetTransactionDataAndSignature();
            byte[] output = (new SHA512Managed()).ComputeHash(tranDataSig).Take(32).ToArray();

            intTransactionID = new Hash(output);
        }

        
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

        ///////////////////////////////////////////////////////////

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[3 + Sources.Count + Destinations.Count + Signatures.Count];

            int cnt = 0;

            PDTs[cnt++] = (ProtocolPackager.Pack(ConfigData, 0));

            PDTs[cnt++] = (ProtocolPackager.Pack(timestamp, 1));

            foreach (TransactionEntity ts in Sources)
            {
                PDTs[cnt++] = (ProtocolPackager.Pack(ts.Serialize(), 2));
            }

            foreach (TransactionEntity td in Destinations)
            {
                PDTs[cnt++] = (ProtocolPackager.Pack(td.Serialize(), 3));
            }

            PDTs[cnt++] = (ProtocolPackager.Pack(transactionFee, 4));

            foreach (Hash te in Signatures)
            {
                PDTs[cnt++] = (ProtocolPackager.Pack(te, 5));
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
                        {
                            ProtocolPackager.UnpackByteVector(PDT, 0, ref ConfigData);
                        }
                        break;

                    case 1:
                        {
                            ProtocolPackager.UnpackInt64(PDT, 1, ref timestamp);
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
                                Sources.Add(tsk);
                            }
                        }
                        break;

                    case 3:
                        {
                            byte[] tempVector = new byte[0];
                            ProtocolPackager.UnpackByteVector(PDT, 3, ref tempVector);
                            if (tempVector.Length > 0)
                            {
                                TransactionEntity tsk = new TransactionEntity();
                                tsk.Deserialize(tempVector);
                                Destinations.Add(tsk);
                            }
                        }
                        break;

                    case 4:
                        {
                            ProtocolPackager.UnpackInt64(PDT, 4, ref transactionFee);
                        }
                        break;

                    case 5:
                        {
                            Hash _sig;
                            ProtocolPackager.UnpackHash(PDT, 5, out _sig);
                            if (_sig.Hex.Length > 0)
                            {
                                Signatures.Add(_sig);
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

