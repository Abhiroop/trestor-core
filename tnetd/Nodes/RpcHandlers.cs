/*
 *  @Author: Arpan Jati
 *  @Version: 1.0  
 *  @Description: Handles the RPC requests from the clients.
 *  @Date: Feb 10 2015 - First Version (From Node.cs)
 */

using Grapevine;
using Grapevine.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TNetD.Address;
using TNetD.Json.JS_Structs;
using TNetD.PersistentStore;
using TNetD.Transactions;
using TNetD.Types;

namespace TNetD.Nodes
{
    class RpcHandlers :  Responder
    {
        NodeConfig nodeConfig;
        NodeState nodeState;

        RESTServer restServer = default(RESTServer);

        public RpcHandlers(NodeConfig nodeConfig, NodeState nodeState)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;

            restServer = new RESTServer(Common.RPC_HOST, nodeConfig.ListenPortRPC.ToString(), "http", "index.html", null, 5, RPCRequestHandler);

            restServer.Start();
        }

        public void StopServer()
        {
            restServer.Stop();
        }

        // RPCRequestHandler rpcRequestHandler;
        bool RPCRequestHandler(HttpListenerContext context)
        {
            Interlocked.Increment(ref nodeState.NodeInfo.NodeDetails.RequestsProcessed);

            if (context.Request.RawUrl.Matches(@"^/info")) // ProcessInfo
            {
                this.SendJsonResponse(context, nodeState.NodeInfo.GetResponse());

                return true;
            }
            if (context.Request.RawUrl.Matches(@"^/time")) // ProcessInfo
            {
                this.SendJsonResponse(context, new JS_Time(nodeState.NetworkTime).GetResponse());

                return true;
            }
            else if (context.Request.RawUrl.Matches(@"^/wallet")) // Create Wallet 
            {
                HandleWalletQuery(context);

                return true;
            }
            else if (context.Request.RawUrl.Matches(@"^/propagateraw")) // Propagate RAW 
            {
                HandlePropagate(context, true);

                return true;
            }
            else if (context.Request.RawUrl.Matches(@"^/propagate")) // Propagate JSON 
            {
                HandlePropagate(context, false);

                return true;
            }
            else if (context.Request.RawUrl.Matches(@"^/txstatus")) // Handle Transaction Status Query
            {
                HandleTransactionStatusQuery(context);

                return true;
            }
            else if (context.Request.RawUrl.Matches(@"^/transactions")) // Fetch Transactions
            {
                if (context.Request.RawUrl.StartsWith("/transactions"))
                {
                    HandleTransactionQuery(context);
                    return true;
                }
            }
            else if (context.Request.RawUrl.Matches(@"^/history")) // Handle Transaction History Fetch
            {
                HandleTransactionHistoryQuery(context);

                return true;
            }
            else if (context.Request.RawUrl.Matches(@"^/accounts")) // Fetch Transactions
            {
                if (context.Request.RawUrl.StartsWith("/accounts"))
                {
                    HandleAccountQuery(context);
                    return true;
                }
            }
            else if (context.Request.RawUrl.Matches(@"^/request")) // Request a work proof
            {
                HandleWorkProofRequest(context);
                return true;
            }
            else if (context.Request.RawUrl.Matches(@"^/register")) // Register Account
            {
                //HandleAccountRegister(context);
                var resp = new JS_Msg("Update the application to create new Accounts..", RPCStatus.InvalidAPIUsage);
                this.SendJsonResponse(context, resp.GetResponse());
                return true;
            }

            return false;
        }

        private void HandleWorkProofRequest(HttpListenerContext context)
        {
            JS_Response resp = new JS_Msg("Starting Handler", RPCStatus.Undefined);

            try
            {
                if (context.Request.QueryString.AllKeys.Length == 0)
                {
                    if (nodeState.WorkProofMap.Count < Constants.WorkProofQueueLength)
                    {
                        //TODO: CRITICAL: MAKE SURE TIME SOURCE IS CORRECT

                        DifficultyTimeData difficultyTimeData = new DifficultyTimeData(Constants.Difficulty,
                            DateTime.FromFileTimeUtc(nodeState.SystemTime), 0, 0, ProofOfWorkType.DOUBLE_SHA256);

                        resp = new JS_WorkProofRequest(difficultyTimeData);

                        ((JS_WorkProofRequest)resp).InitRequest();

                        Hash Work = new Hash(((JS_WorkProofRequest)resp).ProofRequest);

                        nodeState.WorkProofMap.Add(Work, ((JS_WorkProofRequest)resp).GetDifficultyTimeData());
                    }
                    else
                    {
                        resp = new JS_Msg("Server Busy.", RPCStatus.ServerBusy);
                    }

                }
                else
                {
                    resp = new JS_Msg("Invalid API Usage.", RPCStatus.InvalidAPIUsage);
                }

                return;
            }
            catch
            {
                resp = new JS_Msg("Exception During Parsing", RPCStatus.Exception);
            }
            finally
            {
                this.SendJsonResponse(context, resp.GetResponse());
            }
        }

