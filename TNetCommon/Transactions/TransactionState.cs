
// @Author : Arpan Jati
// @Date: 8th Feb 2015 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Transactions
{
    /// <summary>
    /// Keeps the TransactionState.
    /// </summary>
    public class TransactionState
    {
        public TransactionProcessingResult ProcessingResult = TransactionProcessingResult.Unprocessed;
        public TransactionStatusType StatusType = TransactionStatusType.Unprocessed;
        public DateTime FetchTimeUTC;

        public TransactionState()
        {
            FetchTimeUTC = DateTime.UtcNow;
        }

        public TransactionState(TransactionProcessingResult ProcessingResult)
            : this()
        {
            this.ProcessingResult = ProcessingResult;
        }

        public TransactionState(TransactionStatusType StatusType)
            : this()
        {
            this.StatusType = StatusType;
        }

        public TransactionState(TransactionProcessingResult ProcessingResult, TransactionStatusType StatusType)
            : this()
        {
            this.ProcessingResult = ProcessingResult;
            this.StatusType = StatusType;
        }

        public TransactionState(DateTime FetchTimeUTC, TransactionProcessingResult ProcessingResult, TransactionStatusType StatusType)
        {
            this.ProcessingResult = ProcessingResult;
            this.StatusType = StatusType;
            this.FetchTimeUTC = FetchTimeUTC;
        }
    }
}
