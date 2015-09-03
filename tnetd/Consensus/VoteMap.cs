using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Consensus
{
    class VoteMap
    {
        // Key: PublicKey
        Dictionary<Hash, Ballot> map = default(Dictionary<Hash, Ballot>);

        public VoteMap()
        {
            map = new Dictionary<Hash, Ballot>();

        }

        /// <summary>
        /// Adds the votes associated to a list. 
        /// Make sure the ballot is valid by verifying signature before calling.
        /// </summary>
        /// <param name="ballot"></param>
        public void AddBallot(Ballot ballot)
        {
            if (map.ContainsKey(ballot.PublicKey))
            {
                // Update
                map[ballot.PublicKey] = ballot;
            }
            else
            {
                // Insert
                map.Add(ballot.PublicKey, ballot);
            }
        }

        public void VerifyAndPullMissingTransactions(Ballot myBallot)
        {
            // TODO : CRITICAL : VERIFY BALLOT : TIMING

            int voteCount = map.Count;

            if (voteCount < Common.VOTE_MIN_VOTERS)
            {
                DisplayUtils.Display("Insufficient number of Voters :" + voteCount, DisplayType.ImportantInfo);
            }
            else
            {
                // Assemble the current transaction set, from the votes.

                Dictionary<Hash, int> txSet = new Dictionary<Hash, int>();

                foreach (var ballotKVP in map)
                {
                    foreach (var txid in ballotKVP.Value.TransactionIds)
                    {
                        if (txSet.ContainsKey(txid))
                        {
                            txSet[txid]++;
                        }
                        else
                        {
                            txSet.Add(txid, 1);
                        }
                    }
                }

                // Now that the transaction set is built, make sure to check if we have all the transactions,
                // If yes, great, else fetch transactions with more than 50 percent of nodes.

                foreach(var tx in txSet)
                {
                    
                    if(!myBallot.TransactionIds.Contains(tx.Key))
                    {
                        // Check percentage

                        //float perc = 

                    }
                    

                }



            }


        }

        public void Reset()
        {
            map.Clear();
        }

    }
}
