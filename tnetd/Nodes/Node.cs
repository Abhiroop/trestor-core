﻿/*
 *  @Author: Arpan Jati
 *  @Version: 1.0
 *  @Date: Oct 2015 | Jan 2015
 *  @Description: Node: It represents a full fledged transaction processor / validator / the complete thing.
 */

using Chaos.NaCl;
using Grapevine;
using Grapevine.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TNetD.Json.JS_Structs;
using TNetD.Ledgers;
using TNetD.Network;
using TNetD.Network.Networking;
using TNetD.PersistentStore;
using TNetD.Transactions;

namespace TNetD.Nodes
{
    public delegate bool RPCRequestHandler(HttpListenerContext context);

    internal class Node : Responder
    {
        SecureNetwork network = default(SecureNetwork);

        RESTServer restServer = default(RESTServer);

        //public Dictionary<Hash, Node> TrustedNodes = new Dictionary<Hash, Node>();

        /// <summary>
        /// Outer Dict is TransactionID, inner is Voter node.
        /// </summary>
        public Dictionary<Hash, Dictionary<Hash, CandidateStatus>> ReceivedCandidates = new Dictionary<Hash, Dictionary<Hash, CandidateStatus>>();

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

        Timer Tmr;

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

            network = new SecureNetwork(nodeConfig);
            network.PacketReceived += network_PacketReceived;

            TrustedNodes = globalConfiguration.TrustedNodes;

            network.Initialize();

            PersistentAccountStore = new SQLiteAccountStore(nodeConfig);
            TransactionStore = new SQLiteTransactionStore(nodeConfig);

            AI = new AccountInfo(PublicKey, Money);

            ledger = new Ledger(PersistentAccountStore);

            ledger.AddUserToLedger(AI);

            Tmr = new Timer();
            Tmr.Elapsed += Tmr_Elapsed;
            Tmr.Enabled = true;
            Tmr.Interval = nodeConfig.UpdateFrequencyMS;
            Tmr.Start();

            // ////////////////////

            // Connect to TrustedNodes
            // List<Task> tasks = new List<Task>();

            foreach (KeyValuePair<Hash, NodeSocketData> kvp in TrustedNodes)
            {
                if (kvp.Key != PublicKey) // Make sure we are not connecting to self !!
                {
                    if (!network.IsConnected(kvp.Key))
                    {
                        SendInitialize(kvp.Key);
                    }
                }
            }

            ////////////////////////

            restServer = new RESTServer("localhost", nodeConfig.ListenPortRPC.ToString(), "http", "index.html", null, 5, RPCRequestHandler);

            restServer.Start();
        }

        // RPCRequestHandler rpcRequestHandler;
        bool RPCRequestHandler(HttpListenerContext context)
        {
            if (context.Request.RawUrl.Matches(@"^/q")) // ProcessQueries
            {
                if (context.Request.RawUrl.StartsWith("/q"))
                {
                    foreach (string key in context.Request.QueryString.AllKeys)
                    {
                        switch (key)
                        {
                            case "address":


                                break;

                            default:

                                break;
                        }
                    }
                }

                this.SendJsonResponse(context, new JS_Resp());

                return true;
            }
            else if (context.Request.RawUrl.Matches(@"^/info")) // ProcessInfo 
            {
                this.SendJsonResponse(context, nodeConfig.Get_JS_Info().GetResponse());

                return true;
            }
            else if (context.Request.RawUrl.Matches(@"^/propagate")) // Propagate Transactions 
            {
                HandlePropagate(context);

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

                        string[] txids = context.Request.QueryString["id"].Split(',');

                        foreach (string txid in txids)
                        {
                            byte[] _txid = new byte[0];

                            try
                            {
                                _txid = HexUtil.GetBytes(txid);
                            }
                            catch { }

                            if (_txid.Length == 32)
                            {
                                TransactionContent tcxo;
                                if (TransactionStore.FetchTransaction(out tcxo, new Hash(_txid)) == DBResponse.FetchSuccess)
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

        private void HandlePropagate(HttpListenerContext context)
        {
            JS_Msg msg;

            if (context.Request.HttpMethod.ToUpper().Equals("POST") && context.Request.HasEntityBody &&
                context.Request.ContentLength64 < Constants.PREFS_MAX_RPC_POST_CONTENT_LENGTH)
            {
                StreamReader sr = new StreamReader(context.Request.InputStream);

                try
                {
                    byte[] data = HexUtil.GetBytes(sr.ReadToEnd());
                    TransactionContent tco = new TransactionContent();
                    tco.Deserialize(data);

                    if (tco.VerifySignature())
                    {
                        msg = new JS_Msg("Transaction Added to propagation queue.", RPCStatus.Success);
                    }
                    else
                    {
                        msg = new JS_Msg("Signature Verification Failed.", RPCStatus.Failure);
                    }
                }
                catch
                {
                    msg = new JS_Msg("Malformed Transaction.", RPCStatus.Failure);
                }
            }
            else
            {
                msg = new JS_Msg("Improper usage. Need to use POST and Transaction Proposal as Content.", RPCStatus.InvalidAPIUsage);
            }

            this.SendJsonResponse(context, msg.GetResponse());
        }

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

        void SendInitialize(Hash publicKey)
        {
            //await Task.Delay(Constants.random.Next(500, 1000)); // Wait a random delay before connecting.
            NetworkPacketQueueEntry npqe = new NetworkPacketQueueEntry(publicKey, new NetworkPacket(PublicKey, PacketType.TPT_HELLO, new byte[0]));
            network.AddToQueue(npqe);
        }

        private void Tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            //await Task.WhenAll(tasks.ToArray());

            while (PendingIncomingCandidates.Count > 0)
            {

            }

            // Send the transcation to the TrustedNodes
            while (PendingIncomingTransactions.Count > 0)
            {

            }
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
