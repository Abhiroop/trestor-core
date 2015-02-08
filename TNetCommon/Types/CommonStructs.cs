using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Transactions;

namespace TNetD.Types
{
    public struct DifficultyTimeData
    {
        public int Difficulty;
        public DateTime IssueTime;
        public int MemoryCost;
        public int TimeCost;
        public ProofOfWorkType Type;

        public DifficultyTimeData(int difficulty, DateTime issueTime, int memoryCost, int timeCost, ProofOfWorkType type)
        {
            Difficulty = difficulty;
            IssueTime = issueTime;
            MemoryCost = memoryCost;
            TimeCost = timeCost;
            Type = type;
        }
    }

    public class LedgerCloseData
    {
        // DBUtils.ExecuteNonQuery("CREATE TABLE LedgerInfo (SequenceNumber INTEGER PRIMARY KEY AUTOINCREMENT, 
        //         LedgerHash BLOB, Transactions INTEGER, CloseTime INTEGER);", sqliteConnection);

        public long SequenceNumber;
        public byte[] LedgerHash;
        public long Transactions;
        public long TotalTransactions;
        public long CloseTime;
    }

   

}
