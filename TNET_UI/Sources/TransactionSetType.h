#ifndef TransactionSetType_H
#define TransactionSetType_H


#include "HashTree.h"
#include "Utils.h"
#include "TransactionIDInfo.h"

class TransactionSetType : public LeafDataType
{
public:

	Hash PublicKey;
	vector<TransactionIDInfo> TranIDinfo;

	TransactionSetType();
	TransactionSetType(Hash publicKey, vector<TransactionIDInfo> tranIDinfo);

	Hash GetID();
	Hash GetHash();
};


#endif