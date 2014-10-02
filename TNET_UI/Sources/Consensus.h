
#ifndef Consensus_H
#define Consensus_H

#include "TransactionSetType.h"
#include "TransactionContent.h"
#include "Ledger.h"
#include "Utils.h"
#include "ProtocolPackager.h"
#include "HashTree.h"
#include "TransactionIDInfo.h"
#include <hash_set>
#include "VoteType.h"
#include "tbb\concurrent_hash_map.h"
#include "tbb\concurrent_queue.h"

using namespace tbb;

class Consensus
{
	//what to load?
	//LCL, list of validators

private:
	
	vector<Hash> ValidatorPublickey;
	shared_ptr<Ledger> Ledger;
	//HashTree<TransactionSetType > TranasctionSetTree;
	concurrent_hash_map<Hash, bool> forwardedTransaction;
	concurrent_hash_map<Hash, TransactionContent> presentTransaction;

	concurrent_queue<Hash> transactionContentRequest;

	//Transaction ID -> [list of incoming votes]
	//[list of incoming votes] : sender -> vote
	concurrent_hash_map<Hash, concurrent_hash_map<Hash, bool>> IncomingVotes;

public:
	vector<Hash> publishableID;

	bool AddUpdateTransactionTree(TransactionSetType transactionSet);
	bool CheckSingleTransaction(TransactionSetType transactionSet);
	void InsertValidatedTransactions(vector<TransactionContent> Transactions);
	void PublishIDsToVallidatorsForVote();
	void PublishIDsToVallidatorsInitial();

	void GetTransactionIDsInSet();
	vector<TransactionContent> GetTransactionsForID(vector<Hash> ID, Hash ValidatorPublickey);
	
	// Item may delete here if one transaction in a chein is found to be double spending
	void CheckTransactionChain();
	void RefreshVotes();


};



#endif