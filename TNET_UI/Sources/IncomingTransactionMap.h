
#ifndef IncomingTransactionMap_H
#define IncomingTransactionMap_H


#include "Hash.h"
#include "TransactionContent.h"
#include "tbb\concurrent_hash_map.h"
#include "tbb\concurrent_unordered_set.h"
#include "Constants.h"

using namespace tbb;

class TransactionContentData
{
	public:
	TransactionContent TC;
	hash_set<Hash> ForwardersPK;
};


class IncomingTransactionMap
{
private:
	//[Transaction ID] ->[[Transaction Content] -> [vector of sender address]]
	concurrent_hash_map<Hash, TransactionContentData> TransactionMap;

public:

	bool GetTransactionContentData(TransactionContentData& transactionContentData, Hash transactionID);

	//this method will also check for double spending
	void InsertTransactionContent(TransactionContent tc, Hash forwarderPublicKey);
	void UpdateTransactionID(Hash transactionID, Hash forwarderPublicKey);

	void GetEligibleTransactionForConsensus(vector<Hash> connectedValidators, vector<Hash>& transactionIDtoMigrate);

	void RemoveTransactionsFromTransactionMap(vector<Hash> transactionID);

	vector<TransactionContent> FetchTransactionContent(vector<Hash> differenceTransactionIDs);
	vector<Hash> FetchAllTransactionID();

	bool HaveTransactionInfo(Hash transactionID);
};

#endif