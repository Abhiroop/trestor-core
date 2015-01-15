﻿/*
 *  @Author: Arpan Jati
 *  @Version: 1.0
 *  @Date: 9 Jan 2015 | 10 Jan 2015 | 14 Jan 2015
 *  @Description: Json Structs for serialization
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Address;
using TNetD.Transactions;

namespace TNetD.Json.JS_Structs
{
    class JS_Resp
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

    interface JS_Response
    {
        JS_Resp GetResponse();
    }

    class JS_Msg : JS_Response
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

    class JS_NodeInfo : JS_Response
    {
        public byte[] PublicKey;
        public string Name;
        public byte[] Address;
        public DateTime time;
        public string Organisation;
        public string Platform;
        public string Email;

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }
    }

    class JS_TransactionReply : JS_Response
    {
        public byte[] TransactionID;
        public long Timestamp;
        public long Value;
        public long TransactionFee;
        public List<TransactionEntity> Sources;
        public List<TransactionEntity> Destinations;
        public List<byte[]> Signatures;

        public JS_TransactionReply(TransactionContent content)
        {
            Sources = content.Sources;
            Destinations = content.Destinations;
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

    class JS_AccountReply : JS_Response
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

    class JS_TransactionReplies : JS_Response
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


    class JS_AccountReplies : JS_Response
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
