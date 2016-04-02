
/* 
    [ULTRA CRITICAL]:   BODGED UP CODE FROM PREVIOUS 'TransactionHandler'
                        NEEDS CAREFUL REVIEW !!!
*/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TNetD.Address;
using TNetD.Json.JS_Structs;
using TNetD.Nodes;
using TNetD.PersistentStore;
using TNetD.Transactions;

namespace TNetD.Consensus
{
    class TransactionHandlingData
    {
        public Dictionary<Hash, TreeDiffData> PendingDifferenceData { get; set; }
        public Dictionary<Hash, TransactionContent> AcceptedTransactions { get; set; }
        public List<AccountInfo> NewAccounts { get; set; }

        public TransactionHandlingData()
        {
            PendingDifferenceData = new Dictionary<Hash, TreeDiffData>();
            AcceptedTransactions = new Dictionary<Hash, TransactionContent>();
            NewAccounts = new List<AccountInfo>();
        }
    }

    class TransactionValidator
    {
        NodeConfig nodeConfig;
        NodeState nodeState;

        public TransactionValidator(NodeConfig nodeConfig, NodeState nodeState)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;
        }

        public TransactionHandlingData ValidateTransactions(IEnumerable<TransactionContent> transactions)
        {
            long totalTransactionFees = 0;

            Dictionary<Hash, TreeDiffData> pendingDifferenceData = new Dictionary<Hash, TreeDiffData>();
            Dictionary<Hash, TransactionContent> acceptedTransactions = new Dictionary<Hash, TransactionContent>();
            List<AccountInfo> newAccounts = new List<AccountInfo>();

            foreach (TransactionContent transactionContent in transactions)
            {
                try
                {
                    var txSigVerificationResult = transactionContent.VerifySignature();

                    if (txSigVerificationResult == TransactionProcessingResult.Accepted)
                    {
                        TransactionContent transactionFromPersistentDB;
                        long sequenceNumber;
                        if (nodeState.Persistent.TransactionStore.FetchTransaction(out transactionFromPersistentDB, out sequenceNumber,
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
                            bool badTX_TransactionAmountLimit = false;
                            bool badTX_InsufficientFunds = false;

                            List<AccountInfo> temp_NewAccounts = new List<AccountInfo>();

                            // Check if account name in destination is valid.

                            if (transactionContent.Value > Constants.FIN_MAX_TRE_PER_TX)
                            {
                                badTX_TransactionAmountLimit = true;
                            }

                            foreach (TransactionEntity te in transactionContent.Destinations)
                            {

                                if (!Utils.ValidateUserName(te.Name))
                                {
                                    badTX_AccountName = true; // Names should be lowercase.
                                    break;
                                }

                                AccountInfo ai;
                                if (nodeState.Persistent.AccountStore.FetchAccount(out ai, new Hash(te.PublicKey))
                                    == DBResponse.FetchSuccess)
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
                                        if (nodeState.Persistent.AccountStore.FetchAccount(out ai, te.Name) == DBResponse.FetchSuccess)
                                        {
                                            // Thats too bad, transaction cannot happen, 
                                            // new wallet has invalid Name (name already used).
                                            badTX_AccountName = true;
                                            break;
                                        }
                                        else
                                        {

                                        }
                                    }
                                }
                            }

                            if (!Common.TRANSACTION_FEE_ENABLED) // Transaction fee not allowed here !!
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

                                    long removeValueForAccount = 0;

                                    if (source.Value > Constants.FIN_MAX_TRE_PER_TX_ENTITY)
                                    {
                                        badTX_TransactionAmountLimit = true;
                                        break;
                                    }

                                    // Check if the account exists in the pending transaction queue.
                                    if (pendingDifferenceData.ContainsKey(pkSource))
                                    {
                                        removeValueForAccount = pendingDifferenceData[pkSource].RemoveValue;
                                    }

                                    if (account.AccountState == AccountState.Normal)
                                    {
                                        long sourceValue = source.Value;
                                        long creditFromAccount = (removeValueForAccount + Constants.FIN_MIN_BALANCE);
                                        long availableBalance = (account.Money - creditFromAccount);

                                        // Lots of checks :D
                                        if ((sourceValue > 0) && (availableBalance > 0) &&
                                            (account.Money >= creditFromAccount) &&
                                            (creditFromAccount >= 0) && (availableBalance >= sourceValue))
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

                            // Check Destinations

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
                                && !badTX_TransactionAmountLimit && !badTX_AccountName && !badTX_AccountAddress & !badTX_TransactionFee)
                            {
                                newAccounts.AddRange(temp_NewAccounts);

                                // If we are here, this means that the transaction is GOOD and should be added to the difference list.
                                Interlocked.Increment(ref nodeState.NodeInfo.NodeDetails.TransactionsValidated);

                                foreach (TransactionEntity source in transactionContent.Sources)
                                {
                                    // As it is a source, the known amount would be substracted from the value.
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

                                // Added to difference list.
                                acceptedTransactions.Add(transactionContent.TransactionID, transactionContent);

                                //DisplayUtils.Display("Transaction added to intermediate list : " +
                                //  HexUtil.ToString(transactionContent.TransactionID.Hex), DisplayType.Info);

                                nodeState.TransactionStateManager.Set(transactionContent.TransactionID, TransactionProcessingResult.PR_Validated);
                            }
                            else
                            {
                                TransactionProcessingResult rs = TransactionProcessingResult.Unprocessed;

                                if (badTX_SourceDoesNotExist) rs = TransactionProcessingResult.PR_SourceDoesNotExist;
                                if (badTX_AccountCreationValue) rs = TransactionProcessingResult.PR_BadAccountCreationValue;
                                if (badTX_AccountState) rs = TransactionProcessingResult.PR_BadAccountState;
                                if (badTX_InsufficientFunds) rs = TransactionProcessingResult.PR_BadInsufficientFunds;
                                if (badTX_TransactionAmountLimit) rs = TransactionProcessingResult.PR_BadTransactionAmountLimit;
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
                                    (badTX_TransactionAmountLimit ? "\nbadTX_TransactionAmountLimit" : "") +
                                    (badTX_AccountName ? "\nbadTX_AccountName" : "") +
                                    (badTX_TransactionFee ? "\nbadTX_TransactionFee" : "") +
                                    (badTX_AccountAddress ? "\nbadTX_AccountAddress" : "") + "\n" +

                                    JsonConvert.SerializeObject(transactionContent, Common.JSON_SERIALIZER_SETTINGS)

                                    + "\n", DisplayType.BadData);
                            }

                            nodeState.TransactionStateManager.Set(transactionContent.TransactionID, TransactionStatusType.Processed);
                        }
                    }
                    else
                    {
                        // txSigVerificationResult => FAILED
                        // TODO: Do LOG.
                    }
                }
                catch (Exception ex)
                {
                    DisplayUtils.Display("Exception while processing transactions.", ex);
                }
            }

