/*
 *  @Author: Arpan Jati
 *  @Version: 1.0
 *  @Date: Oct 2015 | Jan 2015
 *  @Description: Node: It represents a full fledged transaction processor / validator / the complete thing.
 */

using Chaos.NaCl;
using Grapevine;
using Grapevine.Server;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TNetD.Address;
using TNetD.Json.JS_Structs;
using TNetD.Ledgers;
using TNetD.Network;
using TNetD.Network.Networking;
using TNetD.PersistentStore;
using TNetD.Transactions;
using TNetD.Tree;
using TNetD.Types;

namespace TNetD.Nodes
{

    public delegate void NodeStatusEventHandler(string Status, int NodeID);

    internal class Node : Responder
    {
        public event NodeStatusEventHandler NodeStatusEvent;

        bool TimerEventProcessed = true;

        SecureNetwork network = default(SecureNetwork);

        RESTServer restServer = default(RESTServer);

        NodeState nodeState;

        IncomingTransactionMap incomingTransactionMap;

        /// <summary>
        /// A dictionary of Trusted nodes, stored by PublicKey
        /// </summary>
        public Dictionary<Hash, NodeSocketData> TrustedNodes;

        public int OutTransactionCount = 0;
        // public int InCandidatesCount;
        // public int InTransactionCount;

        public Dictionary<Hash, DifficultyTimeData> WorkProofMap = new Dictionary<Hash, DifficultyTimeData>();

        public AccountInfo AI;

        public IPersistentAccountStore PersistentAccountStore;
        public IPersistentTransactionStore PersistentTransactionStore;

        public SQLiteBannedNames PersistentBannedNameStore;
        public SQLiteCloseHistory PersistentCloseHistory;

        Ledger ledger;

        #region ConstructorsAndTimers

        public Ledger LocalLedger
        {
            get { return ledger; }
        }

        public Hash PublicKey
        {
            get
            {
                return nodeConfig.PublicKey;
            }
        }

        System.Timers.Timer TimerConsensus;
        System.Timers.Timer TimerSecond;
        System.Timers.Timer TimerMinute;

        // TODO: MAKE PRIVATE : AND FAST
        public NodeConfig nodeConfig = default(NodeConfig);

        /// <summary>
        /// Initializes a node. Node ID is 0 for most cases.
        /// Only other use is hosting multiple validators from an IP (bad-idea) and simulation.
        /// </summary>
        /// <param name="ID"></param>
        public Node(int ID, GlobalConfiguration globalConfiguration)
        {
            nodeConfig = new NodeConfig(ID, globalConfiguration);

            nodeState = new NodeState();

            nodeState.NodeInfo = nodeConfig.Get_JS_Info();

            //nodeState.NodeInfo.NodeDetails = new JS_NodeDetails();
            //nodeState.NodeInfo.LastLedgerInfo = new JS_LedgerInfo();            

            incomingTransactionMap = new IncomingTransactionMap(nodeState, nodeConfig);

            network = new SecureNetwork(nodeConfig);
            network.PacketReceived += network_PacketReceived;

            network.Initialize();

            TrustedNodes = globalConfiguration.TrustedNodes;

            PersistentAccountStore = new SQLiteAccountStore(nodeConfig);
            PersistentTransactionStore = new SQLiteTransactionStore(nodeConfig);
            PersistentBannedNameStore = new SQLiteBannedNames(nodeConfig);
            PersistentCloseHistory = new SQLiteCloseHistory(nodeConfig);

            AI = new AccountInfo(PublicKey, Money);

            ledger = new Ledger(PersistentAccountStore);

            //ledger.AddUserToLedger(AI);

            TimerConsensus = new System.Timers.Timer();
            TimerConsensus.Elapsed += TimerConsensus_Elapsed;
            TimerConsensus.Enabled = true;
            TimerConsensus.Interval = nodeConfig.UpdateFrequencyConsensusMS;
            TimerConsensus.Start();

            // ////////////////////

            TimerSecond = new System.Timers.Timer();
            TimerSecond.Elapsed += TimerSecond_Elapsed;
            TimerSecond.Enabled = true;
            TimerSecond.Interval = 100;
            TimerSecond.Start();

            TimerMinute = new System.Timers.Timer();
            TimerMinute.Elapsed += TimerMinute_Elapsed;
            TimerMinute.Enabled = true;
            TimerMinute.Interval = 60000;
            TimerMinute.Start();

            restServer = new RESTServer(Common.RpcHost, nodeConfig.ListenPortRPC.ToString(), "http", "index.html", null, 5, RPCRequestHandler);
            
            restServer.Start();

            DisplayUtils.Display("Started Node " + nodeConfig.NodeID, DisplayType.ImportantInfo);
        }

