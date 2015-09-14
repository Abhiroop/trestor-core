
//
//  @Author: Arpan Jati
//  @Date: 4th September 2015 
//

using System;
using System.Collections.Generic;
using System.Linq;
using TNetD.Nodes;

namespace TNetD.Consensus
{
    class VoteMap
    {
        NodeConfig nodeConfig;
        NodeState nodeState;

        // Key: PublicKey
        Dictionary<Hash, Ballot> map = default(Dictionary<Hash, Ballot>);

        public VoteMap(NodeConfig nodeConfig, NodeState nodeState)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;

            map = new Dictionary<Hash, Ballot>();
        }

        /// <summary>
        /// Adds the votes associated to a list. 
        /// Make sure the ballot is valid by verifying signature before calling.
        /// </summary>
        /// <param name="ballot"></param>
        public void AddBallot(Ballot ballot)
        {
            if (map == null)
                DisplayUtils.Display("VERY_BAD_02", DisplayType.Warning);

            if (ballot == null)
                DisplayUtils.Display("VERY_BAD_03", DisplayType.Warning);

            if (ballot.PublicKey == null)
                DisplayUtils.Display("VERY_BAD_04", DisplayType.Warning);

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
        public void GetMissingTransactions(Ballot myBallot, out Dictionary<Hash, HashSet<Hash>> missingTransactions)
        {
            missingTransactions = new Dictionary<Hash, HashSet<Hash>>();

            // TODO : CRITICAL : VERIFY BALLOT : TIMING

            int votersCount = map.Count;

            if (votersCount < Constants.VOTE_MIN_VOTERS)
            {
                DisplayUtils.Display(nodeConfig.ID() + " Insufficient number of Voters :" + votersCount, DisplayType.ImportantInfo);
            }
            else
            {
                Dictionary<Hash, HashSet<Hash>> voteSet = AssembleVotes();

                // Now that the transaction set is built, make sure to check if we have all the transactions,
                // If yes, great, else fetch transactions with more than 50 percent of nodes.

                // CRITICAL: ADD SUPPORT FOR -ve votes.

                foreach (var vote in voteSet)
                {
                    if (!myBallot.Contains(vote.Key))
                    {
                        // Check percentage

                        // Number of +ve Voters
                        float perc = (vote.Value.Count * 100.0F) / votersCount;

                        if (perc >= Constants.CONS_VOTE_STAGE_FETCH_THRESHOLD_PERC)
                        {
                            // Fetch the transcation and update ballot.

                            Hash txID = vote.Key;
                            Hash pk = vote.Value.ElementAt(0);

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

        /// <summary>
        /// Assemble the current transaction set, from the votes.
        /// Key: TxID, | Values: Voter PublicKeys
        /// </summary>
        /// <returns></returns>
        public Dictionary<Hash, HashSet<Hash>> AssembleVotes()
        {
            // Key is TxID, Value id the list of PK's
            Dictionary<Hash, HashSet<Hash>> voteSet = new Dictionary<Hash, HashSet<Hash>>();

            foreach (var ballotKVP in map)
            {
                foreach (var txid in ballotKVP.Value)
                {
                    if (voteSet.ContainsKey(txid))
                    {
                        voteSet[txid].Add(ballotKVP.Key);
                    }
                    else
                    {
                        HashSet<Hash> pkList = new HashSet<Hash>();
                        pkList.Add(ballotKVP.Key);

                        voteSet.Add(txid, pkList);
                    }
                }
            }

            return voteSet;
        }

        /// <summary>
        /// Used to create the final Ballot.
        /// </summary>
        /// <param name="myBallot"></param>
        /// <param name="Percentage"></param>
        /// <param name="transactionIDs"></param>
        public SortedSet<Hash> FilterTransactionsByVotes(Ballot myBallot, float percentage)
        {
            SortedSet<Hash> transactionIDs = new SortedSet<Hash>();

            int votersCount = map.Count;

            if (votersCount < Constants.VOTE_MIN_VOTERS)
            {
                DisplayUtils.Display(nodeConfig.ID() + " Insufficient number of Voters :" + votersCount, DisplayType.ImportantInfo);
            }
            else
            {
                Dictionary<Hash, HashSet<Hash>> voteSet = AssembleVotes();

                // CRITICAL: ADD SUPPORT FOR -ve votes.

                foreach (var vote in voteSet)
                {
                    if (myBallot.Contains(vote.Key))
                    {
                        // Check percentage

                        // Number of +ve Voters
                        float perc = (vote.Value.Count * 100.0F) / votersCount;

                        if (perc >= percentage)
                        {
                            // Fetch the transcation and update ballot.

                            Hash txID = vote.Key;
                            transactionIDs.Add(txID);
                        }
                    }
                }
            }

            return transactionIDs;
        }

        /// <summary>
        /// Checks if both Ballots have same transactions.
        /// TODO: USE a HASH based version (THINK !)
        /// It will return true for empty ballots :)
        /// </summary>
        /// <param name="myBallot"></param>
        /// <param name="peerBallot"></param>
        /// <returns></returns>
        public bool CheckVoterSyncState(Ballot myBallot, Ballot peerBallot)
        {
            if (myBallot.LedgerCloseSequence != peerBallot.LedgerCloseSequence)
            {
                return false;
            }

            if (myBallot.TransactionCount != peerBallot.TransactionCount)
            {
                return false;
            }

            // Critical: Time Sync

            foreach (var txID in myBallot)
            {
                if (!peerBallot.Contains(txID))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Calculates a list of voters who have the exact same ballot as us.
        /// CRITICAL: Make sure that all the voters have good finalBallot beforehand.
        /// </summary>
        /// <param name="finalBallot"></param>
        /// <returns></returns>
        public HashSet<Hash> GetSynchronisedVoters(Ballot finalBallot)
        {
            HashSet<Hash> synchronisedVoters = new HashSet<Hash>();

            foreach (var voter in map)
            {
                if (CheckVoterSyncState(finalBallot, voter.Value))
                {
                    synchronisedVoters.Add(voter.Key);
                }
            }

            return synchronisedVoters;
        }

        public void Reset()
        {
            map.Clear();
        }




    }
}
