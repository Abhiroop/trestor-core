
//
//  @Author: Arpan Jati
//  @Date: September 2015 
//

using System.Threading;

namespace TNetD.Consensus
{
    /// <summary>
    /// A thread safe class for managing received message counts.
    /// </summary>
    class VoteMessageCounter
    {
        private int previousVotes, previousConfirmations;

        private int votes, confirmations;

        public int Votes { get { return votes; } }
        public int Confirmations { get { return confirmations; } }

        public int PreviousVotes { get { return previousVotes; } }
        public int PreviousConfirmations { get { return previousConfirmations; } }

        public VoteMessageCounter()
        {
            votes = 0;
            confirmations = 0;
            previousVotes = 4;
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

        /// <summary>
        /// Thread Safe
        /// </summary>
        public void ResetConfirmations()
        {
            Interlocked.Exchange(ref confirmations, 0);
        }

        public void ResetAll()
        {
            Interlocked.Exchange(ref votes, 0);
            Interlocked.Exchange(ref confirmations, 0);
            previousVotes = 0;
            previousConfirmations = 0;
        }

        public void SetPreviousVotes()
        {
            this.previousVotes = votes;
        }

        public void SetPreviousConfirmations()
        {
            this.previousConfirmations = confirmations;
        }
    }
}
