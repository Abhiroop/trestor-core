using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using TNetD.Nodes;
using TNetD.Transactions;
using TNetD.Ledgers;

namespace TNetD.Consensus
{
    class ConsensusMap
    {
        NodeState nodeState;
        IncomingTransactionMap incomingTransactionMap;
        Ledger ledger;
        
        ConcurrentDictionary<Hash, ConcurrentDictionary<Hash, bool>> consensusVoteMap;
        ConcurrentDictionary<Hash, bool> currentAcceptedSet;

        public ConsensusMap(NodeState nodeState)
        {
            this.nodeState = nodeState;
        }

        public ConsensusMap(NodeState nodeState, IncomingTransactionMap incomingTransactionMap, Ledger ledger)
        {
            this.nodeState = nodeState;
            this.incomingTransactionMap = incomingTransactionMap;
            this.ledger = ledger;
        }

        public List<Hash> GetTransactionsWithThresholdVotes(List<Hash> TrustedVoters)
        {
	        List<Hash> toSent = new List<Hash>();

            IEnumerator<Hash> en = consensusVoteMap.Keys.GetEnumerator();

	        while(en.MoveNext())
            {
                Hash transactionID = en.Current;
                ConcurrentDictionary<Hash, bool> cd = new ConcurrentDictionary<Hash,bool>();
                consensusVoteMap.TryGetValue(transactionID, out cd);

                IEnumerator<Hash> en1 = cd.Keys.GetEnumerator();

                int voteCounter = 0;
                while(en1.MoveNext())
                {
                    Hash k = en1.Current;
                    bool vote = false;
                    cd.TryGetValue(k, out vote);

                    if(vote)
                    {
                        ++voteCounter;
                    }
                }

                float perc = (float)voteCounter / TrustedVoters.Count;
                if (perc * 100 > Constants.CONS_VOTING_ACCEPTANCE_THRESHOLD_PERC)
			        toSent.Add(transactionID);
            }
	        return toSent;
        }

        public ConcurrentDictionary<Hash, TreeDiffData> GetAcceptedAccountDelta()
        {
	        ConcurrentDictionary<Hash, TreeDiffData> accountMoneyFlow = new ConcurrentDictionary<Hash,TreeDiffData>();

	        IEnumerator<Hash> en = currentAcceptedSet.Keys.GetEnumerator();

            while(en.MoveNext())
            {
                Hash transactionID = en.Current;
                bool accepted = false;
                currentAcceptedSet.TryGetValue(transactionID, out accepted);
                    
                if(!accepted)
                    continue;

                Tuple<TransactionContent, bool> tp = incomingTransactionMap.GetTransactionContent(transactionID);
                if(!tp.Item2)
                    continue;

                TransactionContent TC = tp.Item1;
                List<TransactionEntity> sources = TC.Sources;
                List<TransactionEntity> destinations = TC.Destinations;

                //update sources
                for(int i = 0; i < sources.Count; i++)
                {
                    Hash PKsource = new Hash(sources[i].PublicKey);
                    long outMoney = sources[i].Value;

                    if (outMoney <= 0)
				        throw new Exception("Fatal error #1. Appicaion exit");

                    if(accountMoneyFlow.ContainsKey(PKsource))
                    {
                        TreeDiffData TDD;
                        accountMoneyFlow.TryGetValue(PKsource, out TDD);
                        TDD.RemoveValue += outMoney;
                        accountMoneyFlow.TryUpdate(PKsource, TDD, TDD);
                    }

                    else
                    {
                        TreeDiffData TDD = new TreeDiffData(0, outMoney);
                        accountMoneyFlow.TryAdd(PKsource, TDD);
                    }
                }
                //update destinations
                for(int i = 0; i< destinations.Count; i++)
                {
                    Hash PKdestination = new Hash(destinations[i].PublicKey);
                    long inMoney = destinations[i].Value;

			        if (inMoney<=0)
				        throw new Exception("Fatal error #2. Appicaion exit");

                    if(accountMoneyFlow.ContainsKey(PKdestination))
                    {
                        TreeDiffData TDD;
                        accountMoneyFlow.TryGetValue(PKdestination, out TDD);
                        TDD.AddValue += inMoney;
                        accountMoneyFlow.TryUpdate(PKdestination, TDD, TDD);
                    }

                    else
                    {
                        TreeDiffData TDD = new TreeDiffData(inMoney, 0);
                        accountMoneyFlow.TryAdd(PKdestination, TDD);
                    }
                }
            }
	        return accountMoneyFlow;
        }
    }
}