            TransactionHandlingData thd = new TransactionHandlingData();

            thd.AcceptedTransactions = acceptedTransactions;
            thd.PendingDifferenceData = pendingDifferenceData;
            thd.NewAccounts = newAccounts;

            return thd;
        }

        public void ApplyTransactions(TransactionHandlingData thd, long? CloseTime = null)
        {
            Dictionary<Hash, TreeDiffData> pendingDifferenceData = thd.PendingDifferenceData;
            Dictionary<Hash, TransactionContent> acceptedTransactions = thd.AcceptedTransactions;
            List<AccountInfo> newAccounts = thd.NewAccounts;

            ////////////////////////////////////////////////////////////////////////////////////////////
            //  TODO: MAKE A RETRY QUEUE in case of Failures.                                         //
            //  CRITICAL: REWRITE TO APPLY ONE TRANSACTION AT A TIME TO THE LEDGERS.                  //   
            ////////////////////////////////////////////////////////////////////////////////////////////
            // CRITICAL: In case of some failure we should try again with the remaining transactions. //
            ////////////////////////////////////////////////////////////////////////////////////////////

            ////// Create the accounts in the Ledger  /////

            if (acceptedTransactions.Any())
            {

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

                int newDBAccountCount = nodeState.Persistent.AccountStore.AddUpdateBatch(newAccounts);

                Interlocked.Add(ref nodeState.NodeInfo.NodeDetails.TotalAccounts, newDBAccountCount);

                if (newDBAccountCount != newAccounts.Count)
                {
                    throw new Exception("Persistent DB batch write failure. #2");
                }

                Dictionary<Hash, AccountInfo> accountsInDB;

                int fetchedAccountsCount = nodeState.Persistent.AccountStore.BatchFetch(out accountsInDB, pendingDifferenceData.Keys);

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
                             JsonConvert.SerializeObject(ledgerAccount, Common.JSON_SERIALIZER_SETTINGS) + "\nPersistentAccount :" +
                             JsonConvert.SerializeObject(persistentAccount, Common.JSON_SERIALIZER_SETTINGS) + "\n");
                        }

                        if (ledgerAccount.Money != persistentAccount.Money)
                        {
                            throw new Exception("Persistent DB or Ledger unauthorized overwrite Value. #1 : \nLedgerAccount : " +
                             JsonConvert.SerializeObject(ledgerAccount, Common.JSON_SERIALIZER_SETTINGS) + "\nPersistentAccount :" +
                             JsonConvert.SerializeObject(persistentAccount, Common.JSON_SERIALIZER_SETTINGS) + "\n");
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

                long closeTime = CloseTime ?? DateTime.UtcNow.ToFileTimeUtc();

                // This essentially gets values from Ledger Tree and updates the Persistent-DB

                foreach (KeyValuePair<Hash, TreeDiffData> kvp in pendingDifferenceData)
                {
                    TreeDiffData diffData = kvp.Value;

                    // Apply to ledger

                    AccountInfo ledgerAccount = nodeState.Ledger[diffData.PublicKey];

                    // DisplayUtils.Display("\nFor Account : '" + ledgerAccount.Name + "' : " + HexUtil.ToString(ledgerAccount.PublicKey.Hex), DisplayType.Info);
                    // DisplayUtils.Display("Balance: " + ledgerAccount.Money + ", Added:" + diffData.AddValue + ", Removed:" + diffData.RemoveValue, DisplayType.Info);

                    ledgerAccount.Money += diffData.AddValue;
                    ledgerAccount.Money -= diffData.RemoveValue;

                    ledgerAccount.LastTransactionTime = closeTime;

                    nodeState.Ledger[diffData.PublicKey] = ledgerAccount;

                    // This is good enough as we have previously checked for correctness and matching
                    // values in both locations.

                    finalPersistentDBUpdateList.Add(ledgerAccount);
                }

                LedgerCloseData lcd;

                bool ok = nodeState.Persistent.CloseHistory.GetLastRowData(out lcd);

                lcd.CloseTime = closeTime;
                lcd.SequenceNumber++;
                lcd.Transactions = acceptedTransactions.Count;
                lcd.TotalTransactions += lcd.Transactions;
                lcd.LedgerHash = nodeState.Ledger.RootHash.Hex;

                // Apply to persistent DB.

                nodeState.Persistent.CloseHistory.AddUpdate(lcd);
                nodeState.Persistent.AccountStore.AddUpdateBatch(finalPersistentDBUpdateList);
                nodeState.Persistent.TransactionStore.AddUpdateBatch(acceptedTransactions, lcd.SequenceNumber);

                nodeState.NodeInfo.LastLedgerInfo = new JS_LedgerInfo(lcd);
            }
        }
    }
}
