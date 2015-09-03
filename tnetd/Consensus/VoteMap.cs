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
        
        /// <summary>
        /// Returns a Dictionary, Key is PublicKey, values are a set of required Transactions.
        /// </summary>
        /// <param name="myBallot"></param>
        /// <param name="missingTransactions"></param>
        public void GetMissingTransactions(Ballot myBallot, out Dictionary<Hash,HashSet<Hash>> missingTransactions)
        {
            missingTransactions = new Dictionary<Hash, HashSet<Hash>>();

            // TODO : CRITICAL : VERIFY BALLOT : TIMING

            int votersCount = map.Count;

            if (votersCount < Common.VOTE_MIN_VOTERS)
            {
                DisplayUtils.Display("Insufficient number of Voters :" + votersCount, DisplayType.ImportantInfo);
            }
            else
            {
                // Assemble the current transaction set, from the votes.

                // Key is TxID, Value id the list of PK's
                Dictionary<Hash, HashSet<Hash>> txSet = new Dictionary<Hash, HashSet<Hash>>();

                foreach (var ballotKVP in map)
                {
                    foreach (var txid in ballotKVP.Value.TransactionIds)
                    {
                        if (txSet.ContainsKey(txid))
                        {
                            txSet[txid].Add(ballotKVP.Key);
                        }
                        else
                        {
                            HashSet<Hash> pkList = new HashSet<Hash>();
                            pkList.Add(ballotKVP.Key);

                            txSet.Add(txid, pkList);
                        }
                    }
                }

                // Now that the transaction set is built, make sure to check if we have all the transactions,
                // If yes, great, else fetch transactions with more than 50 percent of nodes.

                // CRITICAL: ADD SUPPORT FOR -ve votes.

                foreach(var tx in txSet)
                {                    
                    if(!myBallot.TransactionIds.Contains(tx.Key))
                    {
                        // Check percentage

                        // Number of +ve Voters
                        float perc = (tx.Value.Count * 100.0F) / votersCount;

                        if(perc >= Common.VOTE_VOTE_STAGE_FETCH_THRESHOLD_PERC)
                        {
                            // Fetch the transcation and update ballot.

                            Hash txID = tx.Key;
                            Hash pk = tx.Value.ElementAt(0);

                            if (missingTransactions.ContainsKey(pk))
                            {
                                missingTransactions[pk].Add(txID);
                            }
                            else
                            {
                                HashSet<Hash> txids = new HashSet<Hash>();
                                txids.Add(txID);

                                missingTransactions.Add(pk, txids);
                            }                            
                        }
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
