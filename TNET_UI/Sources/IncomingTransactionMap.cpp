
#include "IncomingTransactionMap.h"

using namespace std;

typedef concurrent_hash_map<Hash, TransactionContentData> HM;

/*
Given a transactionID returns current associated TransactionContentData, else return false
*/
bool IncomingTransactionMap::GetTransactionContentData(TransactionContentData& transactionContentData, Hash transactionID)
{
	HM::accessor acc;

	if (TransactionMap.find(acc, transactionID))
	{
		transactionContentData = acc->second;
	}

	return false;
}

vector<Hash> IncomingTransactionMap::FetchAllTransactionID()
{
	vector<Hash> toSent;
	HM::iterator it;
	for (it = TransactionMap.begin(); it != TransactionMap.end(); ++it)
	{
		Hash transactionID = it->first;
		toSent.push_back(transactionID);
	}

	return toSent;
}

/*
given a set of transaction IDs get the associated transaction contents
*/
vector<TransactionContent> IncomingTransactionMap::FetchTransactionContent(vector<Hash> differenceTransactionIDs)
{
	vector<TransactionContent> toSent;

	HM::accessor acc;

	for (int i = 0; i < (int)differenceTransactionIDs.size(); i++)
	{
		Hash transactionID = differenceTransactionIDs[i];
		
		if (TransactionMap.find(acc, transactionID))
		{
			TransactionContentData TCD = acc->second;
			TransactionContent TC = TCD.TC;
			toSent.push_back(TC);
		}

	}

	return toSent;
}

bool IncomingTransactionMap::HaveTransactionInfo(Hash transactionID)
{
	HM::accessor ac;
	
	return (TransactionMap.find(ac, transactionID));
}

/*
Given a transaction ID from a validator, upate it in the current transactionMap
*/
void IncomingTransactionMap::UpdateTransactionID(Hash transactionID, Hash forwarderPublicKey)
{
	HM::accessor acc;

	if (TransactionMap.find(acc, transactionID))
	{
		TransactionContentData tcd = acc->second;

		set<Hash> fpk = tcd.ForwardersPK;

		bool exists = false;
		for (set<Hash>::iterator it = fpk.begin(); it != fpk.end(); ++it)
		{
			Hash tmp = *it;
			if (tmp == forwarderPublicKey)
			{
				exists = true;
				break;
			}
		}

		if (!exists)
		{
			fpk.insert(forwarderPublicKey);
		}
	}
}
/*
Given a transaction ID from a validator, insert it in the current transactionMap
*/
void IncomingTransactionMap::InsertTransactionContent(TransactionContent tc, Hash forwarderPublicKey)
{
	//verifye signature
	if (!tc.VerifySignature())
	{
		GlobalBlacklistedValidators.push_back(forwarderPublicKey);
		return;
	}

	//search in the Transaction map to see
	//if the particular transaction ID is in
	//the map. If exists then update. Otherwise
	//ask for the transaction content
	Hash transactionID = tc.GetHash();

	HM::accessor acc;
	if (!TransactionMap.find(acc, transactionID))
	{
		TransactionContentData tcd;

		tcd.ForwardersPK.insert(forwarderPublicKey);
		tcd.TC = tc;
		TransactionMap.insert(make_pair(transactionID, tcd));
	}

	else
	{
		TransactionContentData tcd = acc->second;

		set<Hash> ForwardersPK = tcd.ForwardersPK;

		bool exists = false;
		for (set<Hash>::iterator it = ForwardersPK.begin(); it != ForwardersPK.end(); ++it)
		{
			Hash tmp = *it;
			if (tmp == forwarderPublicKey)
			{
				exists = true;
				break;
			}
		}

		if (!exists)
		{
			ForwardersPK.insert(forwarderPublicKey);
		}
	}

}