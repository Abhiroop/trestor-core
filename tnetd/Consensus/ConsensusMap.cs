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

        public void ConsolidateToLedger(ConcurrentDictionary<Hash, TreeDiffData> delta)
        {
	        //ACID property

            IEnumerator<Hash> it = delta.Keys.GetEnumerator();
	        while(it.MoveNext())
	        {
		        Hash accountID = it.Current;
		        TreeDiffData MIOF = new TreeDiffData ();
                delta.TryGetValue(accountID, out MIOF);

		        if (!ledger.AccountExists(accountID))
			        throw new Exception("Fatal error #3. Appicaion exit");

                AccountInfo accountInfo = ledger[accountID];
		        long inMoney = MIOF.AddValue;
		        long outMoney = MIOF.RemoveValue;
		
		        if (outMoney > accountInfo.Money)
			    throw new Exception("Fatal error #4. Appicaion exit");
	        }

            it.Reset();

	        while (it.MoveNext())
	        {
		        Hash accountID = it.Current;
                TreeDiffData MIOF = new TreeDiffData();
                delta.TryGetValue(accountID, out MIOF);
		
		        AccountInfo accountInfo = ledger[accountID];

		        long inMoney = MIOF.AddValue;
		        long outMoney = MIOF.RemoveValue;

		        accountInfo.Money += (inMoney - outMoney);
	         }
        }

        
        //check double spending here
        void CheckTransactions(ref List<Hash> DoubleSpenderPublicKey, ref List<Hash> DoubleSpendingTransaction)
        {
	        ConcurrentDictionary<Hash, Int64> SpendMap = new ConcurrentDictionary<Hash,long>();

	        //given a public key if it is a source of a transactionID
	        ConcurrentDictionary<Hash, List<Hash>> PK_ID = new ConcurrentDictionary<Hash,List<Hash>>();

        	Int64 TotalTransactionMoney = 0;

            IEnumerator<Hash> it = consensusVoteMap.Keys.GetEnumerator();
	        //populate spending
	        while (it.MoveNext())
	        {
		        Hash transactionID = it.Current;
		        //check each transaction data
		        TransactionContentData TCD;
                Tuple<TransactionContent, bool> tp = incomingTransactionMap.GetTransactionContent(transactionID);
		        if (tp.Item2)
		        {
			        TransactionContent TC = tp.Item1;
			        //get source information
			        List<TransactionEntity> sources = TC.Sources;
			        for (int i = 0; i < sources.Count; i++)
			        {
				        TransactionEntity TE = sources[i];
				        Hash sourcePK = new Hash(TE.PublicKey);
				        Int64 money = TE.Value;
			
				        if (!PK_ID.ContainsKey(sourcePK))
				        {
					        List<Hash> vc = new List<Hash>();
					        vc.Add(transactionID);
					        PK_ID.TryAdd(sourcePK, vc);
				        }
				        else
				        {
					        List<Hash> old_vc = new List<Hash>();
                            PK_ID.TryGetValue(sourcePK, out old_vc);
					        old_vc.Add(transactionID);
                            PK_ID.TryUpdate(sourcePK, old_vc, old_vc);
				        }

				        TotalTransactionMoney += money;
				    
				        if (!SpendMap.ContainsKey(sourcePK))
				        {
					        SpendMap.TryAdd(sourcePK, money);
				        }

				        else
				        {
                            long val = 0;
                            SpendMap.TryGetValue(sourcePK, out val);
					        val += money;
                            SpendMap.TryUpdate(sourcePK, val, val);
				        }
			        }
		        }
	        }

            it.Reset();

            it = SpendMap.Keys.GetEnumerator();
	        
            while (it.MoveNext())
	        {
		        Hash SpenderpublicKey = it.Current;
		        Int64 outgoingMoney = 0;
                SpendMap.TryGetValue(SpenderpublicKey, out outgoingMoney);

		         if (ledger.AccountExists(SpenderpublicKey))
		         {
                     long balance = ledger[SpenderpublicKey].Money;
			         if (outgoingMoney > balance)
			         {
				        DoubleSpenderPublicKey.Add(SpenderpublicKey);
			
			    	    if (PK_ID.ContainsKey(SpenderpublicKey))
				        {
					        List<Hash> ToBeCancelledTransactionIDs = new List<Hash>();
                            PK_ID.TryGetValue(SpenderpublicKey, out ToBeCancelledTransactionIDs);
					
					        for (int i = 0; i < (int)ToBeCancelledTransactionIDs.Count; i++)
					        {
						        DoubleSpendingTransaction.Add(ToBeCancelledTransactionIDs[i]);
					        }
				        }
			        }
		        }
		        else
		        {
			        //The ledger does not have this account info
		        }
	        }
        }

        public void updateVote(List<VoteType> votes, Hash publicKey)
        {
	        for (int i = 0; i < (int)votes.Count; i++)
	        {
		        Hash TransactionID = votes[i].TransactionID;
		        bool vote = votes[i].Vote;

		        if (!consensusVoteMap.ContainsKey(TransactionID))
		        {
                    ConcurrentDictionary<Hash, bool> val = new ConcurrentDictionary<Hash,bool>();
			        val.TryAdd(publicKey, vote);

			        consensusVoteMap.TryAdd(TransactionID, val);
			        //std::cout << endl << "------ Insert";
		        }
		        else
		        {
			        //std::cout << endl << "------ Update";

			        ConcurrentDictionary<Hash, bool> oldHm = new ConcurrentDictionary<Hash,bool>();
                    consensusVoteMap.TryGetValue(publicKey, out oldHm);

			        if (!oldHm.ContainsKey(publicKey))
			        {
				        oldHm.TryAdd(publicKey, vote);
				        //std::cout << endl << "-- Insert";
			        }
			        else
			        {
                        oldHm.TryUpdate(publicKey, vote, vote);
                        //std::cout<< endl  << "-- Update";
			        }	
		        }
	        }
        }
    }
}
