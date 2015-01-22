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

namespace TNetD.Nodes
{


    internal class Node : Responder
    {
        bool TimerEventProcessed = true;

        SecureNetwork network = default(SecureNetwork);

        RESTServer restServer = default(RESTServer);

        NodeState nodeState = new NodeState();

        IncomingTransactionMap incomingTransactionMap;

        /// <summary>
        /// A dictionary of Trusted nodes, stored by PublicKey
        /// </summary>
        public Dictionary<Hash, NodeSocketData> TrustedNodes;

        public int OutTransactionCount = 0;
        public int InCandidatesCount;
        public int InTransactionCount;

        public AccountInfo AI;

        public IPersistentAccountStore PersistentAccountStore;
        public ITransactionStore TransactionStore;

        Ledger ledger;

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

        System.Timers.Timer Tmr;

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

            incomingTransactionMap = new IncomingTransactionMap(nodeState, nodeConfig);

            network = new SecureNetwork(nodeConfig);
            network.PacketReceived += network_PacketReceived;

            network.Initialize();

            TrustedNodes = globalConfiguration.TrustedNodes;

            PersistentAccountStore = new SQLiteAccountStore(nodeConfig);
            TransactionStore = new SQLiteTransactionStore(nodeConfig);

            AI = new AccountInfo(PublicKey, Money);

            ledger = new Ledger(PersistentAccountStore);


            //ledger.AddUserToLedger(AI);

            Tmr = new System.Timers.Timer();
            Tmr.Elapsed += Tmr_Elapsed;
            Tmr.Enabled = true;
            Tmr.Interval = nodeConfig.UpdateFrequencyMS;
            Tmr.Start();

            // ////////////////////


            ////////////////////////

            restServer = new RESTServer("localhost", nodeConfig.ListenPortRPC.ToString(), "http", "index.html", null, 5, RPCRequestHandler);

            restServer.Start();
        }

        /// <summary>
        /// Add more content to be loaded in background here.
        /// </summary>       
        async public void BeginBackgroundLoad()
        {
            await Task.Run(async () =>
            {
                ledger.InitializeLedger();

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

            });
        }


        #region RPC HANDLING

