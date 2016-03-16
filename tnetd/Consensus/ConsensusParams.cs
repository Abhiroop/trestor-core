using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Consensus
{    
    class ConsensusParams
    {
        public LedgerCloseSequence LCS { get; set; }

        public ConsensusStates ConsensusState = ConsensusStates.Sync;

        public VotingStates VotingState = VotingStates.STNone;
        
        public ConsensusParams()
        {

        }
    }
}
