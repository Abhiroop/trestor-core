
#ifndef ConsensusMap_H
#define ConsensusMap_H

#include "tbb\concurrent_hash_map.h"
#include "Utils.h"
#include "VoteType.h"
#include "IncomingTransactionMap.h"
#include "Ledger.h"

using namespace tbb;

class ConsensusMap
{
private:
	IncomingTransactionMap incomingTransactionMap;
	Ledger ledger;

public:
	ConsensusMap();
	ConsensusMap(IncomingTransactionMap incomingTransactionMap, Ledger _ledger);
	// [TransactionID --> (VoterPublicKey --> Vote)]
	concurrent_hash_map<Hash, concurrent_hash_map<Hash, bool>> consensusVoteMap;

	void updateVote(vector<VoteType> votes, Hash publickey);

	vector<Hash> GetTransactionsWithThresholdVotes();

	//public keys which have tried to do double spending

	//transaction IDs which will be given -ve votes as 
	//they have traces of double spending
	void CheckTransactions(vector<Hash>& DoubleSpenderPublicKey, vector<Hash>& DoubleSpendingTransaction);
	
};


#endif