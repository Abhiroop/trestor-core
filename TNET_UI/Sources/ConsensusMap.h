
#ifndef ConsensusMap_H
#define ConsensusMap_H

#include "tbb\concurrent_hash_map.h"
#include "Utils.h"
#include "VoteType.h"

using namespace tbb;

class ConsensusMap
{
public:
	ConsensusMap();

	// [TransactionID --> (VoterPublicKey --> Vote)]
	concurrent_hash_map<Hash, concurrent_hash_map<Hash, bool>> consensusVoteMap;

	void updateVote(vector<VoteType> votes, Hash publickey);
};


#endif