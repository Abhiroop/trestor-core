

#ifndef TransactionIDInfo_H
#define TransactionIDInfo_H


#include "HashTree.h"
#include "Utils.h"

class TransactionIDInfo
{
public:

	Hash TransactionID;
	vector<Hash> VoterPublickeys;
	bool IsMine;

	TransactionIDInfo();
	TransactionIDInfo(Hash transactionID, vector<Hash> voterPublickeys, bool isMine);
};


#endif