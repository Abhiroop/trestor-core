using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Types
{
    public struct DifficultyTimeData
    {
        public int Difficulty;
        public DateTime IssueTime;
        public DifficultyTimeData(int difficulty, DateTime issueTime)
        {
            Difficulty = difficulty;
            IssueTime = issueTime;
        }
    }
}
