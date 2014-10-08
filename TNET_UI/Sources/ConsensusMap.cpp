
#include"ConsensusMap.h"

#include <iostream>

ConsensusMap::ConsensusMap()
{

}

void ConsensusMap::updateVote(vector<VoteType> votes, Hash publicKey)
{
	for (int i = 0; i < votes.size(); i++)
	{
		Hash TransactionID = votes[i].TransactionID;
		bool vote = votes[i].Vote;

		concurrent_hash_map<Hash, concurrent_hash_map<Hash, bool>>::accessor acc;
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

				std::cout << endl << "-- Insert";
			}
			else
			{
				accinner->second = vote;

				std::cout<< endl  << "-- Update";
			}	
		}

	}
}