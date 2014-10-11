
#include"ConsensusMap.h"

#include <iostream>

using namespace std;

typedef concurrent_hash_map<Hash, concurrent_hash_map<Hash, bool>> HM;

ConsensusMap::ConsensusMap()
{

}

ConsensusMap::ConsensusMap(IncomingTransactionMap _incomingTransactionMap, Ledger _ledger)
{
	incomingTransactionMap = _incomingTransactionMap;
	ledger = _ledger;
}

//check double spending here
void ConsensusMap::CheckTransactions(vector<Hash>& DoubleSpenderPublicKey, vector<Hash>& DoubleSpendingTransaction)
{
	concurrent_hash_map<Hash, int64_t> SpendMap;

	//given a public key if it is a source of a transactionID
	concurrent_hash_map<Hash, vector<Hash>> PK_ID;

	int64_t TotalTransactionMoney = 0;
	//populate spending
	for (HM::iterator it = consensusVoteMap.begin(); it != consensusVoteMap.end(); ++it)
	{
		Hash transactionID = it->first;
		//check each transaction data
		TransactionContentData TCD;
		if (incomingTransactionMap.GetTransactionContentData(TCD, transactionID))
		{
			TransactionContent TC = TCD.TC;
			//get source information
			vector<TransactionEntity> sources = TC.Sources;
			for (int i = 0; i < (int)sources.size(); i++)
			{
				TransactionEntity TE = sources[i];
				Hash sourcePK = TE.PublicKey;
				int64_t money = TE.Amount;
				
				concurrent_hash_map<Hash, vector<Hash>>::accessor PK_ID_acc;
				if (!PK_ID.find(PK_ID_acc, sourcePK))
				{
					vector<Hash> vc;
					vc.push_back(transactionID);
					PK_ID.insert(make_pair(sourcePK, vc));
				}
				else
				{
					vector<Hash> old_vc = PK_ID_acc->second;
					old_vc.push_back(transactionID);
				}

				TotalTransactionMoney += money;

				concurrent_hash_map<Hash, int64_t> ::accessor acc;
				
				if (!SpendMap.find(acc, sourcePK))
				{
					SpendMap.insert(make_pair(sourcePK, money));
				}

				else
				{
					acc->second += money;
				}
			}
		}
	}

	for (concurrent_hash_map<Hash, int64_t>::iterator it = SpendMap.begin(); it != SpendMap.end(); ++it)
	{
		Hash SpenderpublicKey = it->first;
		int64_t outgoingMoney = it->second;
		int64_t balance = 0;

		if (ledger.GetAccountBalance(SpenderpublicKey, balance))
		{
			if (outgoingMoney > balance)
			{
				DoubleSpenderPublicKey.push_back(SpenderpublicKey);

				concurrent_hash_map<Hash, vector<Hash>>::accessor PK_ID_acc;
				
				if (PK_ID.find(PK_ID_acc, SpenderpublicKey))
				{
					vector<Hash> ToBeCancelledTransactionIDs = PK_ID_acc->second;
					
					for (int i = 0; i < (int)ToBeCancelledTransactionIDs.size(); i++)
					{
						DoubleSpendingTransaction.push_back(ToBeCancelledTransactionIDs[i]);
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


void ConsensusMap::updateVote(vector<VoteType> votes, Hash publicKey)
{
	for (int i = 0; i < (int)votes.size(); i++)
	{
		Hash TransactionID = votes[i].TransactionID;
		bool vote = votes[i].Vote;

		HM::accessor acc;
		if (!consensusVoteMap.find(acc, TransactionID))
		{
			concurrent_hash_map<Hash, bool> hm;
			hm.insert(make_pair(publicKey, vote));

			consensusVoteMap.insert(make_pair(TransactionID, hm));

			//std::cout << endl << "------ Insert";

		}
		else
		{
			//std::cout << endl << "------ Update";

			concurrent_hash_map<Hash, bool> oldHm = acc->second;
			concurrent_hash_map<Hash, bool>::accessor accinner;

			if (!oldHm.find(accinner, publicKey))
			{
				oldHm.insert(make_pair(publicKey, vote));

				//std::cout << endl << "-- Insert";
			}
			else
			{
				accinner->second = vote;

				//std::cout<< endl  << "-- Update";
			}	
		}

	}
}