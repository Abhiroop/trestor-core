/*
 *  @Author: Arpan Jati
 *  @Version: 1.0  
 *  @Description: Node: It represents a full fledged transaction processor / validator / the complete thing.
 *  @Date: Oct 2015 | Jan-Feb 2015
 *  FEB 10 2015 - Separate out the RPC Handlers
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

    internal class Node
    {
        #region Locals

        public event NodeStatusEventHandler NodeStatusEvent;

        bool TimerEventProcessed = true;
        bool MinuteEventProcessed = true;


        // TODO: MAKE PRIVATE : AND FAST
        public NodeConfig nodeConfig = default(NodeConfig);

        // TODO: MAKE PRIVATE : AND FAST
        public NodeState nodeState = default(NodeState);

        RpcHandlers rpcHandlers = default(RpcHandlers);
        NetworkHandlers networkHandlers = default(NetworkHandlers); 
        
        public AccountInfo AI;

        #endregion

        #region ConstructorsAndTimers

        public Ledger LocalLedger
        {
            get { return nodeState.Ledger; }
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
        
        /// <summary>
        /// Initializes a node. Node ID is 0 for most cases.
        /// Only other use is hosting multiple validators from an IP (bad-idea) and simulation.
        /// </summary>
        /// <param name="ID"></param>
        public Node(int ID, GlobalConfiguration globalConfiguration)
        {
            nodeConfig = new NodeConfig(ID, globalConfiguration);

            nodeState = new NodeState(nodeConfig);

            nodeState.NodeInfo = nodeConfig.Get_JS_Info();

            rpcHandlers = new RpcHandlers(nodeConfig, nodeState);
            networkHandlers = new NetworkHandlers(nodeConfig, nodeState, globalConfiguration);

            AI = new AccountInfo(PublicKey, Money);

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
            TimerMinute.Interval = 30000;
            TimerMinute.Start();


            DisplayUtils.Display("Started Node " + nodeConfig.NodeID, DisplayType.ImportantInfo);
        }

        void TimerMinute_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (MinuteEventProcessed) // Lock to prevent multiple invocations
            {
                MinuteEventProcessed = false;

                try
                {
                    DateTime NOW = DateTime.UtcNow;

                    // Clean temporary proof of work Queue
                    // TODO : CRITICAL : Make sure the valid ones are removed, and not the critical ones.

                    Hash[] Kys = nodeState.WorkProofMap.Keys.ToArray();

                    foreach (Hash key in Kys)
                    {
                        DifficultyTimeData dtd;
                        if (nodeState.WorkProofMap.TryGetValue(key, out dtd))
                        {
                            TimeSpan span = (NOW - dtd.IssueTime);
                            if (span.TotalSeconds > 60) // 1 Minute
                            {
                                nodeState.WorkProofMap.Remove(key);
                            }
                        }
                    }

                    nodeState.TransactionStateManager.ProcessAndClear();

                }
                catch (Exception ex)
                {
                    DisplayUtils.Display("TimerMinute_Elapsed", ex);
                }
            }
            else
            {
                DisplayUtils.Display("Timer Expired : TimerMinute", DisplayType.Warning);
            }

            MinuteEventProcessed = true;
        }

        //WorkProofMap

        void TimerSecond_Elapsed(object sender, ElapsedEventArgs e)
        {
            nodeState.NodeInfo.NodeDetails.TimeUTC = DateTime.UtcNow;
            nodeState.NodeInfo.LastLedgerInfo.Hash = nodeState.Ledger.GetRootHash().Hex;

            nodeState.NodeInfo.NodeDetails.ProofOfWorkQueueLength = nodeState.WorkProofMap.Count;

            nodeState.SystemTime = DateTime.UtcNow.ToFileTimeUtc();
            nodeState.NetworkTime = DateTime.UtcNow.ToFileTimeUtc();

            if (NodeStatusEvent != null)
            {
                var json = JsonConvert.SerializeObject(nodeState.NodeInfo.GetResponse(), Common.JsonSerializerSettings);
                NodeStatusEvent(json, nodeConfig.NodeID);
            }
        }

        /// <summary>
        /// Add more content to be loaded in background here.
        /// </summary>
        async public Task BeginBackgroundLoad()
        {
            await Task.Run(async () =>
            {
                long records = await nodeState.Ledger.InitializeLedger();

                Interlocked.Add(ref nodeState.NodeInfo.NodeDetails.TotalAccounts, records);
                
                await networkHandlers.InitialConnectAsync();
                
                LedgerCloseData ledgerCloseData;
                nodeState.PersistentCloseHistory.GetLastRowData(out ledgerCloseData);
                nodeState.NodeInfo.LastLedgerInfo = new JS_LedgerInfo(ledgerCloseData);
            });
        }

        #endregion        

        public void StopNode()
        {
            Constants.ApplicationRunning = false;

            networkHandlers.Stop();
            rpcHandlers.StopServer();
        }


        #region TRANSACTION PROCESSING

        /// <summary>
        /// Timer now fast. Actual / Final timer would depend on consensus rate.
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
                    if (nodeState.IncomingTransactionMap.IncomingTransactions.Count > 0)
                    {
                        Queue<TransactionContent> transactionContentStack = new Queue<TransactionContent>();

                        lock (nodeState.IncomingTransactionMap.transactionLock)
                        {
                            foreach (KeyValuePair<Hash, TransactionContent> kvp in nodeState.IncomingTransactionMap.IncomingTransactions)
                            {
                                transactionContentStack.Enqueue(kvp.Value);

                                Interlocked.Increment(ref nodeState.NodeInfo.NodeDetails.TransactionsVerified);
                            }

                            nodeState.IncomingTransactionMap.IncomingTransactions.Clear();
                            nodeState.IncomingTransactionMap.IncomingPropagations_ALL.Clear();
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
                                    if (nodeState.PersistentTransactionStore.FetchTransaction(out transactionFromPersistentDB, out sequenceNumber,
                                        transactionContent.TransactionID) == DBResponse.FetchSuccess)
                                    {
                                        //TODO: LOG THIS and Display properly.
                                        DisplayUtils.Display("Transaction Processed. Improbable because of previous checks.", DisplayType.BadData);
                                    }
                                    else
                                    {
                                        // True if BAD
                                        bool badTX_SourceDoesNotExist = false;
                                        bool badTX_AccountName = false;
                                        bool badTX_AccountAddress = false;
                                        bool badTX_AccountCreationValue = false;
                                        bool badTX_AccountState = false;
                                        bool badTX_TransactionFee = false;
                                        bool badTX_InsufficientFunds = false;

                                        List<AccountInfo> temp_NewAccounts = new List<AccountInfo>();

                                        // Check if account name in destination is valid.

                                        foreach (TransactionEntity te in transactionContent.Destinations)
                                        {

                                            if (!Utils.ValidateUserName(te.Name))
                                            {
                                                badTX_AccountName = true; // Names should be lowercase.
                                                break;
                                            }
                                            
                                            AccountInfo ai;
                                            if (nodeState.PersistentAccountStore.FetchAccount(out ai, new Hash(te.PublicKey)) == DBResponse.FetchSuccess)
                                            {
                                                // Account Exists
                                                if (ai.Name != te.Name)
                                                {
                                                    badTX_AccountName = true;
                                                    break;
                                                }

                                                string Addr = ai.GetAddress();

                                                if (Addr != te.Address)
                                                {
                                                    badTX_AccountAddress = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                if (!badTX_AccountName)
                                                {
                                                    // Check if same named account exists. When, public key could not be fetched.
                                                    if (nodeState.PersistentAccountStore.FetchAccount(out ai, te.Name) == DBResponse.FetchSuccess)
                                                    {
                                                        // Thats too bad, transaction cannot happen, 
                                                        // new wallet has invalid Name (name already used).
                                                        badTX_AccountName = true;
                                                    }
                                                    else
                                                    {

                                                    }
                                                }
                                            }
                                        }

                                        if (!Common.IsTransactionFeeEnabled) // Transaction fee not allowed here !!
                                        {
                                            if (transactionContent.TransactionFee > 0)
                                            {
                                                badTX_TransactionFee = true;
                                            }
                                        }

                                        totalTransactionFees += transactionContent.TransactionFee;

                                        foreach (TransactionEntity source in transactionContent.Sources)
                                        {
                                            Hash pkSource = new Hash(source.PublicKey);

                                            if (nodeState.Ledger.AccountExists(pkSource))
                                            {
                                                AccountInfo account = nodeState.Ledger[pkSource];

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
                                                        badTX_InsufficientFunds = true;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    badTX_AccountState = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                badTX_SourceDoesNotExist = true;
                                                break;
                                            }
                                        }

                                        /// Check Destinations

                                        foreach (TransactionEntity destination in transactionContent.Destinations)
                                        {
                                            Hash PK = new Hash(destination.PublicKey);

                                            if (nodeState.Ledger.AccountExists(PK))
                                            {
                                                // Perfect
                                                AccountInfo ai = nodeState.Ledger[PK];

                                                if (ai.AccountState != AccountState.Normal)
                                                {
                                                    badTX_AccountState = true;
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
                                                        ad.NetworkType, ad.AccountType, nodeState.NetworkTime);

                                                    temp_NewAccounts.Add(ai);
                                                }
                                                else
                                                {
                                                    badTX_AccountCreationValue = true;
                                                    break;
                                                }

                                                if (nodeState.IsGoodValidUserName(destination.Name) == false)
                                                {
                                                    badTX_AccountName = true;
                                                    break;
                                                }
                                            }
                                        }

                                        // TODO: ALL WELL / Check for Transaction FEE.
                                        // TEMPORARY SINGLE NODE STUFF // DIRECT DB WRITE.
                                        // Make a list of updated accounts.
                                        if (!badTX_SourceDoesNotExist && !badTX_AccountCreationValue && !badTX_AccountState && !badTX_InsufficientFunds
                                            && !badTX_AccountName && !badTX_AccountAddress & !badTX_TransactionFee)
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

                                            nodeState.TransactionStateManager.Set(transactionContent.TransactionID, TransactionProcessingResult.PR_Validated);
                                        }
                                        else
                                        {
                                            TransactionProcessingResult rs = TransactionProcessingResult.Unprocessed;

                                            if (badTX_SourceDoesNotExist) rs = TransactionProcessingResult.PR_SourceDoesNotExist;
                                            if (badTX_AccountCreationValue) rs = TransactionProcessingResult.PR_BadAccountCreationValue;
                                            if (badTX_AccountState) rs = TransactionProcessingResult.PR_BadAccountState;
                                            if (badTX_InsufficientFunds) rs = TransactionProcessingResult.PR_BadInsufficientFunds;
                                            if (badTX_AccountName) rs = TransactionProcessingResult.PR_BadAccountName;
                                            if (badTX_TransactionFee) rs = TransactionProcessingResult.PR_BadTransactionFee;
                                            if (badTX_AccountAddress) rs = TransactionProcessingResult.PR_BadAccountAddress;

                                            nodeState.TransactionStateManager.Set(transactionContent.TransactionID, rs);

                                            //TODO: LOG THIS and Display properly.
                                            DisplayUtils.Display("BAD Transaction : " + HexUtil.ToString(transactionContent.TransactionID.Hex) + "\n" +
                                                (badTX_SourceDoesNotExist ? "\nbadTX_SourceDoesNotExist" : "") +
                                                (badTX_AccountCreationValue ? "\nbadTX_AccountCreationValue" : "") +
                                                (badTX_AccountState ? "\nbadTX_AccountState" : "") +
                                                (badTX_InsufficientFunds ? "\nbadTX_InsufficientFunds" : "") +
                                                (badTX_AccountName ? "\nbadTX_AccountName" : "") +
                                                (badTX_TransactionFee ? "\nbadTX_TransactionFee" : "") +
                                                (badTX_AccountAddress ? "\nbadTX_AccountAddress" : "") + "\n" +

                                                JsonConvert.SerializeObject(transactionContent, Common.JsonSerializerSettings)

                                                + "\n", DisplayType.BadData);
                                        }

                                        nodeState.TransactionStateManager.Set(transactionContent.TransactionID, TransactionStatusType.Processed);

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

                        int newLedgerAccountCount = nodeState.Ledger.AddUpdateBatch(newAccounts);

                        if (newLedgerAccountCount != newAccounts.Count)
                        {
                            throw new Exception("Ledger batch write failure. #2");
                        }

                        Dictionary<Hash, AccountInfo> accountsInLedger;

                        int fetchedLedgerAccountsCount = nodeState.Ledger.BatchFetch(out accountsInLedger, pendingDifferenceData.Keys);

                        if (fetchedLedgerAccountsCount != pendingDifferenceData.Count)
                        {
                            throw new Exception("Ledger batch read failure. #2");
                        }

                        ////// Create the new accounts in the PersistentDatabase  /////

                        int newDBAccountCount = nodeState.PersistentAccountStore.AddUpdateBatch(newAccounts);

                        Interlocked.Add(ref nodeState.NodeInfo.NodeDetails.TotalAccounts, newDBAccountCount);

                        if (newDBAccountCount != newAccounts.Count)
                        {
                            throw new Exception("Persistent DB batch write failure. #2");
                        }

                        Dictionary<Hash, AccountInfo> accountsInDB;

                        int fetchedAccountsCount = nodeState.PersistentAccountStore.BatchFetch(out accountsInDB, pendingDifferenceData.Keys);

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

                            AccountInfo ledgerAccount = nodeState.Ledger[diffData.PublicKey];

                            DisplayUtils.Display("\nFor Account : '" + ledgerAccount.Name + "' : " + HexUtil.ToString(ledgerAccount.PublicKey.Hex), DisplayType.Info);
                            DisplayUtils.Display("Balance: " + ledgerAccount.Money + ", Added:" + diffData.AddValue + ", Removed:" + diffData.RemoveValue, DisplayType.Info);

                            ledgerAccount.Money += diffData.AddValue;
                            ledgerAccount.Money -= diffData.RemoveValue;
                            ledgerAccount.LastTransactionTime = CloseTime.ToFileTimeUtc();

                            nodeState.Ledger[diffData.PublicKey] = ledgerAccount;

                            // This is good enough as we have previously checked for correctness and matching
                            // values in both locations.

                            finalPersistentDBUpdateList.Add(ledgerAccount);
                        }

                        LedgerCloseData ledgerCloseData;
                        bool ok = nodeState.PersistentCloseHistory.GetLastRowData(out ledgerCloseData);

                        ledgerCloseData.CloseTime = CloseTime.ToFileTimeUtc();
                        ledgerCloseData.SequenceNumber++;
                        ledgerCloseData.Transactions = acceptedTransactions.Count;
                        ledgerCloseData.TotalTransactions += ledgerCloseData.Transactions;
                        ledgerCloseData.LedgerHash = nodeState.Ledger.GetRootHash().Hex;

                        // Apply to persistent DB.

                        nodeState.PersistentCloseHistory.AddUpdate(ledgerCloseData);
                        nodeState.PersistentAccountStore.AddUpdateBatch(finalPersistentDBUpdateList);
                        nodeState.PersistentTransactionStore.AddUpdateBatch(acceptedTransactions, ledgerCloseData.SequenceNumber);

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

            await nodeState.PersistentAccountStore.FetchAllAccountsAsync((X) =>
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

                nodeState.Ledger.LedgerTree.TraverseAllNodes(ref LeafDataCount, ref FoundNodes, (X) =>
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
                if (nodeState.Ledger != null)
                    if (nodeState.Ledger.AccountExists(PublicKey))
                        return nodeState.Ledger[PublicKey].Money;
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
