/*
 *  @Author: Arpan Jati
 *  @Version: 1.0
 *  @Date: 9 Jan 2015 | 10 Jan 2015 | 14 Jan 2015 | 1 Feb 2015
 *  @Description: Json Structs for serialization
 */

using Newtonsoft.Json;
//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Address;
using TNetD.Transactions;
using TNetD.Types;

namespace TNetD.Json.JS_Structs
{
    public enum RPCStatus { Failure, Success, Exception, ServerNotReady, ServerBusy, InvalidAPIUsage, Undefined }

    public class JS_Resp
    {
        public RPCStatus Status = RPCStatus.Failure;
        public JS_Response Data;

        public JS_Resp()
        {

        }

        public JS_Resp(RPCStatus status, JS_Response Data)
        {
            this.Status = status;
            this.Data = Data;
        }
    }

    public interface JS_Response
    {
        JS_Resp GetResponse();
    }

    public class JS_Msg : JS_Response
    {
        public string Message;
        private RPCStatus status = RPCStatus.Failure;

        public JS_Msg(string Message, RPCStatus status)
        {
            this.Message = Message;
            this.status = status;
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(status, this);
        }
    }

    public class JS_Address : JS_Response
    {
        public AccountIdentifier AccountIdentifier;
        public byte[] Secret;

        public JS_Address(AccountIdentifier AccountIdentifier, byte[] Secret)
        {
            this.AccountIdentifier = AccountIdentifier;
            this.Secret = Secret;
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }
    }

    public class JS_NodeInfo : JS_Response
    {
        public byte[] PublicKey;
        public string Name;
        public string Address;
        public string Organisation;
        public string Platform;
        public string Email;
        public string Version;
        public JS_NodeDetails NodeDetails;
        public JS_LedgerInfo LastLedgerInfo;

        public JS_NodeInfo()
        {
            this.NodeDetails = new JS_NodeDetails();
            this.LastLedgerInfo = new JS_LedgerInfo();
        }

        public JS_NodeInfo(JS_NodeDetails NodeDetails, JS_LedgerInfo LastLedgerInfo)
        {
            this.NodeDetails = NodeDetails;
            this.LastLedgerInfo = LastLedgerInfo;
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }
    }

    public class JS_NodeDetails : JS_Response
    {
        [JsonIgnore]
        public int ConnectedPeers = 1;
        public int TransactionsProcessed = 0;
        public int TransactionsAccepted = 0;
        public int TransactionsValidated = 0;
        public int RequestsProcessed = 0;
        public int LoadLevel = 1;
        public DateTime TimeUTC = DateTime.UtcNow;

        public JS_NodeDetails()
        {

        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }
    }

    public class JS_LedgerInfo : JS_Response
    {
        public byte[] Hash;
        public int Index = 0;
        public DateTime CloseTime = DateTime.UtcNow;

        public JS_LedgerInfo()
        {
            Hash = new byte[0];
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }
    }

    public class JS_TransactionReply : JS_Response
    {
        public byte[] VersionData;
        public byte[] ExecutionData;
        public byte[] TransactionID;
        public long Timestamp;
        public long Value;
        public long TransactionFee;
        public JS_TransactionEntity[] Sources;
        public JS_TransactionEntity[] Destinations;
        public List<byte[]> Signatures;

        public JS_TransactionReply()
        {
        }

        public JS_TransactionReply(TransactionContent content)
        {
            VersionData = content.VersionData;
            ExecutionData = content.ExecutionData;
            Sources = (from src in content.Sources select new JS_TransactionEntity(src)).ToArray();
            Destinations = (from dst in content.Destinations select new JS_TransactionEntity(dst)).ToArray();
            Signatures = (from sig in content.Signatures select sig.Hex).ToList();
            Timestamp = content.Timestamp;
            TransactionID = content.TransactionID.Hex;
            TransactionFee = content.TransactionFee;
            Value = content.Value;
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }
    }

    public class JS_TransactionEntity : JS_Response
    {
        public byte[] PublicKey;
        public string Name;
        public string Address;
        public long Value;

        public JS_TransactionEntity()
        {
        }

        public JS_TransactionEntity(TransactionEntity entity)
        {
            PublicKey = entity.PublicKey;
            Name = entity.Name;
            Address = entity.Address;
            Value = entity.Value;
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }
    }


