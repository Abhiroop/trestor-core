
#include"ConsensusMap.h"

#include <iostream>

using namespace std;

typedef concurrent_hash_map<Hash, concurrent_hash_map<Hash, bool>> HM;

ConsensusMap::ConsensusMap()
{

}

ConsensusMap::ConsensusMap(IncomingTransactionMap _incomingTransactionMap)
{
	incomingTransactionMap = _incomingTransactionMap;
}

//check double spending here
void ConsensusMap::CheckTransactions()
{
	hash_map<Hash, int64_t> spend;
	for (HM::iterator it = consensusVoteMap.begin(); it != consensusVoteMap.end(); ++it)
	{
		Hash transactionID = it->first;
		TransactionContentData TCD;
		if (incomingTransactionMap.GetTransactionContentData(TCD, transactionID))
		{
			TransactionContent TC = TCD.TC;
			//TC.
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