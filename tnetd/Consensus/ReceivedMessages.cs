
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
    class ReceivedMessages
    {
        private int votes, confirmations;

        public int Votes { get { return votes; } }
        public int Confirmations { get { return confirmations; } }

        public ReceivedMessages()
        {
            votes = 0;
            confirmations = 0;
        }

        public void IncrementVotes()
        {
            Interlocked.Increment(ref votes);
        }

        public void IncrementConfirmations()
        {
            Interlocked.Increment(ref confirmations);
        }

        public void ResetVotes()
        {
            Interlocked.Exchange(ref votes, 0);
        }

        public void ResetConfirmations()
        {
            Interlocked.Exchange(ref confirmations, 0);
        }

        public void ResetAll()
        {
            Interlocked.Exchange(ref votes, 0);
            Interlocked.Exchange(ref confirmations, 0);
        }

    }
}
