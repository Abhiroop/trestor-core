/*
 *  @Author: Arpan Jati
 *  @Version: 1.0
 *  @Date: 9 Jan 2015
 *  @Description: Json Structs for serialization
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    class JS_Info : JS_Response
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
        public List<TransactionEntity> Sources;
        public List<TransactionEntity> Destinations;
        public List<byte[]> Signatures;
        public long Timestamp;

        public JS_TransactionReply(TransactionContent content)
        {
            Sources = content.Sources;
            Destinations = content.Destinations;
            Signatures = content.Signatures;
            Timestamp = content.Timestamp;
            TransactionID = content.TransactionID.Hex;
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }
    }

    class JS_TransactionReplies : JS_Response
    {        
        public List<JS_TransactionReply> transactions;

        public JS_TransactionReplies(List<JS_TransactionReply> transactions)
        {
            this.transactions = transactions;
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }
    }

}