        private void HandleWalletQuery(HttpListenerContext context)
        {
            JS_Response msg = new JS_Msg("Starting Handler", RPCStatus.Undefined);

            try
            {
                string NAME = "";

                string name = context.Request.QueryString["name"];

                if (!String.IsNullOrEmpty(name))
                {
                    string[] _name = name.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (_name.Length == 1)
                    {
                        if (Utils.ValidateUserName(_name[0]))
                        {
                            NAME = _name[0];
                        }
                        else
                        {
                            msg = new JS_Msg("Invalid Name", RPCStatus.Exception);
                            throw new ArgumentException("RPC: Invalid Name Length.");
                        }
                    }
                }

                Tuple<AccountIdentifier, byte[]> accountData = AddressFactory.CreateNewAccount(NAME);

                msg = new JS_Address(accountData.Item1, accountData.Item2);
            }
            catch
            {
                msg = new JS_Msg("Exception During Parsing", RPCStatus.Exception);
            }
            finally
            {
                this.SendJsonResponse(context, msg);
            }
        }

        private void HandleTransactionHistoryQuery(HttpListenerContext context)
        {
            JS_TransactionReplies replies = new JS_TransactionReplies();
            JS_Msg msg = new JS_Msg("Processing Starting", RPCStatus.Exception);

            try
            {
                string tran = context.Request.QueryString["pk"];

                if (tran != "")
                {
                    string[] publicKey = tran.Split(',');

                    if (publicKey.Length == 1)
                    {
                        byte[] publicKey_Bytes = new byte[0];

                        try
                        {
                            publicKey_Bytes = HexUtil.GetBytes(publicKey[0]);
                        }
                        catch { }

                        if (publicKey_Bytes.Length == Common.LEN_TRANSACTION_ID)
                        {
                            string _limit = context.Request.QueryString["limit"];
                            int limit = 0;
                            if (!String.IsNullOrEmpty(_limit))
                            {
                                string[] __limit = _limit.Split(new char[] { ',' }, 1, StringSplitOptions.RemoveEmptyEntries);
                                if (__limit.Length == 1)
                                {
                                    int.TryParse(__limit[0], out limit);
                                }
                            }

                            string _timeStamp = context.Request.QueryString["time"];
                            long timeStamp = 0;
                            if (!String.IsNullOrEmpty(_timeStamp))
                            {
                                string[] __timeStamp = _timeStamp.Split(new char[] { ',' }, 1, StringSplitOptions.RemoveEmptyEntries);
                                if (__timeStamp.Length == 1)
                                {
                                    long.TryParse(__timeStamp[0], out timeStamp);
                                }
                            }

                            List<TransactionContent> transactionContents;

                            nodeState.Persistent.TransactionStore.FetchTransactionHistory(out transactionContents, new Hash(publicKey_Bytes), timeStamp, limit);

                            foreach (TransactionContent transactionContent in transactionContents)
                            {
                                replies.Transactions.Add(new JS_TransactionReply(transactionContent));
                            }

                        }
                    }
                }

                if (replies.Transactions.Count > 0)
                {
                    msg = new JS_Msg("History Fetch Success.", RPCStatus.Success);
                }
                else
                {
                    msg = new JS_Msg("No Records Found.", RPCStatus.Failure);
                }
            }
            catch
            {
                msg = new JS_Msg("Exception During Parsing.", RPCStatus.Exception);
            }
            finally
            {
                if (replies.Transactions.Count > 0)
                {
                    this.SendJsonResponse(context, replies.GetResponse());
                }
                else
                {
                    this.SendJsonResponse(context, msg.GetResponse());
                }
            }
        }