    /*public class TransactionEntity : ISerializableBase
    {
        byte[] publicKey;
        string name;
        string address;
        long _value;
                
        /// <summary>
        /// Public key for the account.
        /// </summary>
        public byte[] PublicKey
        {
            get { return publicKey; }
        }

        /// <summary>
        /// Name of the account. (Optional)
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Base58 Encoded Address.
        /// </summary>
        public string Address
        {
            get { return address; }
        }

        /// <summary>
        /// Remaining value/amount held with the account.
        /// </summary>
        public long Value
        {
            get { return _value; }
        }*/

    public class JS_TransactionState_Reply : JS_Response
    {
        public JS_TransactionReply Transaction;
        public TransactionStatusType TransactionStatusType;
        public TransactionProcessingResult TransactionProcessingResult;
        public int ValidationsCount;
        public long LedgerSequence;

        public JS_TransactionState_Reply(TransactionContent transactionContent, TransactionStatusType transactionStatusType,
         TransactionProcessingResult transactionProcessingResult,
            int validationsCount = 0, long ledgerSequence = 0)
        {
            Transaction = new JS_TransactionReply(transactionContent);
            TransactionStatusType = transactionStatusType;
            TransactionProcessingResult = transactionProcessingResult;
            ValidationsCount = validationsCount;
            LedgerSequence = ledgerSequence;
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }
    }

    public class JS_AccountReply : JS_Response
    {
        public byte[] PublicKey;
        public long Money;
        public string Name;
        public string Address;
        public AccountState AccountState;
        public NetworkType NetworkType;
        public AccountType AccountType;
        public long LastTransactionTime;

        public JS_AccountReply(AccountInfo accountInfo)
        {
            PublicKey = accountInfo.PublicKey.Hex;
            Money = accountInfo.Money;
            Name = accountInfo.Name;

            Address = accountInfo.GetAddress();

            NetworkType = accountInfo.NetworkType;
            AccountType = accountInfo.AccountType;

            AccountState = accountInfo.AccountState;
            LastTransactionTime = accountInfo.LastTransactionTime;
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }
    }

    /// <summary>
    /// Ugly Hack, fix ME !!!
    /// TODO: Only used to receive json, but still !!! fix me !!!
    /// </summary>
    public class JS_Resp_WorkProofRequest_Outer
    {
        public RPCStatus Status = RPCStatus.Failure;
        public JS_WorkProofRequest Data;

        public JS_Resp_WorkProofRequest_Outer()
        {

        }

        public JS_Resp_WorkProofRequest_Outer(RPCStatus status, JS_WorkProofRequest Data)
        {
            this.Status = status;
            this.Data = Data;
        }
    }


    public class JS_WorkProofRequest : JS_Response
    {    
        public byte[] ProofRequest = new byte[16];
        public DateTime IssueTime; // Consider changing it to long.
        public int Difficulty;

        public JS_WorkProofRequest(DifficultyTimeData difficultyTimeData)
        {
            
            Difficulty = difficultyTimeData.Difficulty;
            IssueTime = difficultyTimeData.IssueTime;
        }

        public void InitRequest()
        {
            Common.rngCsp.GetBytes(ProofRequest);
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }
    }

    public class JS_AccountRegisterRequest : JS_Response
    {
        public byte[] PublicKey;
        public string Name;
        public string Address;

        public byte[] ProofRequest;
        public byte[] ProofResponse;

        public JS_AccountRegisterRequest(byte[] publicKey, string name, string address, byte[] proofRequest, byte[] proofResponse)
        {
            PublicKey = publicKey;
            Name = name;
            Address = address;
            ProofRequest = proofRequest;
            ProofResponse = proofResponse;
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }
    }

    public class JS_TransactionReplies : JS_Response
    {
        public List<JS_TransactionReply> Transactions;

        public JS_TransactionReplies()
        {
            Transactions = new List<JS_TransactionReply>();
        }

        public JS_TransactionReplies(List<JS_TransactionReply> transactions)
        {
            Transactions = transactions;
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }
    }

    public class JS_TransactionStateReplies : JS_Response
    {
        public List<JS_TransactionState_Reply> TransactionState;

        public JS_TransactionStateReplies()
        {
            TransactionState = new List<JS_TransactionState_Reply>();
        }

        public JS_TransactionStateReplies(List<JS_TransactionState_Reply> transactionState)
        {
            TransactionState = transactionState;
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }
    }

    public class JS_AccountReplies : JS_Response
    {
        public List<JS_AccountReply> Accounts;

        public JS_AccountReplies()
        {
            Accounts = new List<JS_AccountReply>();
        }

        public JS_AccountReplies(List<JS_AccountReply> accounts)
        {
            Accounts = accounts;
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }
    }


}
