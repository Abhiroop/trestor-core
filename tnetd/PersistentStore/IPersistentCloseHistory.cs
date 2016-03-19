using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace TNetD.PersistentStore
{
    public delegate void CloseHistoryFetchEventHandler(LedgerCloseData ledgerCloseData);

    interface IPersistentCloseHistory
    {
        bool LCLExists(Hash ledgerHash);

        bool LCLExists(long sequenceNumber);

        DBResponse FetchLCL(out LedgerCloseData ledgerCloseData, Hash ledgerHash);
        
        DBResponse FetchLCL(out LedgerCloseData ledgerCloseData, long sequenceNumber);

        Task FetchAllLCLAsync(CloseHistoryFetchEventHandler closeHistoryFetch);

        int BatchFetch(out Dictionary<Hash, LedgerCloseData> lastClosedLedgers, IEnumerable<Hash> ledgerHashes);

        int AddUpdateBatch(IEnumerable<LedgerCloseData> ledgerCloseData);

        DBResponse AddUpdate(LedgerCloseData ledgerCloseData);

        DBResponse AddUpdate(LedgerCloseData ledgerCloseData, DbTransaction transaction);

        DBResponse Delete(Hash ledgerHash);

        Tuple<DBResponse, long> DeleteEverything();

        bool GetLastRowData(out LedgerCloseData lastCloseData);

    }
}
