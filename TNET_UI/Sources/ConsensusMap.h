
#ifndef ConsensusMap_H
#define ConsensusMap_H

#include "tbb\concurrent_hash_map.h"
#include "Utils.h"
#include "VoteType.h"
#include "IncomingTransactionMap.h"

using namespace tbb;

class ConsensusMap
{
private:
	IncomingTransactionMap incomingTransactionMap;

public:
	ConsensusMap();
	ConsensusMap(IncomingTransactionMap incomingTransactionMap);
	// [TransactionID --> (VoterPublicKey --> Vote)]
	concurrent_hash_map<Hash, concurrent_hash_map<Hash, bool>> consensusVoteMap;

	void updateVote(vector<VoteType> votes, Hash publickey);

	void CheckTransactions();

	
};


#endif