
#ifndef ConsensusMap_H
#define ConsensusMap_H

#include "tbb\concurrent_hash_map.h"
#include "Utils.h"
#include "VoteType.h"
#include "IncomingTransactionMap.h"
#include "Ledger.h"
#include "Hash.h"
#include "MoneyInOutFlow.h"
#include "State.h"

using namespace tbb;

class ConsensusMap
{
private:

	State state;
	IncomingTransactionMap incomingTransactionMap;

	Ledger ledger;

public:
	ConsensusMap();
	ConsensusMap(State _state, IncomingTransactionMap incomingTransactionMap, Ledger _ledger);
	// [TransactionID --> (VoterPublicKey --> Vote)]
	concurrent_hash_map<Hash, concurrent_hash_map<Hash, bool>> consensusVoteMap;
	concurrent_hash_map<Hash, bool> currentAcceptedSet;

	void updateVote(vector<VoteType> votes, Hash publickey);

	vector<Hash> GetTransactionsWithThresholdVotes(vector<Hash> connectedUsers);
	//public keys which have tried to do double spending

	concurrent_hash_map<Hash, MoneyInOutFlow> GetAcceptedAccountDelta();
	//transaction IDs which will be given -ve votes as 
	//they have traces of double spending
	void CheckTransactions(vector<Hash>& DoubleSpenderPublicKey, vector<Hash>& DoubleSpendingTransaction);

	void ConsolidateToLedger(concurrent_hash_map<Hash, MoneyInOutFlow> delta);

	void DoEvents();
};


#endif