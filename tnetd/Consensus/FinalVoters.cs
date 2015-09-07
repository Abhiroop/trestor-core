//
//  @Author: Arpan Jati
//  @Date: 7th Sept 2015 
// 

using System;
using System.Collections.Generic;

namespace TNetD.Consensus
{
    class FinalVoters
    {
        public HashSet<Hash> Voters { get; }

        long ledgerCloseSequence = 0;

        public FinalVoters()
        {
            ledgerCloseSequence = 0;
            Voters = new HashSet<Hash>();
        }

        /// <summary>
        /// Sets LedgerCloseSequence
        /// </summary>
        /// <param name="ledgerCloseSequence"></param>
        public void SetLCS(long ledgerCloseSequence)
        {
            this.ledgerCloseSequence = ledgerCloseSequence;
        }

        public bool Add(Hash voter, long ledgerCloseSequence)
        {
            if(this.ledgerCloseSequence == ledgerCloseSequence)
            {
                if (voter != null)
                {
                    if (!Voters.Contains(voter))
                    {
                        if (voter.Hex.Length == Common.KEYLEN_PUBLIC)
                        {
                            Voters.Add(voter); // SUPER !
                        }
                        else
                        {
                            DisplayUtils.Display("FV_ADD:VERY_BAD_02", DisplayType.Warning);
                        }
                    }                    
                }
                else
                {
                    DisplayUtils.Display("FV_ADD:VERY_BAD_01", DisplayType.Warning);
                }                
            }

            return false;
        }

        public void Reset()
        {
            Voters.Clear();
            ledgerCloseSequence = 0;
        }

        public void Reset(long ledgerCloseSequence)
        {
            Voters.Clear();
            this.ledgerCloseSequence = ledgerCloseSequence;
        }
    }
}
