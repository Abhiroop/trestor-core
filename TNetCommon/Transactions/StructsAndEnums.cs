
// @Author : Arpan Jati
// @Date: 23th Dec 2014 | 12th Jan 2015 | 16th Jan 2015 | 20th Jan 2015 | 22nd Jan 2015 
// 9th Feb 2015 : Full transaction overhaul.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Transactions
{
    public enum TransactionStatusType
    {
        Unprocessed = 0x00,

        Proposed = 0x11,
        InPreProcessing = 0x12,
        InProcessingQueue = 0x13,
        VoteInProgress = 0x14,

        Failure = 0x20,

        Processed = 0x40,
        Success = 0x50,
    };

    public enum TransactionProcessingResult
    {
        /// <summary>
        /// The transaction is yet to be processed.
        /// </summary>
        Unprocessed = 0x00,

        /// <summary>
        /// Initial integrity checks passed. Queued for further processing.
        /// </summary>
        Accepted = 0x01,

        /// <summary>
        /// Insufficient funds in sources. This is an integrity check, not considering the actual accounts.
        /// </summary>
        InsufficientFunds = 0x02,

        /// <summary>
        /// The value in sources and destinations don't match.
        /// </summary>
        SourceSinkValueMismatch = 0x03,

        /// <summary>
        /// Invalid signature. Now that's bad !!!
        /// </summary>
        SignatureInvalid = 0x04,

        /// <summary>
        /// All the sources don't have their associated signatures.
        /// </summary>
        InsufficientSignatureCount = 0x05,

        /// <summary>
        /// Insifficient network fees. For now its ZERO.
        /// </summary>
        InsufficientFees = 0x06,

        /// <summary>
        /// Proposal time for the transaction is invalid (Should be in limits).
        /// </summary>
        InvalidTime = 0x07,

        /// <summary>
        /// Invalid Source/Destination Entity or Main/Test Net Mismatch.
        /// </summary>
        InvalidTransactionEntity = 0x08,

        /// <summary>
        /// A Source providing less number of tre's than network minimum for any transaction.
        /// </summary>
        NoProperSources = 0x09,

        /// <summary>
        /// A Destination having less number of tre's than network minimum for any transaction.
        /// </summary>
        NoProperDestinations = 0x0A,

        /// <summary>
        /// Invalid transaction packet version.
        /// </summary>
        InvalidVersion = 0x0B,

        /// <summary>
        /// Invalid execution Data for any transaction.
        /// </summary>
        InvalidExecutionData = 0x0C,

        /// <summary>
        /// The Source entity is also present as one of the Destinations.
        /// </summary>
        SourceDestinationRepeat = 0x0D,

        /// <summary>
        /// Processing Result: Source does not exist.
        /// </summary>
        PR_SourceDoesNotExist = 0x20,

        /// <summary>
        /// Processing Result: Invalid / Banned account name in destination.
        /// </summary>
        PR_BadAccountName = 0x21,

        /// <summary>
        /// Processing Result: Destination account address validation failure.
        /// </summary>
        PR_BadAccountAddress = 0x22,

        /// <summary>
        /// Processing Result: Insufficient amount to create new account.
        /// </summary>
        PR_BadAccountCreationValue = 0x23,

        /// <summary>
        /// Processing Result: Invalid Account state banned/disabled.
        /// </summary>
        PR_BadAccountState = 0x24,

        /// <summary>
        /// Processing Result: Invalid transaction fee.
        /// </summary>
        PR_BadTransactionFee = 0x25,

        /// <summary>
        /// Processing Result: Not enough funds in account or double spending.
        /// </summary>
        PR_BadInsufficientFunds = 0x26,

        /// <summary>
        /// Processing Result: OMG !!! Its all good !!!
        /// </summary>
        PR_Validated = 0x40, // 64 in decimal.

        /// <summary>
        /// Processing Result: The transaction is successfully processed and account balances reflect the result of the transaction.
        /// </summary>
        PR_Success = 0x50, // 80 in decimal.

    };
}
