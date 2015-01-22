
// @Author : Arpan Jati
// @Date: 23th Dec 2014 | 12th Jan 2015 | 16th Jan 2015 | 20th Jan 2015 | 22nd Jan 2015 

using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TNetD.Json.JS_Structs;
using TNetD.Protocol;

namespace TNetD.Transactions
{
    public enum TransactionStatusType { Proposed, InProcessingQueue, Processed, VoteInProgress, Success, Failure };

    public enum TransactionProcessingResult
    {
        Unprocessed, Accepted, InsufficientFunds, SourceSinkValueMismatch, SignatureInvalid,
        InsufficientSignatureCount, InsufficientFees, InvalidTime, SourceDestinationTypeMismatch,
        NoProperSources, NoProperDestinations, InvalidVersion, InvalidExecutionData, 
        /// <summary>
        /// The Source entity is also present as one of the Destinations.
        /// </summary>
        SourceInDestination
    };

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

        long timeStamp;
        long transactionFee = 0;
        byte[] versionData = { 0, 0 };
        byte[] executionData = { };

        /// <summary>
        /// First byte is transaction Version. 0 for initial version.
        /// Second byte is transaction Type. 0 for standard transaction.
        /// </summary>
        public byte[] VersionData
        {
            get { return versionData; }
        }

        /// <summary>
        /// This field proides data using which the transaction is processed. 
        /// The interpretation of the execution depends on this field.
        /// </summary>
        public byte[] ExecutionData
        {
            get { return executionData; }
        }

        public List<TransactionEntity> Sources { get; set; }

        public List<TransactionEntity> Destinations { get; set; }        