        // RPCRequestHandler rpcRequestHandler;
        bool RPCRequestHandler(HttpListenerContext context)
        {
            if (context.Request.RawUrl.Matches(@"^/info")) // ProcessInfo 
            {
                this.SendJsonResponse(context, nodeConfig.Get_JS_Info().GetResponse());

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
            else if (context.Request.RawUrl.Matches(@"^/accounts")) // Fetch Transactions
            {
                if (context.Request.RawUrl.StartsWith("/accounts"))
                {
                    HandleAccountQuery(context);
                    return true;
                }
            }

            return false;
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

            if (replies.TransactionState.Count == 1)
            {
                this.SendJsonResponse(context, replies.TransactionState[0].GetResponse());
            }
            else if (replies.TransactionState.Count > 1)
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
            // incomingTransactionMap


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
                                if (ledger.NameAccountInfoMap.ContainsKey(name))
                                {
                                    AccountInfo _ai = ledger.NameAccountInfoMap[name];
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

            if (replies.Accounts.Count == 1)
            {
                this.SendJsonResponse(context, replies.Accounts[0].GetResponse());
            }
            else if (replies.Accounts.Count > 1)
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
                                TransactionContent tcxo;
                                if (TransactionStore.FetchTransaction(out tcxo, new Hash(transactionID_Bytes)) == DBResponse.FetchSuccess)
                                {
                                    replies.Transactions.Add(new JS_TransactionReply(tcxo));
                                }
                            }
                        }

                        break;

                    default:

                        break;
                }
            }

            // // /////////////////////////////////////////////////////////////////////

            if (replies.Transactions.Count == 1)
            {
                this.SendJsonResponse(context, replies.Transactions[0].GetResponse());
            }
            else if (replies.Transactions.Count > 1)
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
                        JS_TransactionReply jtr = JsonConvert.DeserializeObject<JS_TransactionReply>(inputStream.ReadToEnd(),
                            Common.JsonSerializerSettings);

                        transactionContent.Deserialize(jtr);
                    }

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

            /*if (background_Load != null)
            {
                if (background_Load.IsAlive)
                {
                    background_Load.Abort();
                }
            }*/
        }

        async Task SendInitialize(Hash publicKey)
        {
            await Task.Delay(Constants.random.Next(500, 1000)); // Wait a random delay before connecting.
            NetworkPacketQueueEntry npqe = new NetworkPacketQueueEntry(publicKey, new NetworkPacket(PublicKey, PacketType.TPT_HELLO, new byte[0]));
            network.AddToQueue(npqe);
        }

        class TreeDiffData
        {
            public Hash PublicKey { get; set; }
            public long RemoveValue { get; set; }
            public long AddValue { get; set; }
        }

        private void Tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (TimerEventProcessed) // Lock to prevent multiple invocations 
            {
                TimerEventProcessed = false;

                // // // // // // // // //

                try
                {
                    Stack<TransactionContent> transactionContentStack = new Stack<TransactionContent>();

                    foreach (KeyValuePair<Hash, TransactionContent> kvp in incomingTransactionMap.IncomingTransactions)
                    {
                        transactionContentStack.Push(kvp.Value);
                    }

                    incomingTransactionMap.IncomingTransactions.Clear();

                    Dictionary<Hash, TreeDiffData> pendingDifferenceData = new Dictionary<Hash, TreeDiffData>();
                    List<AccountInfo> newAccounts = new List<AccountInfo>();

                    while (transactionContentStack.Count > 0)
                    {
                        TransactionContent transactionContent = transactionContentStack.Pop();

                        try
                        {
                            if (transactionContent.VerifySignature() == TransactionProcessingResult.Accepted)
                            {
                                /// Check Sources.

                                bool badSource = false;
                                bool badAccountState = false;
                                bool insufficientFunds = false;

                                foreach (TransactionEntity source in transactionContent.Sources)
                                {
                                    Hash PK = new Hash(source.PublicKey);

                                    if (ledger.AccountExists(PK))
                                    {
                                        AccountInfo account = ledger[PK];

                                        long PendingValueDifference = 0;

                                        // Check if the account exists in the pending transaction queue.
                                        if (pendingDifferenceData.ContainsKey(PK))
                                        {
                                            TreeDiffData treeDiffData = pendingDifferenceData[PK];

                                            PendingValueDifference += treeDiffData.AddValue;
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

                                bool badAccountCreationValue = false;

                                

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

                                        if (destination.Value > Constants.FIN_MIN_BALANCE)
                                        {
                                            AddressData ad = AddressFactory.DecodeAddressString(destination.Address);

                                            AccountInfo ai = new AccountInfo(PK, 0, destination.Name, AccountState.Normal,
                                                ad.NetworkType, ad.AccountType, nodeState.network_time);

                                            newAccounts.Add(ai);

                                        }
                                        else
                                        {
                                            badAccountCreationValue = true;
                                            break;
                                        }
                                    }
                                }

                                // TODO: ALL WELL / Check for tx FEE.

                                /// TEMPORARY SINGLE NODE STUFF // DIRECT DB WRITE.

                                //Make a list of updated accounts.

                                if (!badSource && !badAccountCreationValue && !badAccountState && !insufficientFunds)
                                {
                                    // If we are here, this means that the transaction is GOOD and should be added to the difference list.

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

                                }
                                else
                                {
                                    //TODO: LOG THIS and Display properly.
                                    DisplayUtils.Display("BAD TRANSACTION");
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            DisplayUtils.Display("Exception while processing transactions.", ex);
                        }

                    } //While ends

                    // Create the new accounts in the PersistentDatabase.

                    int newCount = PersistentAccountStore.AddUpdateBatch(newAccounts);

                    if(newCount != newAccounts.Count)
                    {
                        throw new Exception("Persistent database batch write failure. #2");
                    }

                    List<AccountInfo> accountsInDB;

                    int fetchedAccountsCount = PersistentAccountStore.BatchFetch(out accountsInDB, pendingDifferenceData.Keys);

                    if(fetchedAccountsCount != pendingDifferenceData.Count)
                    {
                        throw new Exception("Persistent database batch read failure. #2");
                    }

                    // Next is to populate the ledger and then mark the accounts to save to persistent db.
                    // then save the marked account.


                    // Apply the transactions to the PersistentDatabase.

                    /*while (PendingIncomingCandidates.Count > 0)
                    {
                    }

                    // Send the transcation to the TrustedNodes
                    while (PendingIncomingTransactions.Count > 0)
                    {
                    }*/
                }
                catch
                {
                    DisplayUtils.Display("Timer Event : Exception : Node", DisplayType.Warning);
                }

                // // // // // // // // //
            }
            else
            {
                DisplayUtils.Display("Timer Expired : Node", DisplayType.Warning);
            }

            TimerEventProcessed = true;
        }






        void CreateArbitraryTransactionAndSendToTrustedNodes()
        {
            //List<TransactionSink> tsks = new List<TransactionSink>();

            //Node destNode = Constants.GlobalNodeList[Constants.random.Next(0, Constants.GlobalNodeList.Count)];

            //if (destNode.PublicKey != PublicKey)
            //{
            //    int Amount = Constants.random.Next(0, (int)(Money / 2));

            //    TransactionSink tsk = new TransactionSink(destNode.PublicKey, Amount);
            //    tsks.Add(tsk);

            //    TransactionContent tco = new TransactionContent(PublicKey, 0, tsks.ToArray(), new byte[0]);

            //    OutTransactionCount++;
            //}
        }

        void InitializeValuesFromGlobalLedger()
        {

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

        Stack<TransactionContentPack> PendingIncomingCandidates = new Stack<TransactionContentPack>();
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
        }


    }
}