        private void HandleTransactionStatusQuery(HttpListenerContext context)
        {
            JS_TransactionStateReplies replies = new JS_TransactionStateReplies();

            foreach (string key in context.Request.QueryString.AllKeys)
            {
                switch (key)
                {
                    case "id":

                        string[] transactionIDs = context.Request.QueryString["id"].Split(',');

                        foreach (string transactionID in transactionIDs)
                        {
                            byte[] transactionID_Bytes = new byte[0];

                            try
                            {
                                transactionID_Bytes = HexUtil.GetBytes(transactionID);
                            }
                            catch { }

                            if (transactionID_Bytes.Length == Common.LEN_TRANSACTION_ID)
                            {
                                HandleTransactionStatusQuery_Internal(ref replies, new Hash(transactionID_Bytes));
                            }
                        }

                        break;

                    default:

                        break;
                }
            }

            // // /////////////////////////////////////////////////////////////////////

            if (replies.TransactionState.Count > 0)
            {
                this.SendJsonResponse(context, replies.GetResponse());
            }
            else
            {
                JS_Resp resp = new JS_Resp();
                this.SendJsonResponse(context, resp);
            }
        }

        private void HandleTransactionStatusQuery_Internal(ref JS_TransactionStateReplies replies, Hash transactionID)
        {
            TransactionState transactionState;
            if (nodeState.TransactionStateManager.Fetch(out transactionState, transactionID))
            {
                replies.TransactionState.Add(new JS_TransactionState_Reply(transactionState.StatusType,
                    transactionState.ProcessingResult));

                return;
            }

            // Check if the transaction is processed.
            TransactionContent transactionContent;
            long sequenceNumber;
            if (nodeState.Persistent.TransactionStore.FetchTransaction(out transactionContent, out sequenceNumber, transactionID) == DBResponse.FetchSuccess)
            {
                replies.TransactionState.Add(new JS_TransactionState_Reply(TransactionStatusType.Success,
                    TransactionProcessingResult.PR_Success));
                return;
            }
        }

