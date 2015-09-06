//
//  @Author: Arpan Jati
//  @Date: 7th Sept 2015 
// 

using System;
using System.Collections.Generic;
using System.Linq;

namespace TNetD.Consensus
{
    class FinalVoters
    {
        HashSet<Hash> voters;

        long ledgerCloseSequence = 0;

        public FinalVoters()
        {
            ledgerCloseSequence = 0;
            voters = new HashSet<Hash>();
        }

        /// <summary>
        /// Sets LedgerCloseSequence
        /// </summary>
        /// <param name="ledgerCloseSequence"></param>
        public void SetLCS(long ledgerCloseSequence)
        {
            this.ledgerCloseSequence = ledgerCloseSequence;
        }

        public HashSet<Hash> Voters
        {
            get
            {
                return voters;
            }
        }

        public bool Add(Hash voter, long ledgerCloseSequence)
        {
            if(this.ledgerCloseSequence == ledgerCloseSequence)
            {
                if (voter != null)
                {
                    if (!voters.Contains(voter))
                    {
                        if (voter.Hex.Length == Common.KEYLEN_PUBLIC)
                        {
                            voters.Add(voter); // SUPER !
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
            voters.Clear();
            ledgerCloseSequence = 0;
        }

        public void Reset(long ledgerCloseSequence)
        {
            voters.Clear();
            this.ledgerCloseSequence = ledgerCloseSequence;
        }
    }
}