        public List<Hash> Signatures { get; set; }        

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
            get { return DateTime.FromFileTimeUtc(timeStamp); }
        }

        public long Timestamp
        {
            get { return timeStamp; }
        }

        /// <summary>
        /// Transaction Fees.
        /// </summary>
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
            timeStamp = 0;
            transactionFee = 0;

            intTransactionID = new Hash();
        }

        public TransactionContent(TransactionEntity[] Sources, TransactionEntity[] Destinations, long TransactionFee, 
            List<Hash> Signatures, long Timestamp)
        {
            this.Destinations = Destinations.ToList();
            this.timeStamp = Timestamp;
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
            this.timeStamp = Timestamp;
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
        /// Returns the transaction data which needs to be signed by
        /// all the individual sources.
        /// </summary>
        /// <returns></returns>
        public byte[] GetTransactionData()
        {
            List<byte> transactionData = new List<byte>();

            // Adding version Data
            transactionData.AddRange(versionData);

            // Adding Execution Data
            transactionData.AddRange(executionData);

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
            transactionData.AddRange(Conversions.Int64ToVector(timeStamp));

            return transactionData.ToArray();
        }
               

        /// <summary>
        /// Checks the general integrity of the transaction. 
        /// Does not gurantee that the signatures are valid.
        /// </summary>
        /// <returns></returns>
        public TransactionProcessingResult IntegrityCheck()
        {
            long incoming = 0;
            long outgoing = 0;

            // TODO: MxN complexity, fix with loop limit or Dictionary.
            foreach(TransactionEntity src in Sources)
            {
                foreach (TransactionEntity dst in Destinations)
                {
                    if (src.Address == dst.Address)
                        return TransactionProcessingResult.SourceInDestination;
                }
            }

            if (Sources.Count != Signatures.Count)
                return TransactionProcessingResult.InsufficientSignatureCount;

            if(transactionFee < Common.NETWORK_Min_Transaction_Fee)
            {
                return TransactionProcessingResult.InsufficientFees;
            }

            for (int i = 0; i < (int)Sources.Count; i++)
            {
                if (Sources[i].Value <= 0)
                    return TransactionProcessingResult.NoProperSources;

                incoming += Sources[i].Value;
            }

            for (int i = 0; i < (int)Destinations.Count; i++)
            {
                if (Destinations[i].Value <= 0)
                    return TransactionProcessingResult.NoProperDestinations;

                outgoing += Destinations[i].Value;
            }

            outgoing += transactionFee;

            if ((incoming == outgoing) &&
                (Sources.Count > 0) &&
                (Destinations.Count > 0))
            {
                return TransactionProcessingResult.Accepted;
            }
            else
            {
                return TransactionProcessingResult.InsufficientFunds;
            }

        }

        /// <summary>
        /// Verifies all the signatures for all the sources.
        /// </summary>
        /// <returns></returns>
        public TransactionProcessingResult VerifySignature()
        {
            TransactionProcessingResult tp_result = IntegrityCheck();

            if (tp_result != TransactionProcessingResult.Accepted)
            {
                return tp_result;
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
                    return TransactionProcessingResult.SignatureInvalid;
                }
            }

            if (PassedSignatures == Sources.Count)
            {
                return TransactionProcessingResult.Accepted;
            }
            else
            {
                return TransactionProcessingResult.InsufficientSignatureCount; // Kindof redundant / #THINK
            }            
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
            byte[] output = (new SHA512Cng()).ComputeHash(tranDataSig).Take(32).ToArray();

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
            ProtocolDataType[] PDTs = new ProtocolDataType[4 + Sources.Count + Destinations.Count + Signatures.Count];

            int cnt = 0;

            PDTs[cnt++] = (ProtocolPackager.Pack(versionData, 0));

            PDTs[cnt++] = (ProtocolPackager.Pack(executionData, 1));

            PDTs[cnt++] = (ProtocolPackager.Pack(timeStamp, 2));

            foreach (TransactionEntity ts in Sources)
            {
                PDTs[cnt++] = (ProtocolPackager.Pack(ts.Serialize(), 3));
            }

            foreach (TransactionEntity td in Destinations)
            {
                PDTs[cnt++] = (ProtocolPackager.Pack(td.Serialize(), 4));
            }

            PDTs[cnt++] = (ProtocolPackager.Pack(transactionFee, 5));

            foreach (Hash te in Signatures)
            {
                PDTs[cnt++] = (ProtocolPackager.Pack(te, 6));
            }            

            if (cnt != PDTs.Length) throw new Exception("Invalid pack entries");

            return ProtocolPackager.PackRaw(PDTs);
        }

        /////////////////////////

        public bool Deserialize(JS_TransactionReply Data)
        {
            Init();

            Destinations = Data.Destinations;
            executionData =  Data.ExecutionData;
            Signatures = (from sig in Data.Signatures select new Hash(sig)).ToList();

            Sources = Data.Sources;
            timeStamp = Data.Timestamp;
            transactionFee = Data.TransactionFee;

            versionData = Data.VersionData;

            if(this.Value != Value)
            {

                return false;
                //throw new ArgumentException("JSON Deserialization: Invalid Total Value");
            }

            UpdateIntHash();

            if (!Data.TransactionID.ByteArrayEquals(TransactionID.Hex))
            {
                return false;
                //throw new ArgumentException("JSON Deserialization: Transaction ID Mismatch");
            }

            return true;
        }

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
                            ProtocolPackager.UnpackByteVector(PDT, 0, ref versionData);
                        }
                        break;

                    case 1:
                        {
                            ProtocolPackager.UnpackByteVector(PDT, 1, ref executionData);
                        }
                        break;

                    case 2:
                        {
                            ProtocolPackager.UnpackInt64(PDT, 2, ref timeStamp);
                        }
                        break;

                    case 3:
                        {
                            byte[] tempSource = new byte[0];
                            ProtocolPackager.UnpackByteVector(PDT, 3, ref tempSource);
                            if (tempSource.Length > 0)
                            {
                                TransactionEntity tsk = new TransactionEntity();
                                tsk.Deserialize(tempSource);
                                Sources.Add(tsk);
                            }
                        }
                        break;

                    case 4:
                        {
                            byte[] tempDestination = new byte[0];
                            ProtocolPackager.UnpackByteVector(PDT, 4, ref tempDestination);
                            if (tempDestination.Length > 0)
                            {
                                TransactionEntity tsk = new TransactionEntity();
                                tsk.Deserialize(tempDestination);
                                Destinations.Add(tsk);
                            }
                        }
                        break;

                    case 5:
                        {
                            ProtocolPackager.UnpackInt64(PDT, 5, ref transactionFee);
                        }
                        break;

                    case 6:
                        {
                            Hash tempSignature;
                            ProtocolPackager.UnpackHash(PDT, 6, out tempSignature);
                            if (tempSignature.Hex.Length > 0)
                            {
                                Signatures.Add(tempSignature);
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

