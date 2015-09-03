using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Consensus
{
    class VoteMap
    {

        Dictionary<Hash, HashSet<Hash>> map = default(Dictionary<Hash, HashSet<Hash>>);

        public VoteMap()
        {
            map = new Dictionary<Hash, HashSet<Hash>>();

        }

        public void Add(Ballot ballot)
        {
           

        }

        public void Reset()
        {
            map.Clear();
        }

    }
}
