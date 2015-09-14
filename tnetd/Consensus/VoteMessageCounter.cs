
//
//  @Author: Arpan Jati
//  @Date: September 2015 
//

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace TNetD.Consensus
{
    /// <summary>
    /// A thread safe class for managing received message counts.
    /// </summary>
    class VoteMessageCounter
    {
        object vmcLock = new object();

        HashSet<Hash> uniqueResponders = new HashSet<Hash>();

        //private int previousVotes;
        private int previousConfirmations;

        private int votes;
        private int confirmations;

        public int Votes { get { return votes; } }
        public int Confirmations { get { return confirmations; } }

        public int UniqueVoteResponders { get { return uniqueResponders.Count; } }
        public int PreviousConfirmations { get { return previousConfirmations; } }

        public VoteMessageCounter()
        {
            //votes = 0;
            confirmations = 0;
            // previousVotes = 4;
            previousConfirmations = 4;
        }

        /// <summary>
        /// Thread Safe
        /// </summary>
        public void IncrementVotes()
        {
            Interlocked.Increment(ref votes);
        }

        /// <summary>
        /// Thread Safe
        /// </summary>
        public void IncrementConfirmations()
        {
            Interlocked.Increment(ref confirmations);
        }

        /// <summary>
        /// Thread Safe
        /// </summary>
        public void ResetVotes()
        {
            Interlocked.Exchange(ref votes, 0);
        }

        public void ResetUniqueVoters()
        {
            lock (vmcLock)
            {
                uniqueResponders.Clear();
            }
        }

        /// <summary>
        /// Thread Safe
        /// </summary>
        public void ResetConfirmations()
        {
            Interlocked.Exchange(ref confirmations, 0);
        }

        /*public void ResetAll()
        {
            Interlocked.Exchange(ref votes, 0);
            Interlocked.Exchange(ref confirmations, 0);
            //previousVotes = 0;
            previousConfirmations = 0;
        }*/

        public void UpdateVoters(Hash publicKey)
        {
            lock (vmcLock)
            {
                if (!uniqueResponders.Contains(publicKey))
                {
                    uniqueResponders.Add(publicKey);
                }
            }
        }

        public void SetPreviousConfirmations()
        {
            this.previousConfirmations = confirmations;
        }
    }
}
