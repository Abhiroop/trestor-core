using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.PersistentStore
{
    interface IPersistentCloseHistory
    {
        bool LCLExists(Hash ledgerHash);

        bool LCLExists(long sequenceNumber);

        DBResponse FetchLCL(out LedgerCloseData ledgerCloseData, Hash ledgerHash);

        DBResponse FetchLCL(out LedgerCloseData ledgerCloseData, long sequenceNumber);

        int BatchFetch(out Dictionary<Hash, LedgerCloseData> lastClosedLedgers, IEnumerable<Hash> ledgerHashes);

        int AddUpdateBatch(IEnumerable<LedgerCloseData> ledgerCloseData);

        DBResponse AddUpdate(LedgerCloseData ledgerCloseData);

        DBResponse AddUpdate(LedgerCloseData ledgerCloseData, DbTransaction transaction);

        DBResponse Delete(Hash ledgerHash);

        Tuple<DBResponse, long> DeleteEverything();

        bool GetLastRowData(out LedgerCloseData lastCloseData);


    }
}