        void TimerMinute_Elapsed(object sender, ElapsedEventArgs e)
        {
            Hash[] Kys = WorkProofMap.Keys.ToArray();

            try
            {
                DateTime NOW = DateTime.UtcNow;

                // Clean temporary proof of work Queue
                // TODO : CRITICAL : Make sure the valid ones are removed, and not the critical ones.

                foreach (Hash key in Kys)
                {
                    DifficultyTimeData dtd;
                    if (WorkProofMap.TryGetValue(key, out dtd))
                    {
                        TimeSpan span = (NOW - dtd.IssueTime);
                        if (span.TotalSeconds > 60) // 1 Minute
                        {
                            WorkProofMap.Remove(key);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayUtils.Display("TimerMinute_Elapsed", ex);
            }
        }

        //WorkProofMap

        void TimerSecond_Elapsed(object sender, ElapsedEventArgs e)
        {
            nodeState.NodeInfo.NodeDetails.TimeUTC = DateTime.UtcNow;
            nodeState.NodeInfo.LastLedgerInfo.Hash = ledger.GetRootHash().Hex;

            nodeState.NodeInfo.NodeDetails.ProofOfWorkQueueLength = WorkProofMap.Count;

            nodeState.system_time = DateTime.UtcNow.ToFileTimeUtc();
            nodeState.network_time = DateTime.UtcNow.ToFileTimeUtc();

            if (NodeStatusEvent != null)
            {
                var json = JsonConvert.SerializeObject(nodeState.NodeInfo.GetResponse(), Common.JsonSerializerSettings);
                NodeStatusEvent(json, nodeConfig.NodeID);
            }
        }

        /// <summary>
        /// Add more content to be loaded in background here.
        /// </summary>       
        async public void BeginBackgroundLoad()
        {
            await Task.Run(async () =>
            {
                long records = await ledger.InitializeLedger();

                Interlocked.Add(ref nodeState.NodeInfo.NodeDetails.TotalAccounts, records);

                // Connect to TrustedNodes
                List<Task> tasks = new List<Task>();

                foreach (KeyValuePair<Hash, NodeSocketData> kvp in TrustedNodes)
                {
                    if (kvp.Key != PublicKey) // Make sure we are not connecting to self !!
                    {
                        if (!network.IsConnected(kvp.Key))
                        {
                            tasks.Add(SendInitialize(kvp.Key));
                        }
                    }
                }

                await Task.WhenAll(tasks);

                LedgerCloseData ledgerCloseData;
                PersistentCloseHistory.GetLastRowData(out ledgerCloseData);
                nodeState.NodeInfo.LastLedgerInfo = new JS_LedgerInfo(ledgerCloseData);
            });
        }

        #endregion

        #region RPC HANDLING

        // RPCRequestHandler rpcRequestHandler;
        bool RPCRequestHandler(HttpListenerContext context)
        {
            Interlocked.Increment(ref nodeState.NodeInfo.NodeDetails.RequestsProcessed);

            if (context.Request.RawUrl.Matches(@"^/info")) // ProcessInfo
            {
                this.SendJsonResponse(context, nodeState.NodeInfo.GetResponse());

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
                HandleAccountRegister(context);
                return true;
            }
            /*else if (context.Request.RawUrl.Matches(@"^/validname")) // Register Account
            {
                HandleValidName(context);
                return true;
            }*/

            return false;
        }

        /*private void HandleValidName(HttpListenerContext context)
        {
            JS_Response msg = new JS_Msg("Starting Handler", RPCStatus.Undefined);

            try
            {
                string NAME = "";
                string name = context.Request.QueryString["n"];

                if (!String.IsNullOrEmpty(name))
                {
                    string[] _name = name.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (_name.Length == 1)
                    {
                        if (_name[0].Length >= Constants.Pref_MinNameLength)
                        {
                            NAME = name;
                        }
                        else
                        {
                            msg = new JS_Msg("Invalid Name Length.", RPCStatus.Exception);
                        }
                    }
                    else
                    {
                        msg = new JS_Msg("Invalid Name Length.", RPCStatus.Exception);
                    }
                }
                else
                {
                    msg = new JS_Msg("Invalid Arguments", RPCStatus.Exception);
                }
            }
            catch
            {
                msg = new JS_Msg("Exception During Parsing", RPCStatus.Exception);
            }
            finally
            {
                this.SendJsonResponse(context, msg);
            }
        }*/

        private void HandleWorkProofRequest(HttpListenerContext context)
        {
            JS_Response resp = new JS_Msg("Starting Handler", RPCStatus.Undefined);

            try
            {
                if (context.Request.QueryString.AllKeys.Length == 0)
                {
                    if (WorkProofMap.Count < Constants.WorkProofQueueLength)
                    {
                        DifficultyTimeData difficultyTimeData = new DifficultyTimeData(Constants.Difficulty,
                            nodeState.NodeInfo.NodeDetails.TimeUTC, 0, 0, ProofOfWorkType.DOUBLE_SHA256);

                        resp = new JS_WorkProofRequest(difficultyTimeData);

                        ((JS_WorkProofRequest)resp).InitRequest();

                        Hash Work = new Hash(((JS_WorkProofRequest)resp).ProofRequest);

                        WorkProofMap.Add(Work, ((JS_WorkProofRequest)resp).GetDifficultyTimeData());
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
                        if (_name[0].Length >= Constants.Pref_MinNameLength)
                        {
                            NAME = name;
                        }
                        else
                        {
                            msg = new JS_Msg("Invalid Name Length.", RPCStatus.Exception);
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

                            PersistentTransactionStore.FetchTransactionHistory(out transactionContents, new Hash(publicKey_Bytes), timeStamp, limit);

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

            TransactionProcessingResult result = TransactionProcessingResult.Unprocessed;
            if (incomingTransactionMap.TransactionProcessingMap.ContainsKey(transactionID))
            {
                result = incomingTransactionMap.TransactionProcessingMap[transactionID];

                TransactionContent tc;
                if (incomingTransactionMap.IncomingPropagations_ALL.ContainsKey(transactionID))
                {
                    tc = incomingTransactionMap.IncomingPropagations_ALL[transactionID];

                    replies.TransactionState.Add(new JS_TransactionState_Reply(tc, TransactionStatusType.InPreProcessing, result));
                    return;
                }
            }

            // Check if the transaction is in the proposed queue.
            Tuple<TransactionContent, bool> response_prop = incomingTransactionMap.GetPropagationContent(transactionID);
            if (response_prop.Item2 == true)
            {
                replies.TransactionState.Add(new JS_TransactionState_Reply(response_prop.Item1, TransactionStatusType.Proposed, TransactionProcessingResult.Accepted));
                return;
            }

            // Check if the transaction is in the processing queue.
            Tuple<TransactionContent, bool> response_queue = incomingTransactionMap.GetTransactionContent(transactionID);
            if (response_queue.Item2 == true)
            {
                replies.TransactionState.Add(new JS_TransactionState_Reply(response_queue.Item1, TransactionStatusType.InProcessingQueue, TransactionProcessingResult.Accepted));
                return;
            }

            // Check if the transaction is processed.
            TransactionContent transactionContent;
            long sequenceNumber;
            if (PersistentTransactionStore.FetchTransaction(out transactionContent, out sequenceNumber, transactionID) == DBResponse.FetchSuccess)
            {
                replies.TransactionState.Add(new JS_TransactionState_Reply(transactionContent, TransactionStatusType.Processed,
                    TransactionProcessingResult.Accepted));
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
                                    if (ledger.AccountExists(h_publicKey))
                                    {
                                        replies.Accounts.Add(new JS_AccountReply(ledger[h_publicKey]));
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
                                if (ledger.AddressAccountInfoMap.ContainsKey(address))
                                {
                                    AccountInfo _ai = ledger.AddressAccountInfoMap[address];
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
                                if (ledger.NameAccountInfoMap.ContainsKey(name.ToLowerInvariant()))
                                {
                                    AccountInfo _ai = ledger.NameAccountInfoMap[name.ToLowerInvariant()];
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
                                if (PersistentTransactionStore.FetchTransaction(out transactionContent, out sequenceNumber, new Hash(transactionID_Bytes)) == DBResponse.FetchSuccess)
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
                            Common.JsonSerializerSettings);

                        transactionContent.Deserialize(jtr);
                    }

                    long StaleSeconds = (long)Math.Abs((DateTime.FromFileTimeUtc(transactionContent.Timestamp) - nodeState.NodeInfo.NodeDetails.TimeUTC).TotalSeconds);

                    if (StaleSeconds < (Common.TransactionStaleTimer_Minutes * 60))
                    {
                        TransactionProcessingResult tpResult = incomingTransactionMap.HandlePropagationRequest(transactionContent);

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


        bool IsGoodValidUserName(string Name)
        {
            if ((Name.Length <= Constants.Pref_MinNameLength)) return false;

            if ((Name.Length >= Constants.Pref_MaxNameLength)) return false;

            if (!Utils.ValidateUserName(Name)) return false;

            if (PersistentBannedNameStore.Contains(Name)) return false;

            return true;
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
                        Common.JsonSerializerSettings);

                    Hash proofRequest = new Hash(request.ProofRequest);

                    if (WorkProofMap.ContainsKey(proofRequest))
                    {
                        int diff = WorkProofMap[proofRequest].Difficulty;

                        if (WorkProof.VerifyProof(proofRequest.Hex, request.ProofResponse, diff))
                        {
                            AddressData AD = AddressFactory.DecodeAddressString(request.Address);

                            bool TypesFine = true;

                            if (Common.NetworkType == NetworkType.MainNet)
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
                                        addressData.NetworkType, addressData.AccountType, nodeState.network_time);

                                        bool GoodName = IsGoodValidUserName(request.Name);

                                        if (GoodName)
                                        {
                                            bool ExistsPK = PersistentAccountStore.AccountExists(new Hash(request.PublicKey));
                                            bool ExistsName = PersistentAccountStore.AccountExists(request.Name);

                                            if (!ExistsPK && !ExistsName)
                                            {
                                                if (PersistentAccountStore.AddUpdate(newAccountInfo) == DBResponse.InsertSuccess)
                                                {
                                                    ledger.AddUpdateBatch(new AccountInfo[] { newAccountInfo });
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

        #endregion

        void network_PacketReceived(Hash publicKey, NetworkPacket packet)
        {
            DisplayUtils.Display(" Packet: " + packet.Type + " | From: " + publicKey + " | Data Length : " + packet.Data.Length);
        }

        public void StopNode()
        {
            Constants.ApplicationRunning = false;

            network.Stop();
            restServer.Stop();
        }

        async Task SendInitialize(Hash publicKey)
        {
            await Task.Delay(Common.random.Next(500, 1000)); // Wait a random delay before connecting.
            NetworkPacketQueueEntry npqe = new NetworkPacketQueueEntry(publicKey, new NetworkPacket(PublicKey, PacketType.TPT_HELLO, new byte[0]));
            network.AddToQueue(npqe);
        }


        #region TRANSACTION PROCESSING

        /// <summary>
        /// Timer now fast. Actual/Final timer would depend on consensus rate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerConsensus_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (TimerEventProcessed) // Lock to prevent multiple invocations
            {
                TimerEventProcessed = false;

                // // // // // // // // //

                try
                {

                    if (incomingTransactionMap.IncomingTransactions.Count > 0)
                    {
                        Queue<TransactionContent> transactionContentStack = new Queue<TransactionContent>();

                        lock (incomingTransactionMap.transactionLock)
                        {
                            foreach (KeyValuePair<Hash, TransactionContent> kvp in incomingTransactionMap.IncomingTransactions)
                            {
                                transactionContentStack.Enqueue(kvp.Value);

                                Interlocked.Increment(ref nodeState.NodeInfo.NodeDetails.TransactionsVerified);
                            }

                            incomingTransactionMap.IncomingTransactions.Clear();
                            incomingTransactionMap.ClearTransactionProcessingMap();
                            incomingTransactionMap.IncomingPropagations_ALL.Clear();
                        }


                        Dictionary<Hash, TreeDiffData> pendingDifferenceData = new Dictionary<Hash, TreeDiffData>();
                        Dictionary<Hash, TransactionContent> acceptedTransactions = new Dictionary<Hash, TransactionContent>();
                        List<AccountInfo> newAccounts = new List<AccountInfo>();

                        long totalTransactionFees = 0;

                        while (transactionContentStack.Count > 0)
                        {
                            TransactionContent transactionContent = transactionContentStack.Dequeue();

                            try
                            {
                                if (transactionContent.VerifySignature() == TransactionProcessingResult.Accepted)
                                {
                                    

                                    TransactionContent transactionFromPersistentDB;
                                    long sequenceNumber;
                                    if (PersistentTransactionStore.FetchTransaction(out transactionFromPersistentDB, out sequenceNumber,
                                        transactionContent.TransactionID) == DBResponse.FetchSuccess)
                                    {
                                        //TODO: LOG THIS and Display properly.
                                        DisplayUtils.Display("Transaction Processed. Improbable because of previous checks.", DisplayType.BadData);
                                    }
                                    else
                                    {
                                        // Check Sources
                                        bool badSource = false;

                                        // True if account
                                        bool badAccountName_inTransaction = false;
                                        bool badAccountAddress_inTransaction = false;
                                        bool badAccountCreationValue = false;
                                        bool badAccountState = false;
                                        bool badTransactionFee = false;
                                        bool insufficientFunds = false;

                                        List<AccountInfo> temp_NewAccounts = new List<AccountInfo>();

                                        // Check if account name in destination is valid.

                                        foreach (TransactionEntity te in transactionContent.Destinations)
                                        {

                                            if (!Utils.ValidateUserName(te.Name))
                                            {
                                                badAccountName_inTransaction = true; // Names should be lowercase.
                                                break;
                                            }


                                            AccountInfo ai;
                                            if (PersistentAccountStore.FetchAccount(out ai, new Hash(te.PublicKey)) == DBResponse.FetchSuccess)
                                            {
                                                // Account Exists
                                                if (ai.Name != te.Name)
                                                {
                                                    badAccountName_inTransaction = true;
                                                    break;
                                                }

                                                string Addr = ai.GetAddress();

                                                if (Addr != te.Address)
                                                {
                                                    badAccountAddress_inTransaction = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                if (te.Name.Length >= Constants.Pref_MinNameLength)
                                                {
                                                    // Check if same named account exists. When, public key could not be fetched.
                                                    if (PersistentAccountStore.FetchAccount(out ai, te.Name) == DBResponse.FetchSuccess)
                                                    {
                                                        // Thats too bad, transaction cannot happen, 
                                                        // new wallet has invalid Name (name already used).
                                                        badAccountName_inTransaction = true;
                                                    }
                                                    else
                                                    {

                                                    }
                                                }
                                            }
                                        }

                                        if (!Common.IsTransactionFeeEnabled) // Transaction Fee not allowed here !!
                                        {
                                            if (transactionContent.TransactionFee > 0)
                                            {
                                                badTransactionFee = true;
                                            }
                                        }

                                        totalTransactionFees += transactionContent.TransactionFee;

                                        foreach (TransactionEntity source in transactionContent.Sources)
                                        {
                                            Hash pkSource = new Hash(source.PublicKey);

                                            if (ledger.AccountExists(pkSource))
                                            {
                                                AccountInfo account = ledger[pkSource];

                                                long PendingValueDifference = 0;

                                                // Check if the account exists in the pending transaction queue.
                                                if (pendingDifferenceData.ContainsKey(pkSource))
                                                {
                                                    TreeDiffData treeDiffData = pendingDifferenceData[pkSource];

                                                    // PendingValueDifference += treeDiffData.AddValue; // [Allows simultaneous TX]
                                                    PendingValueDifference -= treeDiffData.RemoveValue;
                                                }

                                                if (account.AccountState == AccountState.Normal)
                                                {
                                                    if ((account.Money + PendingValueDifference) >= (source.Value + Constants.FIN_MIN_BALANCE))
                                                    {
                                                        // Has enough money.
                                                    }
                                                    else
                                                    {
                                                        insufficientFunds = true;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    badAccountState = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                badSource = true;
                                                break;
                                            }
                                        }

                                        /// Check Destinations

                                        foreach (TransactionEntity destination in transactionContent.Destinations)
                                        {
                                            Hash PK = new Hash(destination.PublicKey);

                                            if (ledger.AccountExists(PK))
                                            {
                                                // Perfect
                                                AccountInfo ai = ledger[PK];

                                                if (ai.AccountState != AccountState.Normal)
                                                {
                                                    badAccountState = true;
                                                    break;
                                                }

                                            }
                                            else
                                            {
                                                // Need to create Account
                                                if (destination.Value >= Constants.FIN_MIN_BALANCE)
                                                {
                                                    AddressData ad = AddressFactory.DecodeAddressString(destination.Address);

                                                    AccountInfo ai = new AccountInfo(PK, 0, destination.Name, AccountState.Normal,
                                                        ad.NetworkType, ad.AccountType, nodeState.network_time);

                                                    temp_NewAccounts.Add(ai);
                                                }
                                                else
                                                {
                                                    badAccountCreationValue = true;
                                                    break;
                                                }

                                                if (IsGoodValidUserName(destination.Name) == false)
                                                {
                                                    badAccountName_inTransaction = true;
                                                    break;
                                                }

                                            }
                                        }


                                        // TODO: ALL WELL / Check for Transaction FEE.
                                        // TEMPORARY SINGLE NODE STUFF // DIRECT DB WRITE.
                                        // Make a list of updated accounts.
                                        if (!badSource && !badAccountCreationValue && !badAccountState && !insufficientFunds
                                            && !badAccountName_inTransaction && !badAccountAddress_inTransaction & !badTransactionFee)
                                        {
                                            newAccounts.AddRange(temp_NewAccounts);

                                            // If we are here, this means that the transaction is GOOD and should be added to the difference list.
                                            Interlocked.Increment(ref nodeState.NodeInfo.NodeDetails.TransactionsValidated);

                                            foreach (TransactionEntity source in transactionContent.Sources)
                                            {
                                                // As it is a source the known amount would be substracted from the value.
                                                Hash PK = new Hash(source.PublicKey);

                                                if (pendingDifferenceData.ContainsKey(PK)) // Update Old
                                                {
                                                    TreeDiffData treeDiffData = pendingDifferenceData[PK];
                                                    treeDiffData.RemoveValue += source.Value; // Reference updates the actual value
                                                }
                                                else // Create New
                                                {
                                                    TreeDiffData treeDiffData = new TreeDiffData();
                                                    treeDiffData.PublicKey = PK;
                                                    treeDiffData.RemoveValue += source.Value;
                                                    pendingDifferenceData.Add(PK, treeDiffData);
                                                }
                                            }

                                            foreach (TransactionEntity destination in transactionContent.Destinations)
                                            {
                                                // As it is a destination the known amount would be added to the value.
                                                Hash PK = new Hash(destination.PublicKey);

                                                if (pendingDifferenceData.ContainsKey(PK)) // Update Old
                                                {
                                                    TreeDiffData treeDiffData = pendingDifferenceData[PK];
                                                    treeDiffData.AddValue += destination.Value; // Reference updates the actual value
                                                }
                                                else // Create New
                                                {
                                                    TreeDiffData treeDiffData = new TreeDiffData();
                                                    treeDiffData.PublicKey = PK;
                                                    treeDiffData.AddValue += destination.Value;
                                                    pendingDifferenceData.Add(PK, treeDiffData);
                                                }
                                            }

                                            /// Added to difference list.
                                            acceptedTransactions.Add(transactionContent.TransactionID, transactionContent);

                                            DisplayUtils.Display("Transaction added to intermediate list : " +
                                                HexUtil.ToString(transactionContent.TransactionID.Hex), DisplayType.Info);
                                        }
                                        else
                                        {
                                            //TODO: LOG THIS and Display properly.
                                            DisplayUtils.Display("BAD Transaction : " + HexUtil.ToString(transactionContent.TransactionID.Hex) + "\n" +
                                                (badSource ? "\nbadSource" : "") +
                                                (badAccountCreationValue ? "\nbadAccountCreationValue" : "") +
                                                (insufficientFunds ? "\ninsufficientFunds" : "") +
                                                (badAccountName_inTransaction ? "\nbadAccountName_inTransaction" : "") +
                                                (badTransactionFee ? "\nbadTransactionFee" : "") +
                                                (badAccountAddress_inTransaction ? "\nbadAccountAddress_inTransaction" : "") + "\n" +

                                                JsonConvert.SerializeObject(transactionContent, Common.JsonSerializerSettings)

                                                + "\n", DisplayType.BadData);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                DisplayUtils.Display("Exception while processing transactions.", ex);
                            }

                        } //While ends { transactionContentStack }

                        ///////////////////////////////////////////////////////////////////////////////////////////
                        //  TODO: MAKE A RETRY QUEUE in case of Failures.                                        //
                        //  CRITICAL: REWRITE TO APPLY ONE TRANSACTION AT A TIME TO THE LEDGERS.                 //    
                        ///////////////////////////////////////////////////////////////////////////////////////////
                        // CRITICAL: In case of some failure we should try again with the remaining transactions. /
                        ///////////////////////////////////////////////////////////////////////////////////////////

                        ////// Create the accounts in the Ledger  /////

                        int newLedgerAccountCount = ledger.AddUpdateBatch(newAccounts);

                        if (newLedgerAccountCount != newAccounts.Count)
                        {
                            throw new Exception("Ledger batch write failure. #2");
                        }

                        Dictionary<Hash, AccountInfo> accountsInLedger;

                        int fetchedLedgerAccountsCount = ledger.BatchFetch(out accountsInLedger, pendingDifferenceData.Keys);

                        if (fetchedLedgerAccountsCount != pendingDifferenceData.Count)
                        {
                            throw new Exception("Ledger batch read failure. #2");
                        }

                        ////// Create the new accounts in the PersistentDatabase  /////

                        int newDBAccountCount = PersistentAccountStore.AddUpdateBatch(newAccounts);

                        Interlocked.Add(ref nodeState.NodeInfo.NodeDetails.TotalAccounts, newDBAccountCount);

                        if (newDBAccountCount != newAccounts.Count)
                        {
                            throw new Exception("Persistent DB batch write failure. #2");
                        }

                        Dictionary<Hash, AccountInfo> accountsInDB;

                        int fetchedAccountsCount = PersistentAccountStore.BatchFetch(out accountsInDB, pendingDifferenceData.Keys);

                        if (fetchedAccountsCount != pendingDifferenceData.Count)
                        {
                            throw new Exception("Persistent DB batch read failure. #2");
                        }

                        // We are here without exceptions 
                        // Great the accounts are ready to be written to.                    
                        // Cross-Verify that the initial account contents are the same.

                        foreach (KeyValuePair<Hash, AccountInfo> kvp in accountsInLedger)
                        {
                            if (accountsInDB.ContainsKey(kvp.Key))
                            {
                                AccountInfo ledgerAccount = kvp.Value;
                                AccountInfo persistentAccount = accountsInDB[kvp.Key];

                                if (ledgerAccount.LastTransactionTime != persistentAccount.LastTransactionTime)
                                {
                                    throw new Exception("Persistent DB or Ledger unauthorized overwrite Time. #1 : \nLedgerAccount : " +
                                     JsonConvert.SerializeObject(ledgerAccount, Common.JsonSerializerSettings) + "\nPersistentAccount :" +
                                     JsonConvert.SerializeObject(persistentAccount, Common.JsonSerializerSettings) + "\n");
                                }

                                if (ledgerAccount.Money != persistentAccount.Money)
                                {
                                    throw new Exception("Persistent DB or Ledger unauthorized overwrite Value. #1 : \nLedgerAccount : " +
                                     JsonConvert.SerializeObject(ledgerAccount, Common.JsonSerializerSettings) + "\nPersistentAccount :" +
                                     JsonConvert.SerializeObject(persistentAccount, Common.JsonSerializerSettings) + "\n");
                                }
                            }
                            else
                            {
                                throw new Exception("Improbable Assertion Failed: Persistent DB or Ledger account missing !!!");
                            }
                        }

                        // Fine, the account information is same in both the Ledger and Persistent-DB
                        // CRITICAL : NO EXCEPTION HANDLERS INSIDE !!

                        List<AccountInfo> finalPersistentDBUpdateList = new List<AccountInfo>();

                        // This essentially gets values from Ledger Tree and updates the Persistent-DB

                        DateTime CloseTime = DateTime.UtcNow;

                        foreach (KeyValuePair<Hash, TreeDiffData> kvp in pendingDifferenceData)
                        {
                            TreeDiffData diffData = kvp.Value;

                            // Apply to ledger

                            AccountInfo ledgerAccount = ledger[diffData.PublicKey];

                            DisplayUtils.Display("\nFor Account : '" + ledgerAccount.Name + "' : " + HexUtil.ToString(ledgerAccount.PublicKey.Hex), DisplayType.Info);
                            DisplayUtils.Display("Balance: " + ledgerAccount.Money + ", Added:" + diffData.AddValue + ", Removed:" + diffData.RemoveValue, DisplayType.Info);

                            ledgerAccount.Money += diffData.AddValue;
                            ledgerAccount.Money -= diffData.RemoveValue;
                            ledgerAccount.LastTransactionTime = CloseTime.ToFileTimeUtc();

                            ledger[diffData.PublicKey] = ledgerAccount;

                            // This is good enough as we have previously checked for correctness and matching
                            // values in both locations.

                            finalPersistentDBUpdateList.Add(ledgerAccount);
                        }
                        
                        LedgerCloseData ledgerCloseData;
                        bool ok = PersistentCloseHistory.GetLastRowData(out ledgerCloseData);

                        ledgerCloseData.CloseTime = CloseTime.ToFileTimeUtc();
                        ledgerCloseData.SequenceNumber++;
                        ledgerCloseData.Transactions = acceptedTransactions.Count;
                        ledgerCloseData.TotalTransactions += ledgerCloseData.Transactions;
                        ledgerCloseData.LedgerHash = ledger.GetRootHash().Hex;

                        // Apply to persistent DB.

                        PersistentCloseHistory.AddUpdate(ledgerCloseData);                        
                        PersistentAccountStore.AddUpdateBatch(finalPersistentDBUpdateList);
                        PersistentTransactionStore.AddUpdateBatch(acceptedTransactions, ledgerCloseData.SequenceNumber);
                        
                        nodeState.NodeInfo.LastLedgerInfo = new JS_LedgerInfo(ledgerCloseData);

                        // Apply the transactions to the PersistentDatabase.

                        /*while (PendingIncomingCandidates.Count > 0)
                        {
                        }

                        // Send the transcation to the TrustedNodes
                        while (PendingIncomingTransactions.Count > 0)
                        {
                        }*/
                    }

                }
                catch (Exception ex)
                {
                    DisplayUtils.Display("Timer Event : Exception : Node", ex);
                }

                // // // // // // // // //
            }
            else
            {
                DisplayUtils.Display("Timer Expired : Consensus / Node", DisplayType.Warning);
            }

            TimerEventProcessed = true;
        }

        #endregion
        
        public async Task<long> CalculateTotalMoneyInPersistentStoreAsync()
        {
            long Tres = 0;

            await PersistentAccountStore.FetchAllAccountsAsync((X) =>
            {
                Tres += X.Money;
                return TreeResponseType.NothingDone;
            });

            return Tres;
        }


        public async Task<long> CalculateTotalMoneyFromLedgerTreeAsync()
        {
            long Tres = 0;

            await Task.Run(() =>
            {
                long LeafDataCount = 0;
                long FoundNodes = 0;

                ledger.LedgerTree.TraverseAllNodes(ref LeafDataCount, ref FoundNodes, (X) =>
                {
                    foreach (AccountInfo AI in X)
                    {
                        Tres += AI.Money;
                    }
                    return TreeResponseType.NothingDone;
                });
            });

            return Tres;
        }

        public long Money
        {
            get
            {
                if (ledger != null)
                    if (ledger.AccountExists(PublicKey))
                        return ledger[PublicKey].Money;
                    else return -1;

                return -1;
            }
        }

        /*Stack<TransactionContentPack> PendingIncomingCandidates = new Stack<TransactionContentPack>();
        Stack<TransactionContentPack> PendingIncomingTransactions = new Stack<TransactionContentPack>();

        /// <summary>
        /// [TO BE CALLED BY OTHER NODES] Sends transactions to destination, only valid ones will be processed.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="Transactions"></param>
        public void SendTransaction(Hash source, TransactionContent Transaction)
        {
            PendingIncomingTransactions.Push(new TransactionContentPack(source, Transaction));
            InTransactionCount++;
        }

        /// <summary>
        /// [TO BE CALLED BY OTHER NODES] Sends candidates to destination [ONLY AFTER > 50% voting], only valid ones will be processed.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="Transactions"></param>
        public void SendCandidates(Hash source, TransactionContent[] Transactions)
        {
            if (TrustedNodes.ContainsKey(source))
            {
                foreach (TransactionContent tc in Transactions)
                {
                    PendingIncomingCandidates.Push(new TransactionContentPack(source, tc));
                    InCandidatesCount++;
                }
            }
        }*/


    }
}
