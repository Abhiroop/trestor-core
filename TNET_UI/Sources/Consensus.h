
#ifndef Consensus_H
#define Consensus_H

#include "TransactionSetType.h"
#include "TransactionContent.h"
#include "Ledger.h"
#include "Utils.h"

class Consensus
{
	//what to load?
	//LCL, list of validators

private:
	
	vector<Hash> ValidatorPublickey;
	shared_ptr<Ledger> Ledger;

public:
	
	void InsertValidatedTransactions(vector<TransactionContent> Transactions);
	void PublishIDsToVallidators();
	
	vector<Hash> GetTransactionIDsInSet();
	vector<TransactionContent> GetTransactionsForID(vector<Hash> ID, Hash ValidatorPublickey);
	
	// Item may delete here if one transaction in a chein is found to be double spending
	void CheckTransactionChain();
	void RefreshVotes();


};



#endif