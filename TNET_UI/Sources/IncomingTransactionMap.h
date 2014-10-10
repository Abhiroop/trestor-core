
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
	set<Hash> ForwardersPK;
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

	vector<Hash> FetchAllTransactionID();

	bool HaveTransactionInfo(Hash transactionID);
};

#endif