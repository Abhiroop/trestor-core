using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Consensus.FailureHandler
{
    class FailureHandlerFactory<K, V, Z> where V : struct, IConvertible
                                         where Z : struct, IConvertible
    {
       public FailureHandler<K, V, Z> GetHandler(ConsensusStates currentConsensusState, VotingStates currentVotingState)
        {
            switch(currentConsensusState)
            {
                case ConsensusStates.Sync: return new SyncFailureHandler<K, V, Z>();
                case ConsensusStates.Merge: return new MergeFailureHandler<K, V, Z>();
                case ConsensusStates.Vote: return new VotingFailureHandler<K, V, Z>(currentVotingState);
            }
            return new DefaultFailureHandler<K,V,Z>();
        }
    }
}