        private void HandleAccountQuery(HttpListenerContext context)
        {
            JS_AccountReplies replies = new JS_AccountReplies();

            foreach (string key in context.Request.QueryString.AllKeys)
            {
                switch (key)
                {
                    case "p":
                        {
                            string[] publicKeys = context.Request.QueryString["p"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (string publicKey in publicKeys)
                            {
                                byte[] publicKey_Bytes = new byte[0];

                                try
                                {
                                    publicKey_Bytes = HexUtil.GetBytes(publicKey);
                                }
                                catch { }

                                if (publicKey_Bytes.Length == 32)
                                {
                                    Hash h_publicKey = new Hash(publicKey_Bytes);
                                    if (nodeState.Ledger.AccountExists(h_publicKey))
                                    {
                                        replies.Accounts.Add(new JS_AccountReply(nodeState.Ledger[h_publicKey]));
                                    }
                                }
                            }
                        }
                        break;

                    case "a":
                        {
                            string[] adresses = context.Request.QueryString["a"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (string address in adresses)
                            {
                                if (nodeState.Ledger.AddressAccountInfoMap.ContainsKey(address))
                                {
                                    AccountInfo _ai = nodeState.Ledger.AddressAccountInfoMap[address];
                                    replies.Accounts.Add(new JS_AccountReply(_ai));
                                }
                            }
                        }

                        break;

                    case "n":
                        {
                            string[] names = context.Request.QueryString["n"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (string name in names)
                            {
                                if (nodeState.Ledger.NameAccountInfoMap.ContainsKey(name.ToLowerInvariant()))
                                {
                                    AccountInfo _ai = nodeState.Ledger.NameAccountInfoMap[name.ToLowerInvariant()];
                                    replies.Accounts.Add(new JS_AccountReply(_ai));
                                }
                            }
                        }

                        break;

                    default:

                        break;
                }
            }

            // // /////////////////////////////////////////////////////////////////////

            if (replies.Accounts.Count > 0)
            {
                this.SendJsonResponse(context, replies.GetResponse());
            }
            else
            {
                JS_Resp resp = new JS_Resp();
                this.SendJsonResponse(context, resp);
            }
        }

        private void HandleTransactionQuery(HttpListenerContext context)
        {
            JS_TransactionReplies replies = new JS_TransactionReplies();

            foreach (string key in context.Request.QueryString.AllKeys)
            {
                switch (key)
                {
                    case "id":

                        string[] transactionIDs = context.Request.QueryString["id"].Split(',');

                        foreach (string transactionID in transactionIDs)
                        {
                            byte[] transactionID_Bytes = new byte[0];

                            try
                            {
                                transactionID_Bytes = HexUtil.GetBytes(transactionID);
                            }
                            catch { }

                            if (transactionID_Bytes.Length == 32)
                            {
                                TransactionContent transactionContent;
                                long sequenceNumber;
                                if (nodeState.Persistent.TransactionStore.FetchTransaction(out transactionContent, out sequenceNumber, new Hash(transactionID_Bytes)) == DBResponse.FetchSuccess)
                                {
                                    replies.Transactions.Add(new JS_TransactionReply(transactionContent));
                                }
                            }
                        }

                        break;

                    default:

                        break;
                }
            }

            // // /////////////////////////////////////////////////////////////////////

            if (replies.Transactions.Count > 0)
            {
                this.SendJsonResponse(context, replies.GetResponse());
            }
            else
            {
                JS_Resp resp = new JS_Resp();
                this.SendJsonResponse(context, resp);
            }
        }

        private void HandlePropagate(HttpListenerContext context, bool IsRaw)
        {
            JS_Msg msg;

            if (context.Request.HttpMethod.ToUpper().Equals("POST") && context.Request.HasEntityBody &&
                context.Request.ContentLength64 < Constants.PREFS_MAX_RPC_POST_CONTENT_LENGTH)
            {
                StreamReader inputStream = new StreamReader(context.Request.InputStream);

                try
                {
                    TransactionContent transactionContent = new TransactionContent();

                    if (IsRaw)
                    {
                        byte[] data = HexUtil.GetBytes(inputStream.ReadToEnd());
                        transactionContent.Deserialize(data);
                    }
                    else
                    {
                        string json = inputStream.ReadToEnd();
                        JS_TransactionReply jtr = JsonConvert.DeserializeObject<JS_TransactionReply>(json,
                            Common.JSON_SERIALIZER_SETTINGS);

                        transactionContent.Deserialize(jtr);
                    }

                    //// TODO: CRITICAL: MAKE SURE TIME SOURCE IS CORRECT
                    long StaleSeconds = (long)Math.Abs((DateTime.FromFileTimeUtc(transactionContent.Timestamp) - DateTime.FromFileTimeUtc(nodeState.SystemTime)).TotalSeconds);

                    if (StaleSeconds < (Common.TRANSACTION_STALE_TIMER_MINUTES * 60))
                    {
                        // This is a bit Redundant / Done later too. But okay.
                        if (transactionContent.VerifySignature() == TransactionProcessingResult.Accepted)
                        {
                            TransactionProcessingResult tpResult = nodeState.IncomingTransactionMap.HandlePropagationRequest(transactionContent);

                            if (tpResult == TransactionProcessingResult.Accepted)
                            {
                                msg = new JS_Msg("Transaction Added to propagation queue.", RPCStatus.Success);
                            }
                            else
                            {
                                msg = new JS_Msg("Transaction Processing Error: " + tpResult, RPCStatus.Failure);
                            }
                        }
                        else
                        {
                            msg = new JS_Msg("Transaction signature is invalid.", RPCStatus.Failure);
                        }
                    }
                    else
                    {
                        msg = new JS_Msg("Transaction time is stale by " + StaleSeconds + " seconds. Your clock may be set incorrectly or the program may need upgradation.", RPCStatus.Failure);
                    }
                }
                catch
                {
                    msg = new JS_Msg("Malformed Transaction.", RPCStatus.Failure);
                }
            }
            else
            {
                msg = new JS_Msg("Improper usage. Need to use HTTP POST with 'Transaction Proposal' as Content.", RPCStatus.InvalidAPIUsage);
            }

            this.SendJsonResponse(context, msg.GetResponse());
        }
        
        private void HandleAccountRegister(HttpListenerContext context)
        {
            JS_Msg msg = new JS_Msg("Processing Initiated.", RPCStatus.Undefined);

            if (context.Request.HttpMethod.ToUpper().Equals("POST") && context.Request.HasEntityBody &&
                context.Request.ContentLength64 < Constants.PREFS_MAX_RPC_POST_CONTENT_LENGTH)
            {
                Interlocked.Increment(ref nodeState.NodeInfo.NodeDetails.AccountCreationRequests);

                StreamReader inputStream = new StreamReader(context.Request.InputStream);

                try
                {
                    TransactionContent transactionContent = new TransactionContent();

                    string json = inputStream.ReadToEnd();
                    JS_AccountRegisterRequest request = JsonConvert.DeserializeObject<JS_AccountRegisterRequest>(json,
                        Common.JSON_SERIALIZER_SETTINGS);

                    Hash proofRequest = new Hash(request.ProofRequest);

                    if (nodeState.WorkProofMap.ContainsKey(proofRequest))
                    {
                        int diff = nodeState.WorkProofMap[proofRequest].Difficulty;

                        if (WorkProof.VerifyProof(proofRequest.Hex, request.ProofResponse, diff))
                        {
                            AddressData AD = AddressFactory.DecodeAddressString(request.Address);

                            bool TypesFine = true;

                            if (Common.NETWORK_TYPE == NetworkType.MainNet)
                            {
                                if (AD.NetworkType != NetworkType.MainNet)
                                {
                                    msg = new JS_Msg("Invalid Network Type", RPCStatus.InvalidAPIUsage);
                                    TypesFine = false;
                                }

                                if ((AD.AccountType != AccountType.MainNormal))
                                {
                                    msg = new JS_Msg("Invalid Account Type", RPCStatus.InvalidAPIUsage);
                                    TypesFine = false;
                                }
                            }
                            else
                            {
                                if (AD.NetworkType != NetworkType.TestNet)
                                {
                                    msg = new JS_Msg("Invalid Network Type", RPCStatus.InvalidAPIUsage);
                                    TypesFine = false;
                                }

                                if ((AD.AccountType != AccountType.TestNormal))
                                {
                                    msg = new JS_Msg("Invalid Account Type", RPCStatus.InvalidAPIUsage);
                                    TypesFine = false;
                                }
                            }

                            if (TypesFine)
                            {
                                AddressData addressData;
                                if (AddressFactory.VerfiyAddress(out addressData, request.Address, request.PublicKey, request.Name))
                                {
                                    if (addressData.ValidateAccountType())
                                    {
                                        AccountInfo newAccountInfo = new AccountInfo(new Hash(request.PublicKey), 0, request.Name, AccountState.Normal,
                                        addressData.NetworkType, addressData.AccountType, nodeState.NetworkTime);

                                        bool GoodName = nodeState.IsGoodValidUserName(request.Name);

                                        if (GoodName)
                                        {
                                            bool ExistsPK = nodeState.Persistent.AccountStore.AccountExists(new Hash(request.PublicKey));
                                            bool ExistsName = nodeState.Persistent.AccountStore.AccountExists(request.Name);

                                            if (!ExistsPK && !ExistsName)
                                            {
                                                if (nodeState.Persistent.AccountStore.AddUpdate(newAccountInfo) == DBResponse.InsertSuccess)
                                                {
                                                    nodeState.Ledger.AddUpdateBatch(new AccountInfo[] { newAccountInfo });
                                                    Interlocked.Increment(ref nodeState.NodeInfo.NodeDetails.TotalAccounts);
                                                    msg = new JS_Msg("Account Successfully Added", RPCStatus.Success);
                                                }
                                                else
                                                {
                                                    msg = new JS_Msg("Server Database Busy", RPCStatus.Exception);
                                                }
                                            }
                                            else
                                            {
                                                msg = new JS_Msg("Account Already Exists", RPCStatus.Failure);
                                            }
                                        }
                                        else
                                        {
                                            msg = new JS_Msg("Invalid or Banned Name", RPCStatus.Failure);
                                        }
                                    }
                                    else
                                    {
                                        msg = new JS_Msg("Invalid Network or Account Type", RPCStatus.Failure);
                                    }
                                }
                                else
                                {
                                    msg = new JS_Msg("Invalid Address Format", RPCStatus.Failure);
                                }
                            }
                        }
                        else
                        {
                            msg = new JS_Msg("Proof Verification Failed", RPCStatus.Failure);
                        }

                    } // End IF - workproofmap   
                    else
                    {
                        msg = new JS_Msg("No Such Request Issued", RPCStatus.Failure);
                    }
                }
                catch (Exception ex)
                {
                    DisplayUtils.Display("HandleAccountRegister", ex);

                    msg = new JS_Msg("Malformed Request.", RPCStatus.Failure);
                }

            }
            else
            {
                msg = new JS_Msg("Improper usage. Need to use HTTP POST with 'Account Register Request' as Content.", RPCStatus.InvalidAPIUsage);
            }

            this.SendJsonResponse(context, msg.GetResponse());
        }

    }
}
